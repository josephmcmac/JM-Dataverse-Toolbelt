using JosephM.XrmModule.Crud.Validations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.XrmModule.Test
{
    [TestClass]
    public class ValidationSolutionOrPublisherNameValidationTests
    {
        /// <summary>
        /// Scripts through several scenarios for the saved requests module
        /// </summary>
        [TestMethod]
        public void ValidationSolutionOrPublisherNameValidationTest()
        {
            var validator = new SolutionOrPublisherNameValidation();

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