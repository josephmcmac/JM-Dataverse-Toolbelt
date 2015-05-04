#region

using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Attributes;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class PropertyValidatorTests
    {
        [TestMethod]
        public void PropertyValidatorValidateTests()
        {
            var testObject = new TestObject();

            var validator = new RequiredProperty();
            Assert.IsFalse(validator.IsValid(null, null));
            Assert.IsFalse(validator.IsValid("  ", null));
            Assert.IsTrue(validator.IsValid("Somthing", null));

            Assert.IsNotNull(validator.GetErrorMessage("RequiredProperty", testObject));
        }
    }
}