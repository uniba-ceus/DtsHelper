// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Apps
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using DtsHelper.Configuration;
    using DtsHelper.Core;
    using log4net;

    /// <summary>
    ///     Application Bootstrapper.
    /// </summary>
    public class Bootstrapper
    {
        private const string DefaultConfigFileName = "config.json";
        private static readonly ILog _log = LogManager.GetLogger(typeof(Bootstrapper));
        private Config _config;
        private string _configFileName;
        private bool _helpCmd;
        private bool _initCmd;
        private bool _runCmd;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Bootstrapper" /> class.
        /// </summary>
        public Bootstrapper()
        {
            _configFileName = DefaultConfigFileName;
        }

        /// <summary>
        ///     Starts the bootstrapper.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Run(string[] args)
        {
            try
            {
                try
                {
                    ParseArgs(args);

                    if (_helpCmd)
                    {
                        PrintHelp();
                        return;
                    }

                    if (_initCmd)
                    {
                        if (File.Exists(DefaultConfigFileName))
                        {
                            _log.Error(
                                $"Eine neue Konfigurationsdatei konnte nicht erstellt werden, da bereits eine Datei mit dem Namen {DefaultConfigFileName} existiert.");
                        }
                        else
                        {
                            _log.Debug($"Erstelle neue Konfigurationsdatei {DefaultConfigFileName}.");
                            ConfigParser.Save(DefaultConfigFileName, new Config());
                            _log.Info($"Eine neue Konfigurationsdatei {DefaultConfigFileName} wurde erstellt.");
                        }

                        return;
                    }
                }
                catch (ArgumentException ex)
                {
                    _log.Error("Ein Eingabewert in der Konfigurationsspezifikation ist ungültig.\n\n" + ex.Message);
                    PrintHelp();
                    return;
                }

                // Einlesen der Konfiguration
                if (File.Exists(_configFileName))
                {
                    _log.Info($"Lese Konfigurationsdatei {_configFileName}.");
                    _config = ConfigParser.Load(_configFileName);
                }
                else
                {
                    _log.Error($"Die Konfigurationsdatei {_configFileName} wurde nicht gefunden!");
                    PrintHelp();
                    return;
                }

                if (_runCmd)
                {
                    RunCommands();
                }
                else
                {
                    PrintHelp();
                }
            }
            catch (Exception ex)
            {
                _log.Error("Fehler in " + new StackTrace().GetFrame(0).GetMethod().Name + ": " + ex);
            }
        }

        /// <summary>
        ///     Prints the help information to console.
        /// </summary>
        private static void PrintHelp()
        {
            var helpMsg = new StringBuilder();
            helpMsg.AppendLine($"{AppInfo.Default.ProductName} {AppInfo.Default.Copyright}");
            helpMsg.AppendLine($"Version: {AppInfo.Default.Version}");
            helpMsg.AppendLine(
                "Beschreibung: Mit dem DtsHelper lassen sich bestehende SQL-Tasks in einem SSIS-Projekt bearbeiten. Sofern Dateiverbindung als Quelle für den");
            helpMsg.AppendLine(
                "Skriptinput vorliegen, lassen sich die Quellen auf Skriptinput von Variablen umstellen.");
            helpMsg.AppendLine(
                "Zu jedem SQL-Task werden Task-Variablen erstellt, die als Namespaces die Universitätsnamen haben. Weiterhin werden in jedem Task");
            helpMsg.AppendLine("Expressions erstellt, die über einen Projektparameter eine Task-Variable auswählen.");
            helpMsg.AppendLine();
            helpMsg.AppendLine();
            helpMsg.AppendLine("Verwendung: DtsHelper [<Befehle...>]");
            helpMsg.AppendLine();
            helpMsg.AppendLine();
            helpMsg.AppendLine("Befehle:");
            helpMsg.AppendLine();
            helpMsg.AppendLine("    -h, --help   Zeigt diese Hilfe an.");
            helpMsg.AppendLine();
            helpMsg.AppendLine("    -i, --init   Erstellt eine neue Konfigurationsdatei config.json mit Standardwerten.");
            helpMsg.AppendLine();
            helpMsg.AppendLine("    -r, --run    Startet das Programm.");
            helpMsg.AppendLine();
            helpMsg.AppendLine();
            helpMsg.AppendLine("Optionen:");
            helpMsg.AppendLine(
                "    -c,--config=<config>  Angabe einer alternativen Konfigurationsdatei. Falls diese nicht explizit angegeben ist, wird nach einer Datei config.json im aktuellen Verzeichnis gesucht");
            helpMsg.AppendLine();

            Console.WriteLine(helpMsg);
        }

        /// <summary>
        ///     Parses the arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ArgumentException">
        ///     Die angegebene Konfigurationsdatei \"" +
        ///     _configFileName + "\" existiert nicht.
        ///     or
        ///     Nach dem Befehl \"-config\" muss ein = Zeichen gefolgt von dem Dateipfad angegeben werden. Falls der Pfad
        ///     Leerzeichen enthält, muss dieser in Hochkommata gesetzt werden.
        /// </exception>
        private void ParseArgs(string[] args)
        {
            foreach (var arg in args)
            {
                var key = arg;
                var value = string.Empty;
                var equalIndex = arg.IndexOf("=", StringComparison.Ordinal);
                if (equalIndex >= 0)
                {
                    key = arg.Substring(0, equalIndex);
                    value = arg.Substring(equalIndex + 1);
                }

                switch (key)
                {
                    case "--help":
                    case "-h":
                        _helpCmd = true;
                        break;
                    case "--init":
                    case "-i":
                        _initCmd = true;
                        break;
                    case "--run":
                    case "-r":
                        _runCmd = true;
                        break;
                    case "--config":
                    case "-c:":
                        if (!string.IsNullOrEmpty(value))
                        {
                            _configFileName = value.Replace("\"", "");
                            _log.Debug($"Name der Konfiguationsdatei geändert auf {_configFileName}");
                            if (!File.Exists(_configFileName))
                            {
                                throw new ArgumentException("Die angegebene Konfigurationsdatei \"" +
                                                            _configFileName + "\" existiert nicht.");
                            }
                        }
                        else
                        {
                            throw new ArgumentException(
                                "Nach dem Befehl \"-config\" muss ein = Zeichen gefolgt von dem Dateipfad angegeben werden. Falls der Pfad Leerzeichen enthält, muss dieser in Hochkommata gesetzt werden.");
                        }

                        break;
                }
            }
        }

        /// <summary>
        ///     Validates whether a command is specified.
        /// </summary>
        private void ValidateCommands()
        {
            if (_config.Commands.SetFileConnectionToVariable)
            {
                _log.Debug("CMD: SetFileConnectionToVariable.");
            }

            if (_config.Commands.DeleteVariables)
            {
                _log.Debug("CMD: DeleteVariables.");
            }

            if (_config.Commands.CreateVariables)
            {
                _log.Debug("CMD: CreateVariables.");
            }

            if (_config.Commands.ConvertToUTF8)
            {
                _log.Debug("CMD: ConvertToUTF8.");
            }

            if (_config.Commands.ChangeAppName)
            {
                _log.Debug("CMD: FormatSQL.");
            }

            if (_config.Commands.InjectVariables)
            {
                _log.Debug("CMD: InjectVariables.");
            }

            if (_config.Commands.InjectDirect)
            {
                _log.Debug("CMD: InjectDirect.");
            }

            if (_config.Commands.ChangeAppName)
            {
                _log.Debug("CMD: ChangeAppName.");
            }

            if ((_config.Commands.SetFileConnectionToVariable != true)
                & (_config.Commands.DeleteVariables != true)
                & (_config.Commands.CreateVariables != true)
                & (_config.Commands.ConvertToUTF8 != true)
                & (_config.Commands.FormatSQL != true)
                & (_config.Commands.InjectVariables != true)
                & (_config.Commands.ChangeAppName != true))
            {
                throw new InvalidConfigurationException("CMD: Es wurde kein gültiger Arbeitsbefehl gefunden.");
            }
        }

        /// <summary>
        ///     Validates configuration.
        /// </summary>
        private void ValidateConfig()
        {
            #region Notwendig Konfigurationen. Wenn diese nicht konfiguriert sind, bricht das Tool ab.

            // sqlRootDirectory.
            if (!string.IsNullOrEmpty(_config.Settings.SqlRootDirectory))
            {
                if (Directory.Exists(_config.Settings.SqlRootDirectory))
                {
                    _log.Debug("SETTING: SQLRootDirectory = " + _config.Settings.SqlRootDirectory);
                }
                else
                {
                    throw new InvalidConfigurationException("SQLRootDirectory \"" + _config.Settings.SqlRootDirectory +
                                                            "\" existiert nicht.");
                }
            }
            else
            {
                throw new InvalidConfigurationException("SQLRootDirectory ist nicht spezifiziert.");
            }

            // dtsRootDirectory.
            if (!string.IsNullOrEmpty(_config.Settings.DtsRootDirectory))
            {
                if (Directory.Exists(_config.Settings.DtsRootDirectory))
                {
                    _log.Debug("SETTING: DtsRootDirectory = " + _config.Settings.DtsRootDirectory);
                }
                else
                {
                    throw new InvalidConfigurationException("DtsRootDirectory \"" + _config.Settings.DtsRootDirectory +
                                                            "\" existiert nicht.");
                }
            }
            else
            {
                throw new InvalidConfigurationException("DtsRootDirectory ist nicht spezifiziert.");
            }

            // dtsNamespaceDirectories.
            if (_config.Settings.DtsNamespaceDirectories != null)
            {
                _log.Debug($"SETTING: DtsNamespaceDirectories = {string.Join(",", _config.Settings.DtsNamespaceDirectories)}");

                foreach (var namespaceDirectory in _config.Settings.DtsNamespaceDirectories)
                {
                    var path = Path.Combine(_config.Settings.SqlRootDirectory, namespaceDirectory);
                    if (!Directory.Exists(path))
                    {
                        _log.Warn($"NamespaceDirectory (Ordner) \"{path}\" existiert nicht.");

                        //throw new InvalidConfigurationException($"NamespaceDirectory (Ordner) \"{path}\" existiert nicht.");
                    }
                }
            }
            else
            {
                throw new InvalidConfigurationException("DtsNamespaceDirectories ist nicht spezifiziert.");
            }

            // dtsSwitchParam.
            if (_config.Settings.DtsSwitchParam != null && _config.Settings.DtsNamespaceDirectories != null &&
                _config.Settings.DtsNamespaceDirectories.Length > 1)
            {
                _log.Debug("SETTING: DtsSwitchParam = " + _config.Settings.DtsSwitchParam);
            }
            else if (_config.Settings.DtsSwitchParam != null && _config.Settings.DtsNamespaceDirectories == null)
            {
                throw new InvalidConfigurationException(
                    "DtsSwitchParam ist spezifiziert. Dieser wird jedoch nicht benötigt, da kein DtsNamespaceDirectory spezifiziert ist.");
            }
            else if (_config.Settings.DtsSwitchParam != null && _config.Settings.DtsNamespaceDirectories != null &&
                     _config.Settings.DtsNamespaceDirectories.Length == 1)
            {
                throw new InvalidConfigurationException(
                    "DtsSwitchParam ist spezifiziert. Dieser wird jedoch nicht benötigt, da nur ein DtsNamespaceDirectory spezifiziert ist.");
            }
            else if (_config.Settings.DtsSwitchParam == null && _config.Settings.DtsNamespaceDirectories != null &&
                     _config.Settings.DtsNamespaceDirectories.Length > 1)
            {
                throw new InvalidConfigurationException(
                    "DtsSwitchParam ist nicht spezifiziert. Es existieren jedoch mehrere DtsNamespaceDirectories. Der DtsSwitchParam dient als Schalter zwischen den DtsNamespaceDirectories und muss angegeben werden.");
            }

            // dtsAppName.
            if (_config.Commands.ChangeAppName && string.IsNullOrEmpty(_config.Settings.DtsAppName))
            {
                throw new InvalidConfigurationException("ApplicationName ist nicht spezifiziert.");
            }

            _log.Debug("SETTING: ApplicationName = " + _config.Settings.DtsAppName);

            #endregion

            #region Optionale Konfigurationen. Der Konfigurationszustand wird ausgegeben.

            // dtsActiveTasksOnly.
            _log.Debug($"SETTING: ActiveTasksOnly = {_config.Settings.DtsActiveTasksOnly}");

            #endregion
        }

        /// <summary>
        ///     Validates sql formatter options.
        /// </summary>
        private void ValidateSqlFormatterOptions()
        {
            if (_config.SqlFormatterOptions.IndentString == null)
            {
                throw new InvalidConfigurationException("IndentString ist nicht spezifiziert.");
            }

            _log.Debug(
                $"SQLFORMAT: NewClauseLineBreaks: {_config.SqlFormatterOptions.NewClauseLineBreaks}");
            _log.Debug(
                $"SQLFORMAT: ExpandBetweenConditions: {_config.SqlFormatterOptions.ExpandBetweenConditions}");
            _log.Debug(
                $"SQLFORMAT: ExpandBooleanExpressions: {_config.SqlFormatterOptions.ExpandBooleanExpressions}");
            _log.Debug(
                $"SQLFORMAT: ExpandCaseStatements: {_config.SqlFormatterOptions.ExpandCaseStatements}");
            _log.Debug(
                $"SQLFORMAT: ExpandCommaLists: {_config.SqlFormatterOptions.ExpandCommaLists}");
            _log.Debug($"SQLFORMAT: IndentString = {_config.SqlFormatterOptions.IndentString}");
            _log.Debug($"SQLFORMAT: ExpandInLists: {_config.SqlFormatterOptions.ExpandInLists}");
            _log.Debug($"SQLFORMAT: MaxLineWidth: {_config.SqlFormatterOptions.MaxLineWidth}");
            _log.Debug(
                $"SQLFORMAT: SpaceAfterExpandedComma: {_config.SqlFormatterOptions.SpaceAfterExpandedComma}");
            _log.Debug($"SQLFORMAT: SpacesPerTab: {_config.SqlFormatterOptions.SpacesPerTab}");
            _log.Debug(
                $"SQLFORMAT: KeywordStandardization: {_config.SqlFormatterOptions.KeywordStandardization}");
            _log.Debug(
                $"SQLFORMAT: NewStatementLineBreaks: {_config.SqlFormatterOptions.NewStatementLineBreaks}");
            _log.Debug("SQLFORMAT: TrailingCommas: " + _config.SqlFormatterOptions.TrailingCommas);
            _log.Debug(
                $"SQLFORMAT: UppercaseKeywords: {_config.SqlFormatterOptions.UppercaseKeywords}");
        }

        /// <summary>
        ///     Runs the commands.
        /// </summary>
        private void RunCommands()
        {
            _log.Debug("Validiere die Konfiguration.");

            ValidateConfig();
            ValidateCommands();
            ValidateSqlFormatterOptions();

            _log.Info("Beginne mit der Verarbeitung der Kommandos.");

            // load all packages
            var dtsProcessor = new DtsProcessor(_config);
            dtsProcessor.LoadPackages();

            // execute all commands
            if (_config.Commands.SetFileConnectionToVariable)
            {
                _log.Info("CMD: Konvertiere Dateiverbindungen zu Variablen...");
                dtsProcessor.SetFileConToVariable();
            }

            if (_config.Commands.ConvertToUTF8)
            {
                Console.WriteLine("CMD: Skripte werden in UTF-8 konvertiert...");
                dtsProcessor.ConvertToUTF8(_config);
            }

            if (_config.Commands.FormatSQL)
            {
                Console.WriteLine("CMD: SQL-Code wird formatiert...");
                dtsProcessor.FormatSqlFiles(_config);
            }

            if (_config.Commands.DeleteVariables)
            {
                Console.WriteLine("CMD: Variablen in Tasks werden gelöscht...");
                dtsProcessor.DeleteVariables();
            }

            if (_config.Commands.CreateVariables)
            {
                Console.WriteLine("CMD: Variablen in Tasks werden erstellt...");
                dtsProcessor.CreateVariables();
            }

            if (_config.Commands.InjectVariables)
            {
                Console.WriteLine("CMD: Variablen in Tasks werden mit Code befüllt...");
                dtsProcessor.InjectVariables();
            }

            if (_config.Commands.InjectDirect)
            {
                Console.WriteLine("CMD: Tasks werden direkt mit Code befüllt...");
                dtsProcessor.InjectDirect();
            }

            if (_config.Commands.ChangeAppName)
            {
                Console.WriteLine("CMD: App-Name wird geschrieben...");
                dtsProcessor.ChangeAppName();
            }

            // store all changes
            dtsProcessor.SavePackages();

            _log.Info("Alle Kommandos erfolgreich abgearbeitet.");
        }
    }
}