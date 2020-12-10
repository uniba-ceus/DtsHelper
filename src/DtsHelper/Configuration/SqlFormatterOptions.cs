// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Configuration
{
    /// <summary>
    ///     Sql file formatting options.
    /// </summary>
    public class SqlFormatterOptions
    {
        /// <summary>
        ///     Gets or sets a value indicating whether [expand between conditions].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [expand between conditions]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandBetweenConditions { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [expand boolean expressions].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [expand boolean expressions]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandBooleanExpressions { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [expand case statements].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [expand case statements]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandCaseStatements { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [expand comma lists].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [expand comma lists]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandCommaLists { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [expand in lists].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [expand in lists]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandInLists { get; set; }

        /// <summary>
        ///     Gets or sets the indent string.
        /// </summary>
        /// <value>
        ///     The indent string.
        /// </value>
        public string IndentString { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [keyword standardization].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [keyword standardization]; otherwise, <c>false</c>.
        /// </value>
        public bool KeywordStandardization { get; set; }

        /// <summary>
        ///     Gets or sets the maximum width of the line.
        /// </summary>
        /// <value>
        ///     The maximum width of the line.
        /// </value>
        public int MaxLineWidth { get; set; }

        /// <summary>
        ///     Gets or sets the new clause line breaks.
        /// </summary>
        /// <value>
        ///     The new clause line breaks.
        /// </value>
        public int NewClauseLineBreaks { get; set; }

        /// <summary>
        ///     Gets or sets the new statement line breaks.
        /// </summary>
        /// <value>
        ///     The new statement line breaks.
        /// </value>
        public int NewStatementLineBreaks { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [space after expanded comma].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [space after expanded comma]; otherwise, <c>false</c>.
        /// </value>
        public bool SpaceAfterExpandedComma { get; set; }

        /// <summary>
        ///     Gets or sets the spaces per tab.
        /// </summary>
        /// <value>
        ///     The spaces per tab.
        /// </value>
        public int SpacesPerTab { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [trailing commas].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [trailing commas]; otherwise, <c>false</c>.
        /// </value>
        public bool TrailingCommas { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [uppercase keywords].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [uppercase keywords]; otherwise, <c>false</c>.
        /// </value>
        public bool UppercaseKeywords { get; set; }
    }
}