// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Core
{
    using System.Collections.Generic;
    using Microsoft.SqlServer.Dts.Runtime;
    using Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask;

    /// <summary>
    ///     Simple class holding task context data.
    /// </summary>
    public class TaskContext
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
        ///     Gets or sets the task host.
        /// </summary>
        /// <value>
        ///     The task host.
        /// </value>
        public TaskHost TaskHost { get; set; }

        /// <summary>
        ///     Gets or sets the execute SQL task.
        /// </summary>
        /// <value>
        ///     The execute SQL task.
        /// </value>
        public IDTSExecuteSQL ExecuteSqlTask { get; set; }
    }
}