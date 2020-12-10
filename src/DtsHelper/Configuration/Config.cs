// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Configuration
{
    /// <summary>
    ///     Application configuration.
    /// </summary>
    public class Config
    {
        /// <summary>
        ///     The default settings.
        /// </summary>
        public static Settings DefaultSettings = new Settings
        {
            DtsActiveTasksOnly = false,
            DtsAppName = string.Empty,
            DtsRootDirectory = ".",
            DtsSwitchParam = string.Empty,
            SqlRootDirectory = ".",
            SqlFileFilterRegex = "[.]sql$",
            SqlRecursiveFileSearch = true,
            SqlMappingPatterns = new[] {"<Namespace>/<Package>/<Task>.sql", "<Package>/<Task>.sql", "<Namespace>/<Task>.sql", "<Task>.sql"}
        };
        /// <summary>
        ///     The default commands.
        /// </summary>
        public static Commands DefaultCommands = new Commands
        {
            SetFileConnectionToVariable = false,
            DeleteVariables = false,
            CreateVariables = false,
            ConvertToUTF8 = false,
            InjectVariables = false,
            ChangeAppName = false,
            InjectDirect = false
        };
        /// <summary>
        ///     The default SQL formatter options.
        /// </summary>
        public static SqlFormatterOptions DefaultSqlFormatterOptions = new SqlFormatterOptions
        {
            NewClauseLineBreaks = 1,
            ExpandBetweenConditions = true,
            ExpandBooleanExpressions = true,
            ExpandCaseStatements = true,
            ExpandCommaLists = true,
            ExpandInLists = true,
            IndentString = "\t",
            MaxLineWidth = 999,
            SpaceAfterExpandedComma = false,
            SpacesPerTab = 4,
            KeywordStandardization = true,
            NewStatementLineBreaks = 2,
            TrailingCommas = true,
            UppercaseKeywords = true
        };

        /// <summary>
        ///     Execution commands.
        /// </summary>
        /// <value>
        ///     The commands.
        /// </value>
        public Commands Commands { get; set; } = DefaultCommands;

        /// <summary>
        ///     Application settings.
        /// </summary>
        /// <value>
        ///     The settings.
        /// </value>
        public Settings Settings { get; set; } = DefaultSettings;

        /// <summary>
        ///     Formatting options.
        /// </summary>
        /// <value>
        ///     The SQL formatter options.
        /// </value>
        public SqlFormatterOptions SqlFormatterOptions { get; set; } = DefaultSqlFormatterOptions;
    }
}