#region

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class ObjectExtentionsTests
    {
        [TestMethod]
        public void ObjectExtentionsIsInContextTests()
        {
            var testObject = new TestObject();
            Assert.IsNull(testObject.BooleanNullableField);
            Assert.IsFalse(testObject.IsInContext("ValidForBooleanNullableFieldTrue"));
            testObject.BooleanNullableField = false;
            Assert.IsFalse(testObject.IsInContext("ValidForBooleanNullableFieldTrue"));
            testObject.BooleanNullableField = true;
            Assert.IsTrue(testObject.IsInContext("ValidForBooleanNullableFieldTrue"));

            Assert.IsNull(testObject.EnumFieldNullable);
            Assert.IsFalse(testObject.IsInContext("ValidForEnumFieldNullable3Or4"));
            testObject.EnumFieldNullable = TestEnum.Enum1;
            Assert.IsFalse(testObject.IsInContext("ValidForEnumFieldNullable3Or4"));
            testObject.EnumFieldNullable = TestEnum.Enum3;
            Assert.IsTrue(testObject.IsInContext("ValidForEnumFieldNullable3Or4"));
            testObject.EnumFieldNullable = TestEnum.Enum2;
            Assert.IsFalse(testObject.IsInContext("ValidForEnumFieldNullable3Or4"));
            testObject.EnumFieldNullable = TestEnum.Enum4;
            Assert.IsTrue(testObject.IsInContext("ValidForEnumFieldNullable3Or4"));

            testObject.EnumFieldNullable = null;
            Assert.IsFalse(testObject.IsInContext("ValidForEnumNullableNotNull"));
            testObject.EnumFieldNullable = TestEnum.Enum1;
            Assert.IsTrue(testObject.IsInContext("ValidForEnumNullableNotNull"));

            Assert.IsTrue(testObject.IsInContext("NoInContextAttributes"));
        }

        [TestMethod]
        public void ObjectExtentionsGetPropertyValueTests()
        {
            var testObject = new TestObject();
            try
            {
                testObject.GetPropertyValue("PropertyDoesntExist");
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                testObject.GetPropertyValue("SetOnly");
                Assert.Fail();
            }
            catch (MemberAccessException)
            {
            }
            Assert.IsNull(testObject.GetPropertyValue("GetOnly"));
            Assert.IsNull(testObject.GetPropertyValue("GetSet"));
            testObject.GetSet = "Something";
            Assert.AreEqual("Something", testObject.GetPropertyValue("GetSet"));
        }

        [TestMethod]
        public void ObjectExtentionsSetPropertyValueTests()
        {
            var testObject = new TestObject();
            try
            {
                testObject.SetPropertyValue("PropertyDoesntExist", "Something");
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                testObject.SetPropertyValue("GetOnly", "Something");
                Assert.Fail();
            }
            catch (MemberAccessException)
            {
            }
            testObject.SetPropertyValue("SetOnly", "Something");
            testObject.SetPropertyValue("GetSet", "Something");
            Assert.AreEqual("Something", testObject.GetPropertyValue("GetSet"));
        }
    }
}