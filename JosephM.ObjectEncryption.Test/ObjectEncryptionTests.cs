using JosephM.Application.Desktop.Test;
using JosephM.Core.FieldType;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.TestModule.ObjectEncrypt;
using JosephM.Record.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.ObjectEncryption.Test
{
    [TestClass]
    public class ObjectEncryptionTests : CoreTest
    {
        [TestMethod]
        public void ObjectEncryptionTest()
        {
            FileUtility.DeleteFiles(TestingFolder);

            var application = TestApplication.CreateTestApplication();
            application.AddModule<ObjectEncryptModule<TestObjectEncryptDialog, TestClassToEncrypt>>();

            var instance = new TestClassToEncrypt();
            PopulateObject(instance);

            application.NavigateAndProcessDialog<ObjectEncryptModule<TestObjectEncryptDialog, TestClassToEncrypt>, TestObjectEncryptDialog>(new object[] { instance, new ObjectEncryptToFolder() { SaveToFolder = new Folder(TestingFolder)}});

            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());
        }
    }
}
