// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Core
{
    using System.IO;
    using Microsoft.SqlServer.Dts.Runtime;

    /// <summary>
    ///     Simple class for package context data.
    /// </summary>
    public class PackageContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageContext" /> class.
        /// </summary>
        /// <param name="canonicalPath">The canonical path.</param>
        /// <param name="packageXML">The package XML.</param>
        public PackageContext(string canonicalPath, Package packageXML)
        {
            CanonicalPath = canonicalPath;
            Name = Path.GetFileNameWithoutExtension(canonicalPath);
            PackageXML = packageXML;
        }

        /// <summary>
        ///     Name of full path to dtsx package.
        /// </summary>
        /// <value>
        ///     The canonical path.
        /// </value>
        public string CanonicalPath { set; get; }

        /// <summary>
        ///     Name of dtsx package without file extension.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { set; get; }

        /// <summary>
        ///     Dtsx package object.
        /// </summary>
        /// <value>
        ///     The package XML.
        /// </value>
        public Package PackageXML { set; get; }
    }
}