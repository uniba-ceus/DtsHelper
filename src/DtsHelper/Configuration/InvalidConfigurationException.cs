// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Configuration
{
    using System;

    /// <summary>
    ///     Indicates a invalid configuration.
    /// </summary>
    [Serializable]
    public class InvalidConfigurationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidConfigurationException" /> class.
        /// </summary>
        public InvalidConfigurationException() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidConfigurationException" /> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public InvalidConfigurationException(string msg) : base(msg) { }
    }
}