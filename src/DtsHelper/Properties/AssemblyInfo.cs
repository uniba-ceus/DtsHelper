// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using log4net.Config;


[assembly: AssemblyVersion("4.0.*")]
[assembly: AssemblyInformationalVersion("4.0 RC1")]
[assembly: AssemblyCompany("CEUS")]
[assembly: AssemblyCopyright("Copyright © 2020")]
[assembly: AssemblyTrademark("CEUS")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("de-DE")]

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
[assembly: AssemblyProduct("DtsHelper")]
[assembly: AssemblyTitle("DtsHelper")]
[assembly: AssemblyDescription("DtsHelper library with useful functionality in modern SSIS development.")]
[assembly: AssemblyConfiguration("")]
[assembly: Guid("022708c3-8a56-4c88-8884-934fc6044795")]

// Enable automatic log4net configuration in app.config
[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]