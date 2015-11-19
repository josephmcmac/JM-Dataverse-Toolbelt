using System.Linq;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.CodeGenerator.Service;
using JosephM.CodeGenerator.Xrm;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.CodeGenerator.Test
{
    [TestClass]
    public class CodeGeneratorModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void CodeGeneratorTest()
        {
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            //okay script through generation of the three types

            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<XrmCodeGeneratorModule>();

            //first script generation of C# entities and fields
            var request = new CodeGeneratorRequest()
            {
                AllRecordTypes = true,
                FileName = "Schema",
                Folder = new Folder(TestingFolder),
                Namespace = "Schema",
                Type = CodeGeneratorType.CSharpMetadata
            };

            testApplication.NavigateAndProcessDialog<XrmCodeGeneratorModule, XrmCodeGeneratorDialog>(request);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            FileUtility.DeleteFiles(TestingFolder);

            //now script generation of javascript options
            request = new CodeGeneratorRequest()
            {
                AllRecordTypes = true,
                FileName = "JS",
                Folder = new Folder(TestingFolder),
                Namespace = "JS",
                Type = CodeGeneratorType.JavaScriptOptionSets
            };

            testApplication.ClearTabs();
            testApplication.NavigateAndProcessDialog<XrmCodeGeneratorModule, XrmCodeGeneratorDialog>(request);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            FileUtility.DeleteFiles(TestingFolder);
            //now script generation of C# options
            request = new CodeGeneratorRequest()
            {
                AllRecordTypes = true,
                FileName = "OptionSets",
                Folder = new Folder(TestingFolder),
                Namespace = "OptionSets",
                Type = CodeGeneratorType.CSharpMetadata
            };

            testApplication.ClearTabs();
            testApplication.NavigateAndProcessDialog<XrmCodeGeneratorModule, XrmCodeGeneratorDialog>(request);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            FileUtility.DeleteFiles(TestingFolder);
            //do one other test for genarte only one specific type
            request = new CodeGeneratorRequest()
            {
                AllRecordTypes = false,
                FileName = "Schema",
                Folder = new Folder(TestingFolder),
                Namespace = "Schema",
                Type = CodeGeneratorType.CSharpMetadata,
                RecordTypes = new[] {new RecordTypeSetting(Entities.account, Entities.account)}
            };

            testApplication.NavigateAndProcessDialog<XrmCodeGeneratorModule, XrmCodeGeneratorDialog>(request);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());
        }
    }
}