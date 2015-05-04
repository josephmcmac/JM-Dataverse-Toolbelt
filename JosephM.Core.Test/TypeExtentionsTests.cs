#region

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class TypeExtentionsTests
    {
        [TestMethod]
        public void TypeExtentionsGetDisplayNameTest()
        {
            Assert.AreEqual(TestConstants.TestObjectClassDisplayName, typeof (TestObject).GetDisplayName());
            Assert.AreEqual(typeof (TestObject2).Name.SplitCamelCase(), typeof (TestObject2).GetDisplayName());
        }

        [TestMethod]
        public void TypeExtentionsHasStringConstructorTest()
        {
            Assert.IsTrue(typeof (TestObject).HasStringConstructor());
            Assert.IsFalse(typeof (TestObject2).HasStringConstructor());
        }

        [TestMethod]
        public void TypeExtentionsHasParameterlessConstructorTest()
        {
            Assert.IsTrue(typeof (TestObject).HasParameterlessConstructor());
            Assert.IsFalse(typeof (NoEmptyConstructorObject).HasParameterlessConstructor());
        }

        [TestMethod]
        public void TypeExtentionsIsTypeOfTest()
        {
            Assert.IsTrue(typeof (TestObject).IsTypeOf(typeof (TestObject)));
            Assert.IsTrue(typeof (TestObjectSubClass).IsTypeOf(typeof (TestObject)));
            Assert.IsFalse(typeof (TestObject2).IsTypeOf(typeof (TestObject)));
        }

        [TestMethod]
        public void TypeExtentionsCreateFromParameterlessConstructorTest()
        {
            Assert.IsNotNull(typeof (TestObject).CreateFromParameterlessConstructor());
        }

        [TestMethod]
        public void TypeExtentionsGetWriteablePropertiesTest()
        {
            var properties = typeof (TestObject).GetWritableProperties();
            Assert.IsTrue(properties.Any(p => p.Name == "SetOnly"));
            Assert.IsTrue(properties.Any(p => p.Name == "GetSet"));
            Assert.IsFalse(properties.Any(p => p.Name == "GetOnly"));
        }

        [TestMethod]
        public void TypeExtentionsGetValidatorAttributesTest()
        {
            var attributes = typeof (TestObject).GetValidatorAttributes("RequiredProperty");
            Assert.AreEqual(1, attributes.Count());
            Assert.IsTrue(attributes.ElementAt(0) is RequiredProperty);
        }
    }
}