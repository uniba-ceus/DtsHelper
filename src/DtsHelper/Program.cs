// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper
{
    using DtsHelper.Apps;
    using log4net;

    /// <summary>
    ///     Main application entry class.
    /// </summary>
    public class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        ///     Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            _log.Debug("Start der Anwendung.");

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run(args);

            _log.Debug("Beenden der Anwendung.");
        }
    }
}