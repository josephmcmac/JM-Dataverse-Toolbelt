using System.Configuration;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;

namespace JosephM.Core.Test
{
    [TestClass]
    public class AppConfigManagerTests : CoreTest
    {
        [TestMethod]
        [DeploymentItem("app.config")]
        public void AppConfigManagerGetThenSetTests()
        {
            AppConfigManagerResolveTests();
            AppConfigManagerSetConfigurationObjectTests();
        }

        private void AppConfigManagerResolveTests()
        {
            var appconfigManager = new AppConfigManager();
            var testObject = appconfigManager.Resolve<TestObject>();
            Assert.IsNotNull(testObject);
            Assert.IsTrue(testObject.BooleanField);
            Assert.AreEqual("StringValue", testObject.StringField);
            Assert.AreEqual(1, testObject.IntField);
            Assert.AreEqual(TestEnum.Enum1, testObject.EnumField);
            Assert.AreEqual(2, testObject.EnumerableStringField.Count());
            Assert.IsTrue(testObject.EnumerableStringField.Contains("StringValue1"));
            Assert.IsTrue(testObject.EnumerableStringField.Contains("StringValue2"));
            Assert.IsNotNull(testObject.ExcelFileField);
            Assert.AreEqual("Excel.xls", testObject.ExcelFileField.FileName);
            try
            {
                appconfigManager.Resolve<TestObject2>();
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.IsTrue(ex.DisplayString().Contains("JosephM.IncorrectAssemblyName"));
            }

            var testObject3 = appconfigManager.Resolve<TestObject3>();
            Assert.IsNull(testObject3.StringField);
            Assert.IsFalse(testObject3.BooleanField);
            Assert.AreEqual(1, testObject3.IntField);
        }

        private void AppConfigManagerSetConfigurationObjectTests()
        {
            var appconfigManager = new AppConfigManager();
            var testObject = new TestObject();
            testObject.StringField = "NewStringValue";
            testObject.EnumerableStringField = new[]
            {
                "NewStringValue1",
                "NewStringValue2"
            };
            appconfigManager.SetConfigurationSectionObject(testObject);

            var newTestObject = appconfigManager.Resolve<TestObject>();

            Assert.AreEqual(testObject.StringField, newTestObject.StringField);
            Assert.AreEqual(testObject.EnumerableStringField.Count(), newTestObject.EnumerableStringField.Count());
            foreach (var stringValue in testObject.EnumerableStringField)
                Assert.IsTrue(newTestObject.EnumerableStringField.Contains(stringValue));
        }
    }
}