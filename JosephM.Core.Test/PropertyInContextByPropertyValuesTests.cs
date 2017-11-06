#region

using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class PropertyInContextByPropertyValuesTests
    {
        /// <summary>
        /// Tests for in ctext by property value
        /// </summary>
        [TestMethod]
        public void PropertyInContextByPropertyValuesTest()
        {
            //property in context by value should ba able to specify a lookup name in case id is not known at compile time
            var testObject = new TestPropertyValidator();
            Assert.IsFalse(testObject.IsInContext(nameof(TestPropertyValidator.DependantOnLookupName)));
            testObject.LookupField = new Lookup("foo", "foo", "Record Name 1");
            Assert.IsTrue(testObject.IsInContext(nameof(TestPropertyValidator.DependantOnLookupName)));
            testObject.LookupField = new Lookup("foo", "foo", "Other Record Name");
            Assert.IsFalse(testObject.IsInContext(nameof(TestPropertyValidator.DependantOnLookupName)));
            testObject.LookupField = new Lookup("foo", "foo", "Record Name 2");
            Assert.IsTrue(testObject.IsInContext(nameof(TestPropertyValidator.DependantOnLookupName)));
        }

        public class TestPropertyValidator
        {
            public Lookup LookupField { get; set; }

            [PropertyInContextByPropertyValues(nameof(LookupField), "Record Name 1", "Record Name 2")]
            public bool DependantOnLookupName { get; set; }
        }
    }
}