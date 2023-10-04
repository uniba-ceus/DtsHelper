// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Test
{
    using System.IO;
    using System.Linq;
    using DtsHelper.Configuration;
    using DtsHelper.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DtsProcessorTests
    {
        private const string TestConfigFileName = "config_test.json";

        [TestMethod]
        public void TestGetProjectParams()
        {
            var dtsProcessor = new DtsProcessor(ReadTestConfig());
            var paramList = dtsProcessor.LoadProjectParams();
            Assert.IsTrue(paramList != null && paramList.Count > 0);
            Assert.IsTrue(paramList.FirstOrDefault(p => p.Name == "Uni") != null);
        }

        [TestMethod]
        public void TestInjectVariables()
        {
            var config = ReadTestConfig();
            config.Commands.ChangeAppName = false;
            config.Commands.ConvertToUTF8 = false;
            config.Commands.FormatSQL = false;
            config.Commands.SetFileConnectionToVariable = false;

            config.Commands.CreateVariables = true;
            config.Commands.DeleteVariables = true;
            config.Commands.InjectVariables = true;

            var dtsProcessor = new DtsProcessor(config);
            dtsProcessor.LoadPackages();
            dtsProcessor.DeleteVariables();
            dtsProcessor.CreateVariables();
            dtsProcessor.InjectVariables();
            dtsProcessor.SavePackages();

            Assert.IsTrue(true);
        }

        
        [TestMethod]
        public void TestSearchPathCombine()
        { 
            var root = "Root";
            var patterns = new []{"<Namespace>/<Package>/<Task>.sql", "<Package>/<Task>.sql", "<Namespace>/<Task>.sql", "<Task>.sql"};
            var ns = "TestNamespace";
            var pck = "TestPackage";
            var taskName = "Test Task";

            foreach(var p in patterns)
            {
                var pattern = p;
                pattern = pattern.Replace("<Namespace>", ns);
                pattern = pattern.Replace("<Package>", pck);
                pattern = pattern.Replace("<Task>",taskName);
                var path = Path.Combine(root, pattern);
                Assert.IsTrue(!string.IsNullOrEmpty(path));
            }
        }

        private Config ReadTestConfig()
        {
            return ConfigParser.Load(TestConfigFileName);
        }
    }
}