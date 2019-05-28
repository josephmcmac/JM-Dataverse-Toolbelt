using JosephM.XrmModule.Crud.Validations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.XrmModule.Test
{
    [TestClass]
    public class ValidationPrefixValidationTests
    {
        /// <summary>
        /// Scripts through several scenarios for the saved requests module
        /// </summary>
        [TestMethod]
        public void ValidationPrefixValidationTest()
        {
            var validator = new PrefixValidation();

            Assert.IsTrue(validator.IsValid("ne"));
            Assert.IsTrue(validator.IsValid("new"));
            Assert.IsTrue(validator.IsValid("NEW"));
            Assert.IsTrue(validator.IsValid("New"));
            Assert.IsTrue(validator.IsValid("new1"));

            Assert.IsFalse(validator.IsValid("n"));
            Assert.IsFalse(validator.IsValid("mscrm"));
            Assert.IsFalse(validator.IsValid("mscrm1"));
            Assert.IsFalse(validator.IsValid("mScRm"));
            Assert.IsFalse(validator.IsValid("mScRm1"));
            Assert.IsFalse(validator.IsValid("1new"));
            Assert.IsFalse(validator.IsValid("_new"));
            Assert.IsFalse(validator.IsValid("new#"));
        }
    }
}