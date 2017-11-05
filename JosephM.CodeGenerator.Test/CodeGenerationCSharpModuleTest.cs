using JosephM.Application.ViewModel.SettingTypes;
using JosephM.CodeGenerator.CSharp;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.CodeGenerator.Test
{
    [TestClass]
    public class CodeGenerationCSharpModuleTest : XrmModuleTest
    {
        [TestMethod]
        public void CodeGenerationCSharpModuleTes()
        {
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<CSharpModule>();

            var request = new CSharpRequest()
            {
                IncludeAllRecordTypes = true,
                Entities = true,
                Fields = true,
                FieldOptions = true,
                Relationships = true,
                SharedOptions = true,
                Actions = true,
                FileName = "Schema",
                Folder = new Folder(TestingFolder),
                Namespace = "Schema"
            };

            var response = testApplication.NavigateAndProcessDialog<CSharpModule, CSharpDialog, CSharpResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            FileUtility.DeleteFiles(TestingFolder);

            //do one other test for generate only one specific type
            request = new CSharpRequest()
            {
                IncludeAllRecordTypes = false,
                Entities = true,
                Fields = true,
                FieldOptions = true,
                Relationships = true,
                SharedOptions = true,
                FileName = "Schema",
                Folder = new Folder(TestingFolder),
                Namespace = "Schema",
                RecordTypes = new[] {new RecordTypeSetting(Entities.account, Entities.account)}
            };

            response = testApplication.NavigateAndProcessDialog<CSharpModule, CSharpDialog, CSharpResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());
        }
    }
}