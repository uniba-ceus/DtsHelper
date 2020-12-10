// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Configuration
{
    /// <summary>
    ///     Collection of all available commands.
    /// </summary>
    public class Commands
    {
        /// <summary>
        ///     Change application name of database connections. This allows identification of open connections in SQL Server
        ///     activity monitor.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [change application name]; otherwise, <c>false</c>.
        /// </value>
        public bool ChangeAppName { get; set; }

        /// <summary>
        ///     Convert all files to UTF8.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [convert to ut f8]; otherwise, <c>false</c>.
        /// </value>
        public bool ConvertToUTF8 { get; set; }

        /// <summary>
        ///     Create Variables automatically.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [create variables]; otherwise, <c>false</c>.
        /// </value>
        public bool CreateVariables { get; set; }

        /// <summary>
        ///     Delete Variables automatically.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [delete variables]; otherwise, <c>false</c>.
        /// </value>
        public bool DeleteVariables { get; set; }

        /// <summary>
        ///     Format all SQL files.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [format SQL]; otherwise, <c>false</c>.
        /// </value>
        public bool FormatSQL { get; set; }

        /// <summary>
        ///     Inject sql code to variables.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [inject variables]; otherwise, <c>false</c>.
        /// </value>
        public bool InjectVariables { get; set; }

        /// <summary>
        ///     Set SqlTask file connections to variables automatically.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [set file connection to variable]; otherwise, <c>false</c>.
        /// </value>
        public bool SetFileConnectionToVariable { get; set; }

        /// <summary>
        ///     Inject sql code to SqlTask with direct code.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [inject direct]; otherwise, <c>false</c>.
        /// </value>
        public bool InjectDirect { get; set; }
    }
}