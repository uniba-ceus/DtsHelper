// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void TestRunProgram()
        {
            var args = new[] {"--config=config_test.json", "--run"};
            Program.Main(args);
            Assert.IsTrue(true);
        }
    }
}