// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Configuration
{
    /// <summary>
    ///     Application wide settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        ///     Only apply changes to active SqlTasks.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [DTS active tasks only]; otherwise, <c>false</c>.
        /// </value>
        public bool DtsActiveTasksOnly { get; set; }

        /// <summary>
        ///     Name of Dts application.
        /// </summary>
        /// <value>
        ///     The name of the DTS application.
        /// </value>
        public string DtsAppName { get; set; }

        /// <summary>
        ///     Namespace of new Variables.
        /// </summary>
        /// <value>
        ///     The DTS namespace directories.
        /// </value>
        public string[] DtsNamespaceDirectories { get; set; }

        /// <summary>
        ///     Root of dts project with all dtsx packages.
        /// </summary>
        /// <value>
        ///     The DTS root directory.
        /// </value>
        public string DtsRootDirectory { get; set; }

        /// <summary>
        ///     Parameter which determines the script variable in SqlTask.
        /// </summary>
        /// <value>
        ///     The DTS switch parameter.
        /// </value>
        public string DtsSwitchParam { get; set; }

        /// <summary>
        ///     Root of sql database project with all sql files.
        /// </summary>
        /// <value>
        ///     The SQL root directory.
        /// </value>
        public string SqlRootDirectory { get; set; }

        /// <summary>
        ///     Filter for searching sql files. Default is *.sql (Wildcard).
        /// </summary>
        /// <value>
        ///     The SQL file filter regex.
        /// </value>
        public string SqlFileFilterRegex { get; set; }

        /// <summary>
        ///     Define whether to search for sql files recursive inclusive sub directories.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [SQL recursive file search]; otherwise, <c>false</c>.
        /// </value>
        public bool SqlRecursiveFileSearch { get; set; }

        /// <summary>
        ///     Pattern for search matching sql files to SqlTasks.
        /// </summary>
        /// <value>
        ///     The SQL mapping patterns.
        /// </value>
        public string[] SqlMappingPatterns { get; set; }
    }
}