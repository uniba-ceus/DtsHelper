// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Core
{
    /// <summary>
    ///     Simple class holding project data.
    /// </summary>
    public class ProjectParam
    {
        /// <summary>
        ///     Gets or sets the name of the creation.
        /// </summary>
        /// <value>
        ///     The name of the creation.
        /// </value>
        public string CreationName { get; set; }

        /// <summary>
        ///     Gets or sets the type of the data.
        /// </summary>
        /// <value>
        ///     The type of the data.
        /// </value>
        public int DataType { get; set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [include in debug dump].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [include in debug dump]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInDebugDump { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="ProjectParam" /> is required.
        /// </summary>
        /// <value>
        ///     <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="ProjectParam" /> is sensitive.
        /// </summary>
        /// <value>
        ///     <c>true</c> if sensitive; otherwise, <c>false</c>.
        /// </value>
        public bool Sensitive { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public string Value { get; set; }
    }
}