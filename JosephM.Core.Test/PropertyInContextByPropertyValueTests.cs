#region

using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class PropertyInContextByPropertyValueTests
    {
        /// <summary>
        /// Tests for in ctext by property value
        /// </summary>
        [TestMethod]
        public void PropertyInContextByPropertyValueTest()
        {
            //property in context by value should ba able to specify a lookup name in case id is not known at compile time
            var testObject = new TestPropertyValidator();
            Assert.IsFalse(testObject.IsInContext(nameof(TestPropertyValidator.DependantOnLookupName)));
            testObject.LookupField = new Lookup("foo", "foo", "Record Name");
            Assert.IsTrue(testObject.IsInContext(nameof(TestPropertyValidator.DependantOnLookupName)));
            testObject.LookupField = new Lookup("foo", "foo", "Other Record Name");
            Assert.IsFalse(testObject.IsInContext(nameof(TestPropertyValidator.DependantOnLookupName)));
        }

        public class TestPropertyValidator
        {
            public Lookup LookupField { get; set; }

            [PropertyInContextByPropertyValue(nameof(LookupField), "Record Name")]
            public bool DependantOnLookupName { get; set; }
        }
    }
}