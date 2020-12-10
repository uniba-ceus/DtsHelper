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
[assembly: AssemblyTitle("DtsHelper.Test")]
[assembly: AssemblyDescription("Test project for DtsHelper library")]
[assembly: AssemblyConfiguration("")]
[assembly: Guid("2e8919f4-5660-4aa4-b24d-ca3d1942404c")]

// Enable automatic log4net configuration in app.config
[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]