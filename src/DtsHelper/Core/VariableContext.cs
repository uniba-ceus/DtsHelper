// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Core
{
    using System.Collections.Generic;
    using Microsoft.SqlServer.Dts.Runtime;

    /// <summary>
    ///     Simple class holding variable context data.
    /// </summary>
    public class VariableContext
    {
        /// <summary>
        ///     Gets or sets the package.
        /// </summary>
        /// <value>
        ///     The package.
        /// </value>
        public Package Package { get; set; }

        /// <summary>
        ///     Gets or sets the project parameters.
        /// </summary>
        /// <value>
        ///     The project parameters.
        /// </value>
        public List<ProjectParam> ProjectParams { get; set; }

        /// <summary>
        ///     Gets or sets the variable.
        /// </summary>
        /// <value>
        ///     The variable.
        /// </value>
        public Variable Variable { get; set; }
    }
}