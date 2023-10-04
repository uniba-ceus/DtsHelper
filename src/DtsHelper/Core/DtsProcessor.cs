// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using DtsHelper.Common;
    using DtsHelper.Configuration;
    using DtsHelper.Resources;
    using log4net;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask;
    using PoorMansTSqlFormatterLib.Formatters;

    /// <summary>
    /// Processes all dtsx files and applies changes.
    /// </summary>
    /// <remarks>
    /// Note for structure:
    /// 1. level: variant folder =&gt; equals Namespace.
    /// 2. level: sub directory =&gt; equals package name.
    /// 3. level: sql file name =&gt; equals SqlTask name or Variable name.
    /// </remarks>
    public class DtsProcessor
    {
        private const string ConnectionManagerFileFilterRegex = "[.]conmgr$";
        private const string PackageFileFilterRegex = "[.]dtsx$";
        private const string ProjectParamsFileName = "Project.params";
        private static readonly ILog _log = LogManager.GetLogger(typeof(DtsProcessor));
        private readonly Config _config;
        private readonly Application _dtsApp;
        private List<PackageContext> _packageContexts;
        private List<string> _sqlFiles;
        private List<string> _conmgrFiles;
        private List<string> _dtsxFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="DtsProcessor" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public DtsProcessor(Config config)
        {
            _config = config;
            _dtsApp = new Application();
        }

        /// <summary>
        /// Changes the name of the application in database connections.
        /// </summary>
        public void ChangeAppName()
        {
            Trace.TraceInformation("ChangeAppName - BEGIN");

            var conmgrFileNames = FindConmgrFiles();
            foreach (var conmgrFileName in conmgrFileNames)
            {
                var hasChanged = false;

                var doc = new XmlDocument();
                using (var reader = new XmlTextReader(conmgrFileName))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.XmlDeclaration:
                                doc.AppendChild(doc.ReadNode(reader) ?? throw new InvalidOperationException());
                                break;
                            case XmlNodeType.Element:

                                var node = doc.ReadNode(reader);

                                var pgk = new Package();
                                var conmgr = pgk.Connections.Add("ADO.NET");
                                conmgr.LoadFromXML(node, null);

                                var props = conmgr.ConnectionString.Split(';');
                                var newConnectionString = "";
                                foreach (var prop in props)
                                {
                                    if (!string.IsNullOrEmpty(prop))
                                    {
                                        if (prop.StartsWith("Application Name="))
                                        {
                                            var appNameProps = prop.Split('=');
                                            if (appNameProps.Length == 2)
                                            {
                                                if (appNameProps[1] != _config.Settings.DtsAppName)
                                                {
                                                    newConnectionString +=
                                                        "Application Name=" + _config.Settings.DtsAppName + ";";
                                                    hasChanged = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            newConnectionString += prop + ";";
                                        }
                                    }
                                }

                                if (hasChanged)
                                {
                                    conmgr.ConnectionString = newConnectionString;

                                    // create new connection manager file
                                    var tmpDoc = new XmlDocument();
                                    conmgr.SaveToXML(ref tmpDoc, null, null);

                                    // reference new connection manager file in doc
                                    var tmpNode = doc.ImportNode(tmpDoc.DocumentElement ?? throw new InvalidOperationException(), true);
                                    doc.AppendChild(tmpNode);
                                }

                                break;
                        }
                    }
                }

                if (hasChanged)
                {
                    doc.Save(conmgrFileName);
                }
            }

            Trace.TraceInformation("ChangeAppName - END");
        }

        /// <summary>
        /// Check whether project contains a parameter for switching variant.
        /// </summary>
        /// <returns>True in case the switch parameter exists.</returns>
        public bool CheckProjectContainsSwitchParam()
        {
            Trace.TraceInformation("CheckProjectContainsSwitchParam - BEGIN");

            var switchParam = LoadProjectParams().Any(p => p.Name.Equals(_config.Settings.DtsSwitchParam));

            Trace.TraceInformation("CheckProjectContainsSwitchParam - END");
            return switchParam;
        }

        /// <summary>
        /// Converts all sql files from ASCII/ANSI/Codepage 1252 to UTF-8 with BOM.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void ConvertToUTF8(Config config)
        {
            Trace.TraceInformation("ConvertToUTF8 - BEGIN");

            var sqlFiles = FindSqlFiles();
            foreach (var sqlFilePath in sqlFiles)
            {
                FileUtils.ConvertFileEncodingToUTF8(sqlFilePath);
            }

            Trace.TraceInformation("ConvertToUTF8 - END");
        }

        /// <summary>
        /// Creates for each SqlTask variables for each variant. Namespace will be determined from directory name.
        /// </summary>
        /// <remarks>
        /// SwitchParam determines which SSIS parameter will be used for identification of variable. If not SwitchParam is
        /// defined a generic one will be used.
        /// </remarks>
        public void CreateVariables()
        {
            Trace.TraceInformation("CreateVariables - BEGIN");

            string switchParam;
            if (CheckProjectContainsSwitchParam())
            {
                switchParam = $"@[$Project::{_config.Settings.DtsSwitchParam}]";
            }
            else if (_config.Settings.DtsNamespaceDirectories.Length == 1)
            {
                // in case no switch param is set use default namespace
                switchParam = $"\"{_config.Settings.DtsNamespaceDirectories[0]}\"";
            }
            else
            {
                throw new InvalidOperationException(new StackTrace().GetFrame(0).GetMethod().Name +
                                                    ": Für die Variablen-Expression ist kein Switch-Parameter definiert. Es existieren aber mehr als 1 Namespace.");
            }

            foreach (var packageContext in _packageContexts)
            {
                var package = packageContext.PackageXML;
                _log.Info("CreateVariables: Paket '" + packageContext.CanonicalPath + "' wird bearbeitet.");

                var taskHosts = GetTaskHosts(package, _config.Settings.DtsActiveTasksOnly);

                foreach (var taskHost in taskHosts)
                {
                    if (taskHost.InnerObject is IDTSExecuteSQL executeSQLTask)
                    {
                        if (executeSQLTask.SqlStatementSourceType == SqlStatementSourceType.Variable)
                        {
                            _log.Info("CreateVariables: Variablen für Task '" + taskHost.Name + "' erstellen.");

                            var expression = string.Empty;

                            foreach (var namespaceValue in _config.Settings.DtsNamespaceDirectories)
                            {
                                // mask variable names for validity
                                var variableName = FileUtils.MaskTaskName(taskHost.Name);

                                // variable must be generated
                                taskHost.Variables.Add(variableName, false, namespaceValue, "");

                                _log.Info("CreateVariables: Variable '" + variableName + "' mit Namespace '" + namespaceValue + "' erstellt.");

                                // define expression which selected correct variant.                                       
                                expression += "(" + switchParam + " == \"" + namespaceValue + "\")? \"" + namespaceValue + "::" + variableName + "\":";
                            }

                            expression += "\"\"";

                            taskHost.SetExpression("SqlStatementSource", expression);
                            _log.Info("CreateVariables: Task '" + taskHost.Name + "': Expression wurde gesetzt.");
                        }
                    }
                }
            }

            Trace.TraceInformation("CreateVariables - END");
        }

        /// <summary>
        /// Deletes all variables in specific namespace.
        /// </summary>
        public void DeleteVariables()
        {
            Trace.TraceInformation("DeleteVariables - BEGIN");

            foreach (var packageContext in _packageContexts)
            {
                var package = packageContext.PackageXML;
                _log.Info("DeleteVariables: Paket '" + packageContext.CanonicalPath + "' wird bearbeitet.");

                var taskHosts = GetTaskHosts(package, _config.Settings.DtsActiveTasksOnly);

                foreach (var taskHost in taskHosts)
                {
                    _log.Info("DeleteVariables: Untersuche Task '" + taskHost.Name + "'.");

                    if (taskHost.InnerObject is IDTSExecuteSQL executeSQLTask)
                    {
                        _log.Info("DeleteVariables: ... ist ein ExecuteSQL-Task.");

                        // only tasks with Variable input
                        if (executeSQLTask.SqlStatementSourceType == SqlStatementSourceType.Variable)
                        {
                            _log.Info("DeleteVariables: Variablenlöschung beginnt.");

                            foreach (var namespaceValue in _config.Settings.DtsNamespaceDirectories)
                            {
                                // Delete all variables of task with defined namespace.
                                foreach (var variable in taskHost.Variables.Cast<Variable>().ToList())
                                {
                                    if (variable.Namespace == namespaceValue)
                                    {
                                        taskHost.Variables.Remove(variable);

                                        _log.Info("DeleteVariables: Variable '" + variable.Namespace + "::" + variable.Name + "' im Task '" + taskHost.Name + "' gelöscht.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            _log.Info("DeleteVariables: ... hat KEIN Variable-Input, sondern " + executeSQLTask.SqlStatementSourceType + ".");
                        }
                    }
                    else
                    {
                        _log.Info("DeleteVariables: ... ist KEIN ExecuteSQL-Task.");
                    }
                }
            }

            Trace.TraceInformation("DeleteVariables - END");
        }

        /// <summary>
        /// Format sql files with TSqlFormatter.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void FormatSqlFiles(Config config)
        {
            Trace.TraceInformation("FormatSqlFiles - BEGIN");

            var formatOptions = new TSqlStandardFormatterOptions
            {
                NewClauseLineBreaks = config.SqlFormatterOptions.NewClauseLineBreaks,
                ExpandBetweenConditions = config.SqlFormatterOptions.ExpandBetweenConditions,
                ExpandBooleanExpressions = config.SqlFormatterOptions.ExpandBooleanExpressions,
                ExpandCaseStatements = config.SqlFormatterOptions.ExpandCaseStatements,
                ExpandCommaLists = config.SqlFormatterOptions.ExpandCommaLists,
                ExpandInLists = config.SqlFormatterOptions.ExpandInLists,
                IndentString = config.SqlFormatterOptions.IndentString,
                MaxLineWidth = config.SqlFormatterOptions.MaxLineWidth,
                SpaceAfterExpandedComma = config.SqlFormatterOptions.SpaceAfterExpandedComma,
                SpacesPerTab = config.SqlFormatterOptions.SpacesPerTab,
                KeywordStandardization = config.SqlFormatterOptions.KeywordStandardization,
                NewStatementLineBreaks = config.SqlFormatterOptions.NewStatementLineBreaks,
                TrailingCommas = config.SqlFormatterOptions.TrailingCommas,
                UppercaseKeywords = config.SqlFormatterOptions.UppercaseKeywords
            };

            var sqlFiles = FindSqlFiles();
            foreach (var sqlFilePath in sqlFiles)
            {
                FileUtils.FormatSqlFile(sqlFilePath, formatOptions);
            }

            Trace.TraceInformation("FormatSqlFiles - END");
        }

        /// <summary>
        /// Inject sql file contents in SqlTask with Direct as input.
        /// </summary>
        public void InjectDirect()
        {
            Trace.TraceInformation("InjectDirect - BEGIN");

            var sqlScripts = LoadSQLFiles();
            var projectParams = LoadProjectParams();

            foreach (var packageContext in _packageContexts)
            {
                var package = packageContext.PackageXML;
                _log.Info("InjectDirect: Paket '" + packageContext.CanonicalPath + "' wird bearbeitet.");

                var taskHosts = GetTaskHosts(package, _config.Settings.DtsActiveTasksOnly);

                foreach (var taskHost in taskHosts)
                {
                    if (taskHost.InnerObject is IDTSExecuteSQL executeSQLTask)
                    {
                        // only tasks with Direct as input
                        if (executeSQLTask.SqlStatementSourceType == SqlStatementSourceType.DirectInput)
                        {
                            var sqlScriptPath = SearchSqlScriptPath(sqlScripts, new TaskContext
                            {
                                Package = package,
                                TaskHost = taskHost,
                                ProjectParams = projectParams,
                                ExecuteSqlTask = executeSQLTask
                            });

                            if (sqlScriptPath != null)
                            {
                                _log.Info($"InjectDirect: Skript für Task '{taskHost.Name}' gefunden = '{sqlScriptPath}'.");

                                var sqlScript = sqlScripts[sqlScriptPath];
                                var oldValue = executeSQLTask.SqlStatementSource ?? "";

                                if (!oldValue.Equals(sqlScript, StringComparison.Ordinal))
                                {
                                    _log.Info("InjectDirect: Aktueller Taskinhalt ist nicht mit Skript identisch und wird ersetzt.");
                                    executeSQLTask.SqlStatementSource = sqlScript;
                                }
                                else
                                {
                                    _log.Info("InjectDirect: Aktueller Taskinhalt ist mit Skriptinhalt identisch. Keine Ersetzung.");
                                }
                            }
                            else
                            {
                                _log.Warn($"InjectDirect: Kein Skript für Task '{taskHost.Name}' gefunden!");
                            }
                        }
                    }
                }
            }

            Trace.TraceInformation("InjectDirect - END");
        }

        /// <summary>
        /// Inject sql file content in variables for SqlTasks. This will search for each SqlTask with Variable input for
        /// mapping variable with correct namespace.
        /// </summary>
        public void InjectVariables()
        {
            Trace.TraceInformation("InjectVariables - BEGIN");

            var sqlScripts = LoadSQLFiles();
            var projectParams = LoadProjectParams();

            foreach (var packageContext in _packageContexts)
            {
                var package = packageContext.PackageXML;
                _log.Info("InjectVariables: Paket '" + packageContext.CanonicalPath + "' wird bearbeitet.");

                foreach (var variable in package.Variables)
                {
                    // only apply changes to variables with configured namespace.
                    var isConfiguredNamespace = _config.Settings.DtsNamespaceDirectories.Any(n => n.Equals(variable.Namespace));

                    if (isConfiguredNamespace)
                    {
                        _log.Info($"InjectVariables: Suche Skript für Variable '{variable.Namespace}::{variable.Name}'.");

                        var sqlScriptPath = SearchSqlScriptPath(sqlScripts, new VariableContext
                        {
                            Variable = variable,
                            Package = package,
                            ProjectParams = projectParams
                        });

                        if (sqlScriptPath != null)
                        {
                            _log.Info($"InjectVariables: Skript für Variable '{variable.Namespace}::{variable.Name}' gefunden = '{sqlScriptPath}'.");

                            var sqlScript = sqlScripts[sqlScriptPath];
                            var oldValue = (string)variable.Value ?? "";

                            if (!oldValue.Equals(sqlScript, StringComparison.Ordinal))
                            {
                                _log.Info("InjectVariables: Aktueller Variableninhalt ist nicht mit Skript identisch und wird ersetzt.");
                                variable.Value = sqlScript;
                            }
                            else
                            {
                                _log.Info("InjectVariables: Aktueller Variableninhalt ist mit Skriptinhalt identisch. Keine Ersetzung.");
                            }
                        }
                        else
                        {
                            _log.Warn($"InjectVariables: Kein Skript für Variable '{variable.Namespace}::{variable.Name}' gefunden!");
                        }
                    }
                }
            }

            Trace.TraceInformation("InjectVariables - END");
        }

        /// <summary>
        /// Loads all packages inclusive content.
        /// </summary>
        public List<PackageContext> LoadPackages()
        {
            Trace.TraceInformation("LoadPackages - BEGIN");

            _log.Info(Strings.DTS_LOAD_PACKAGES);

            var dtsxFileNames = FindPackageFiles();

            _packageContexts = new List<PackageContext>();

            if (!dtsxFileNames.Any())
            {
                _log.Warn("Es wurden keine Pakete gefunden.");
            }
            else
            {
                foreach (var dtsxFileName in dtsxFileNames)
                {
                    _packageContexts.Add(new PackageContext(dtsxFileName, _dtsApp.LoadPackage(dtsxFileName, null)));

                    _log.Info("Paket '" + dtsxFileName + "' geladen.");
                }
            }

            Trace.TraceInformation("LoadPackages - BEGIN");
            return _packageContexts;
        }

        /// <summary>
        /// Load all project params from project file.
        /// </summary>
        /// <returns>List of project parameter.</returns>
        public List<ProjectParam> LoadProjectParams()
        {
            Trace.TraceInformation("LoadProjectParams - BEGIN");

            _log.Info("Projektparameter werden geladen.");

            var projectParams = new List<ProjectParam>();

            var paramsFileName = Path.Combine(_config.Settings.DtsRootDirectory, ProjectParamsFileName);
            if (!File.Exists(paramsFileName))
            {
                _log.Warn($"Die Projektparameterdatei {paramsFileName} existiert nicht.");
            }
            else
            {
                var doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(paramsFileName));

                var ns = new XmlNamespaceManager(doc.NameTable);
                ns.AddNamespace("SSIS", "www.microsoft.com/SqlServer/SSIS");

                // read parameter node
                var parameterNodeList = doc.SelectNodes("/SSIS:Parameters/SSIS:Parameter", ns);
                if (parameterNodeList != null)
                {
                    foreach (XmlNode parameterNode in parameterNodeList)
                    {
                        var param = new ProjectParam();

                        // Name is attribute
                        if (parameterNode.Attributes?["SSIS:Name"] != null)
                        {
                            param.Name = parameterNode.Attributes["SSIS:Name"].Value;
                        }

                        // Properties
                        var propertiesNode = parameterNode.SelectSingleNode("SSIS:Properties", ns);
                        var propertyNodeList = propertiesNode?.SelectNodes("SSIS:Property", ns);
                        if (propertyNodeList != null)
                        {
                            foreach (XmlNode propertyNode in propertyNodeList)
                            {
                                if (propertyNode.Attributes?["SSIS:Name"] != null)
                                {
                                    switch (propertyNode.Attributes["SSIS:Name"].Value)
                                    {
                                        case "ID":
                                            param.Id = propertyNode.InnerText;
                                            break;
                                        case "CreationName":
                                            param.CreationName = propertyNode.InnerText;
                                            break;
                                        case "Description":
                                            param.Description = propertyNode.InnerText;
                                            break;
                                        case "IncludeInDebugDump":
                                            param.IncludeInDebugDump =
                                                Convert.ToBoolean(Convert.ToInt32(propertyNode.InnerText));
                                            break;
                                        case "Required":
                                            param.Required = Convert.ToBoolean(Convert.ToInt32(propertyNode.InnerText));
                                            break;
                                        case "Sensitive":
                                            param.Sensitive =
                                                Convert.ToBoolean(Convert.ToInt32(propertyNode.InnerText));
                                            break;
                                        case "Value":
                                            param.Value = propertyNode.InnerText;
                                            break;
                                        case "DataType":
                                            param.DataType = Convert.ToInt32(propertyNode.InnerText);
                                            break;
                                    }
                                }
                            }
                        }

                        projectParams.Add(param);

                        _log.Info($"Parameter {param.Name} = {param.Value} geladen.");
                    }
                }
            }

            Trace.TraceInformation("LoadProjectParams - END");
            return projectParams;
        }

        /// <summary>
        /// Save and overwrites all packages.
        /// </summary>
        public void SavePackages()
        {
            Trace.TraceInformation("SavePackages - BEGIN");

            _log.Info("Pakete werden gespeichert.");

            foreach (var packageContext in _packageContexts)
            {
                _log.Info("Paket '" + packageContext.CanonicalPath + "' wird überschrieben.");
                _dtsApp.SaveToXml(packageContext.CanonicalPath, packageContext.PackageXML, null);
            }

            Trace.TraceInformation("SavePackages - END");
        }

        /// <summary>
        /// Changes input of all SqlTasks with FileConnection to Variable.
        /// </summary>
        public void SetFileConToVariable()
        {
            Trace.TraceInformation("SetFileConToVariable - BEGIN");

            foreach (var packageContext in _packageContexts)
            {
                var package = packageContext.PackageXML;
                _log.Info("SetFileConToVariable: Paket '" + packageContext.CanonicalPath + "' wird bearbeitet.");

                var taskHosts = GetTaskHosts(package, _config.Settings.DtsActiveTasksOnly);

                foreach (var taskHost in taskHosts)
                {
                    // WARN: Perhaps casting does not work because of System.__ComObjects
                    if (taskHost.InnerObject is IDTSExecuteSQL executeSQLTask)
                    {
                        // only tasks with FileConnection input
                        if (executeSQLTask.SqlStatementSourceType == SqlStatementSourceType.FileConnection)
                        {
                            executeSQLTask.SqlStatementSourceType = SqlStatementSourceType.Variable;
                            _log.Info("Task '" + taskHost.Name +
                                      "' von FileConnection auf Variable-Input umgestellt.");
                        }
                    }
                }
            }

            Trace.TraceInformation("SetFileConToVariable - END");
        }

        /// <summary>
        /// Creates a search path for given pattern and variable.
        /// </summary>
        /// <remarks>
        /// Sample for pattern:
        /// <![CDATA[
        /// "<Namespace>/<Package>/<Task>.sql"
        /// "<Package>/<Task>.sql",
        /// "<Namespace>/<Task>.sql"
        /// "<Task>.sql"
        /// "<ProjectParam:Variant>/<Package>/<Task>.sql"
        /// ]]>
        /// </remarks>
        /// <returns>Absolute path to sql file.</returns>
        private string BuildSearchPath(string pattern, VariableContext context)
        {
            Trace.TraceInformation("BuildSearchPath - BEGIN");

            pattern = pattern.Replace("<Namespace>", context.Variable.Namespace);
            pattern = pattern.Replace("<Package>", context.Package.Name);
            pattern = pattern.Replace("<Variable>", context.Variable.Name);
            foreach (var param in context.ProjectParams)
            {
                pattern = pattern.Replace($"<ProjectParam:{param.Name}>", param.Value);
            }

            try
            {
                var path = Path.Combine(_config.Settings.SqlRootDirectory, pattern);
                Trace.TraceInformation("BuildSearchPath - BEGIN");
                return new FileInfo(path).FullName;
            }
            catch
            {
                _log.Error($"Invalid path! SqlRootDirectory: {_config.Settings.SqlRootDirectory}, Pattern: {pattern}");
                throw;
            }
        }

        /// <summary>
        /// Creates a search path for given pattern and task.
        /// </summary>
        /// <remarks>
        /// Sample for pattern:
        /// <![CDATA[
        /// "<Namespace>/<Package>/<Task>.sql"
        /// "<Package>/<Task>.sql",
        /// "<Namespace>/<Task>.sql"
        /// "<Task>.sql"
        /// "<ProjectParam:Variant>/<Package>/<Task>.sql"
        /// ]]>
        /// </remarks>
        /// <returns>Absolute path to sql file.</returns>
        private string BuildSearchPath(string pattern, TaskContext context)
        {
            Trace.TraceInformation("BuildSearchPath - BEGIN");

            pattern = pattern.Replace("<Namespace>", ".");
            pattern = pattern.Replace("<Package>", context.Package.Name);
            pattern = pattern.Replace("<Task>", context.TaskHost.Name);
            foreach (var param in context.ProjectParams)
            {
                pattern = pattern.Replace($"<ProjectParam:{param.Name}>", param.Value);
            }

            try
            {
                var path = Path.Combine(_config.Settings.SqlRootDirectory, pattern);
                Trace.TraceInformation("BuildSearchPath - BEGIN");
                return new FileInfo(path).FullName;
            }
            catch
            {
                _log.Error($"Invalid path! SqlRootDirectory: {_config.Settings.SqlRootDirectory}, Pattern: {pattern}");
                throw;
            }
        }

        /// <summary>
        /// Searches for SqlTasks recursively.
        /// </summary>
        /// <param name="parentObject">Parent object..</param>
        /// <param name="onlyActive">Only active tasks will be searched.</param>
        /// <returns>List of SqlTasks.</returns>
        private List<TaskHost> GetTaskHosts(object parentObject, bool onlyActive = false)
        {
            Trace.TraceInformation("GetTaskHosts - BEGIN");

            var hosts = new List<TaskHost>();

            // NOTE: all Executables (Package, ForLoop, ForEachLoop, Sequence), of parent object will be checked.
            // NOTE: Check type of Executable. All could have TaskHosts as child objects.
            switch (parentObject)
            {
                case Executables executables:
                    Trace.TraceInformation("GetTaskHosts - Executable BEGIN");
                    foreach (var executable in executables)
                    {
                        hosts.AddRange(GetTaskHosts(executable));
                    }

                    Trace.TraceInformation("GetTaskHosts - Executable END");
                    break;
                case Package package when !onlyActive || !package.Disable:
                    Trace.TraceInformation("GetTaskHosts - Package BEGIN");
                    foreach (var packageExecutables in package.Executables)
                    {
                        hosts.AddRange(GetTaskHosts(packageExecutables));
                    }

                    Trace.TraceInformation("GetTaskHosts - Package END");
                    break;
                case ForLoop forLoop when !onlyActive || !forLoop.Disable:
                    Trace.TraceInformation("GetTaskHosts - ForLoop BEGIN");
                    hosts.AddRange(GetTaskHosts(forLoop.Executables));
                    Trace.TraceInformation("GetTaskHosts - ForLoop END");
                    break;
                case ForEachLoop forEachLoop when !onlyActive || !forEachLoop.Disable:
                    Trace.TraceInformation("GetTaskHosts - ForEachLoop BEGIN");
                    hosts.AddRange(GetTaskHosts(forEachLoop.Executables));
                    Trace.TraceInformation("GetTaskHosts - ForEachLoop END");
                    break;
                case Sequence sequence when !onlyActive || !sequence.Disable:
                    Trace.TraceInformation("GetTaskHosts - Sequence BEGIN");
                    hosts.AddRange(GetTaskHosts(sequence.Executables));
                    Trace.TraceInformation("GetTaskHosts - Sequence END");
                    break;
                case TaskHost taskHost when !onlyActive && !taskHost.Disable:
                    Trace.TraceInformation("GetTaskHosts - TaskHost BEGIN");
                    hosts.Add(taskHost);
                    Trace.TraceInformation("GetTaskHosts - TaskHost END");
                    break;
            }

            Trace.TraceInformation("GetTaskHosts - END");
            return hosts;
        }

        /// <summary>
        /// Finds the sql files.
        /// </summary>
        /// <returns>List of sql file names.</returns>
        private List<string> FindSqlFiles()
        {
            Trace.TraceInformation("FindSqlFiles - BEGIN");

            if (_sqlFiles == null)
            {
                var sqlRootDirectory = new DirectoryInfo(_config.Settings.SqlRootDirectory);
                if (!sqlRootDirectory.Exists)
                {
                    _log.Error($"SQL-Wurzelverzeichnis {sqlRootDirectory.FullName} existiert nicht");
                    _sqlFiles = new List<string>();
                }
                else
                {
                    _log.Debug($"Suche im Ordner {sqlRootDirectory.FullName} nach SQL-Dateien");

                    var regex = new Regex(_config.Settings.SqlFileFilterRegex);
                    _sqlFiles = Directory.GetFiles(sqlRootDirectory.FullName,
                            "*.*",
                            _config.Settings.SqlRecursiveFileSearch
                                ? SearchOption.AllDirectories
                                : SearchOption.TopDirectoryOnly)
                        .Where(path => regex.IsMatch(path))
                        .Select(path => new FileInfo(path).FullName).ToList();

                    if (!_sqlFiles.Any())
                    {
                        _log.Warn(
                            $"Es wurden keine SQL-Dateien im Pfad {sqlRootDirectory.FullName} {(_config.Settings.SqlRecursiveFileSearch ? "(inklusive Unterverzeichnisse) " : "")}mit dem Filter {_config.Settings.SqlFileFilterRegex} gefunden.");
                    }
                }
            }

            Trace.TraceInformation("FindSqlFiles - END");
            return _sqlFiles;
        }

        /// <summary>
        /// Finds the connection manager files.
        /// </summary>
        /// <returns>List of conmgr file names.</returns>
        private List<string> FindConmgrFiles()
        {
            Trace.TraceInformation("FindConmgrFiles - BEGIN");

            if (_conmgrFiles == null)
            {
                var dtsRootDirectory = new DirectoryInfo(_config.Settings.DtsRootDirectory);
                if (!dtsRootDirectory.Exists)
                {
                    _log.Error($"DTS-Wurzelverzeichnis {dtsRootDirectory.FullName} existiert nicht");
                    _conmgrFiles = new List<string>();
                }
                else
                {
                    _log.Debug($"Suche im Ordner {dtsRootDirectory.FullName} nach Conmgr-Dateien");

                    var regex = new Regex(ConnectionManagerFileFilterRegex);
                    _conmgrFiles = Directory.GetFiles(dtsRootDirectory.FullName,
                            "*.*", SearchOption.TopDirectoryOnly)
                        .Where(path => regex.IsMatch(path))
                        .Select(path => new FileInfo(path).FullName).ToList();

                    if (!_conmgrFiles.Any())
                    {
                        _log.Warn($"Es wurden keine Conmgr-Dateien im Pfad {dtsRootDirectory.FullName} mit dem Filter {ConnectionManagerFileFilterRegex} gefunden.");
                    }
                }
            }

            Trace.TraceInformation("FindConmgrFiles - END");
            return _conmgrFiles;
        }

        /// <summary>
        /// Finds the package files.
        /// </summary>
        /// <returns>List of package file names.</returns>
        private List<string> FindPackageFiles()
        {
            Trace.TraceInformation("FindPackageFiles - BEGIN");

            if (_dtsxFiles == null)
            {
                var dtsRootDirectory = new DirectoryInfo(_config.Settings.DtsRootDirectory);
                if (!dtsRootDirectory.Exists)
                {
                    _log.Error($"DTS-Wurzelverzeichnis {dtsRootDirectory.FullName} existiert nicht");
                    _dtsxFiles = new List<string>();
                }
                else
                {
                    _log.Debug($"Suche im Ordner {dtsRootDirectory.FullName} nach Conmgr-Dateien");

                    var regex = new Regex(PackageFileFilterRegex);
                    _dtsxFiles = Directory.GetFiles(dtsRootDirectory.FullName,
                            "*.*", SearchOption.TopDirectoryOnly)
                        .Where(path => regex.IsMatch(path))
                        .Select(path => new FileInfo(path).FullName).ToList();

                    if (!_dtsxFiles.Any())
                    {
                        _log.Warn($"Es wurden keine DTSX-Dateien im Pfad {dtsRootDirectory.FullName} mit dem Filter {PackageFileFilterRegex} gefunden.");
                    }
                }
            }

            Trace.TraceInformation("FindPackageFiles - END");
            return _dtsxFiles;
        }

        /// <summary>
        /// Load all sql files in temporary dictionary.
        /// </summary>
        /// <returns>Dictionary with file name as keys and content as values.</returns>
        private Dictionary<string, string> LoadSQLFiles()
        {
            Trace.TraceInformation("LoadSQLFiles - BEGIN");

            _log.Info("SQL-Dateien werden geladen.");

            var sqlFilesCollection = new Dictionary<string, string>();

            var sqlFiles = FindSqlFiles();

            foreach (var sqlFile in sqlFiles)
            {
                var sqlFileContent = File.ReadAllText(sqlFile);

                // WARN: SSIS use LF only (unix line breaks). Replace CRLF with LF for comparison
                sqlFileContent = sqlFileContent.Replace("\r\n", "\n");

                _log.Info("SQL-Datei '" + sqlFile + "' geladen.");

                if (!sqlFilesCollection.ContainsKey(sqlFile))
                {
                    sqlFilesCollection.Add(sqlFile, sqlFileContent);
                }
            }

            Trace.TraceInformation("LoadSQLFiles - END");
            return sqlFilesCollection;
        }

        /// <summary>
        /// Searches for mapping sql file for variable.
        /// </summary>
        /// <param name="sqlScripts">Found sql file.</param>
        /// <param name="context">Context of variable.</param>
        /// <returns>
        /// Path to sql file or null.
        /// </returns>
        private string SearchSqlScriptPath(Dictionary<string, string> sqlScripts, VariableContext context)
        {
            Trace.TraceInformation("SearchSqlScriptPath - BEGIN");

            var variablePatterns = _config.Settings.SqlMappingPatterns.Where(p => p.Contains("<Variable>")).ToList();
            if (variablePatterns.Count == 0)
            {
                _log.Error("No SqlMappingPattern for <Variable> found");
            }
            else
            {
                foreach (var pattern in variablePatterns)
                {
                    try
                    {
                        var searchPath = BuildSearchPath(pattern, context);
                        foreach (var scriptPath in sqlScripts.Keys)
                        {
                            if (string.Equals(scriptPath, searchPath, StringComparison.CurrentCultureIgnoreCase))
                            {
                                _log.Debug(
                                    $"SQL Datei {scriptPath} für Variable '{context.Variable.Namespace}::{context.Variable.Name}' über Pattern {pattern} gefunden.");
                                return scriptPath;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Search sql script path has failed:", ex);
                    }
                }
            }

            Trace.TraceInformation("SearchSqlScriptPath - END");
            return null;
        }

        /// <summary>
        /// Searches for mapping sql file for SqlTask.
        /// </summary>
        /// <param name="sqlScripts">Found sql file.</param>
        /// <param name="context">Context of task.</param>
        /// <returns>
        /// Path to sql file or null.
        /// </returns>
        private string SearchSqlScriptPath(Dictionary<string, string> sqlScripts, TaskContext context)
        {
            Trace.TraceInformation("SearchSqlScriptPath - BEGIN");

            var taskPatterns = _config.Settings.SqlMappingPatterns.Where(p => p.Contains("<Task>")).ToList();
            if (taskPatterns.Count == 0)
            {
                _log.Error("No SqlMappingPattern for <Task> found");
            }
            else
            {
                foreach (var pattern in _config.Settings.SqlMappingPatterns)
                {
                    try
                    {
                        var searchPath = BuildSearchPath(pattern, context);
                        foreach (var scriptPath in sqlScripts.Keys)
                        {
                            if (string.Equals(scriptPath, searchPath, StringComparison.CurrentCultureIgnoreCase))
                            {
                                _log.Debug($"SQL Datei {scriptPath} für Task '{context.TaskHost.Name}' über Pattern {pattern} gefunden.");
                                return scriptPath;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Search sql script path has failed:", ex);
                    }
                }
            }

            Trace.TraceInformation("SearchSqlScriptPath - END");
            return null;
        }
    }
}