using JosephM.Xrm.Vsix.Module.PackageSettings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class ValidationClassPrefixValidationTests
    {
        /// <summary>
        /// Scripts through several scenarios for the saved requests module
        /// </summary>
        [TestMethod]
        public void ValidationClassPrefixValidationTest()
        {
            var validator = new ClassPrefixValidation();

            Assert.IsTrue(validator.IsValid("n"));
            Assert.IsTrue(validator.IsValid("ne"));
            Assert.IsTrue(validator.IsValid("new"));
            Assert.IsTrue(validator.IsValid("NEW"));
            Assert.IsTrue(validator.IsValid("New"));
            Assert.IsTrue(validator.IsValid("new1"));
            Assert.IsTrue(validator.IsValid("new1_new"));


            Assert.IsFalse(validator.IsValid("1new"));
            Assert.IsFalse(validator.IsValid("new#"));
        }
    }
}