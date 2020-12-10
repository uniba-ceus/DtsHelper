// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Apps
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    ///     This class provides information about the running application. It uses Assembly.GetExecutingAssembly for
    ///     determining default assembly. For libraries you should use Assembly.GetEntryAssembly().
    /// </summary>
    public sealed class AppInfo
    {
        private readonly Assembly _assembly;

        /// <summary>
        ///     Initializes the <see cref="AppInfo" /> class.
        /// </summary>
        static AppInfo()
        {
            Default = new AppInfo(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AppInfo" /> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public AppInfo(Assembly assembly)
        {
            _assembly = assembly;
        }

        /// <summary>
        ///     The default instance for this application.
        /// </summary>
        public static AppInfo Default { get; private set; }

        /// <summary>
        ///     Gets the path for the executable file that started the application, not including the executable name.
        /// </summary>
        /// <value>
        ///     The application path.
        /// </value>
        public string ApplicationPath => GetApplicationPath();

        /// <summary>
        ///     Gets the assembly.
        /// </summary>
        /// <value>
        ///     The assembly.
        /// </value>
        public Assembly Assembly => GetAssembly();

        /// <summary>
        ///     Gets the company of the application.
        /// </summary>
        /// <value>
        ///     The company.
        /// </value>
        public string Company => GetCompany();

        /// <summary>
        ///     Gets the copyright information of the application.
        /// </summary>
        /// <value>
        ///     The copyright.
        /// </value>
        public string Copyright => GetCopyright();

        /// <summary>
        ///     Gets the description information of the application.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description => GetDescription();

        /// <summary>
        ///     Gets the name of the developer. This is configured in the GeneralSettings.
        /// </summary>
        /// <value>
        ///     The name of the developer.
        /// </value>
        public string DeveloperName => GetDeveloperName();

        /// <summary>
        ///     Gets the name of the file.
        /// </summary>
        /// <value>
        ///     The name of the file.
        /// </value>
        public string FileName => GetFileName();

        /// <summary>
        ///     Gets the product name of the application.
        /// </summary>
        /// <value>
        ///     The name of the product.
        /// </value>
        public string ProductName => GetProductName();

        /// <summary>
        ///     Gets the title information of the application.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string Title => GetTitle();

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public AppVersion Version => GetVersion();

        /// <summary>
        ///     Gets the version number of the application.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public string VersionString => GetVersionString();

        /// <summary>
        ///     Creates AppInfo from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static AppInfo From(Assembly assembly)
        {
            return new AppInfo(assembly);
        }

        /// <summary>
        ///     Resets the default instance for this application.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public static void Reset(Assembly assembly)
        {
            Default = new AppInfo(assembly);
        }

        /// <summary>
        ///     Gets the application path.
        /// </summary>
        /// <returns></returns>
        private string GetApplicationPath()
        {
            return Path.GetDirectoryName(GetAssembly().Location);
        }

        /// <summary>
        ///     Gets the assembly. If assembly is null it will return default assembly.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly.</param>
        /// <returns></returns>
        private Assembly GetAssembly(Assembly resourceAssembly = null)
        {
            return resourceAssembly ?? _assembly;
        }

        /// <summary>
        ///     Gets the company.
        /// </summary>
        /// <returns></returns>
        private string GetCompany()
        {
            var attribute =
                (AssemblyCompanyAttribute) Attribute.GetCustomAttribute(GetAssembly(),
                    typeof(AssemblyCompanyAttribute));
            return attribute != null ? attribute.Company : "";
        }

        /// <summary>
        ///     Gets the copyright.
        /// </summary>
        /// <returns></returns>
        private string GetCopyright()
        {
            var attribute =
                (AssemblyCopyrightAttribute) Attribute.GetCustomAttribute(GetAssembly(),
                    typeof(AssemblyCopyrightAttribute));
            return attribute != null ? attribute.Copyright : "";
        }

        /// <summary>
        ///     Gets the description.
        /// </summary>
        /// <returns></returns>
        private string GetDescription()
        {
            var attribute =
                (AssemblyDescriptionAttribute) Attribute.GetCustomAttribute(GetAssembly(),
                    typeof(AssemblyDescriptionAttribute));
            return attribute != null ? attribute.Description : "";
        }

        /// <summary>
        ///     Gets the name of the developer.
        /// </summary>
        /// <returns></returns>
        private string GetDeveloperName()
        {
            return AppConstants.DeveloperName;
        }

        /// <summary>
        ///     Gets the file name.
        /// </summary>
        /// <returns></returns>
        private string GetFileName()
        {
            return GetAssembly().GetName().Name;
        }

        /// <summary>
        ///     Gets the name of the product.
        /// </summary>
        /// <returns></returns>
        private string GetProductName()
        {
            var attribute =
                (AssemblyProductAttribute) Attribute.GetCustomAttribute(GetAssembly(),
                    typeof(AssemblyProductAttribute));
            return attribute != null ? attribute.Product : "";
        }

        /// <summary>
        ///     Gets the title.
        /// </summary>
        /// <returns></returns>
        private string GetTitle()
        {
            var attribute =
                (AssemblyTitleAttribute) Attribute.GetCustomAttribute(GetAssembly(), typeof(AssemblyTitleAttribute));
            return attribute != null
                ? attribute.Title
                : Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        }

        /// <summary>
        ///     Gets the version as complex type for part selection.
        /// </summary>
        /// <returns></returns>
        private AppVersion GetVersion()
        {
            return AppVersion.Cast(VersionString);
        }

        /// <summary>
        ///     Gets the version as string like "1.0.1.232".
        /// </summary>
        /// <returns></returns>
        private string GetVersionString()
        {
            return GetAssembly().GetName().Version.ToString();
        }
    }
}