using JosephM.Application.Desktop.Module.ReleaseCheckModule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JosephM.Application.Desktop.Test
{
    [TestClass]
    public class ReleaseCheckModuleTests
    {
        [TestMethod]
        public void ReleaseCheckModuleTestIsNewerVersion()
        {
            var module = new TestReleaseCheckModule();
            Assert.IsTrue(module.IsNewerVersion("2.4.6.8", "1.9.9.9"));
            Assert.IsTrue(module.IsNewerVersion("2.4.6.8", "2.4.6.7"));
            Assert.IsFalse(module.IsNewerVersion("2.4.6.8", "2.4.6.8"));
            Assert.IsFalse(module.IsNewerVersion("2.4.6.8", "2.5.1.8"));
            Assert.IsTrue(module.IsNewerVersion("2.4.6.8", "1.4.6.8"));
            Assert.IsTrue(module.IsNewerVersion("2", "1.4.6.8"));
            Assert.IsFalse(module.IsNewerVersion("2", "3.4.6.8"));
            Assert.IsFalse(module.IsNewerVersion("2", "3.4.6.8"));
            Assert.IsTrue(module.IsNewerVersion("11.0.0.0", "2.0.0.0"));
            Assert.IsFalse(module.IsNewerVersion("9.0.0.0", "19.0.0.0"));
            Assert.IsFalse(module.IsNewerVersion("2.0.3.0", "2.0.3"));
            Assert.IsFalse(module.IsNewerVersion("2.0.3", "2.0.3.0"));
            try
            {
                Assert.IsFalse(module.IsNewerVersion("what the", "19.0.0.0"));
            }
            catch(Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
        }

        public class TestReleaseCheckModule : GitHubReleaseCheckModule
        {
            public override string Githubusername => "josephmcmac";

            public override string GithubRepository => "testingonly";
        }
    }
}