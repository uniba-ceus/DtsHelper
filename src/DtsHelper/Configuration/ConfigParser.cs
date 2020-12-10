// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Configuration
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    ///     Parser for configuration file.
    /// </summary>
    public static class ConfigParser
    {
        /// <summary>
        ///     Load configuration from Json file.
        /// </summary>
        /// <param name="fileName">Name of json file..</param>
        /// <returns>
        ///     The application configuration.
        /// </returns>
        public static Config Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new InvalidOperationException($"Die Datei {fileName} existiert nicht.");
            }

            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(fileName));
        }

        /// <summary>
        ///     Saves configuration to Json file.
        /// </summary>
        /// <param name="fileName">Name of json file..</param>
        /// <param name="config"> The application configuration.</param>
        public static void Save(string fileName, Config config)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(config));
        }
    }
}