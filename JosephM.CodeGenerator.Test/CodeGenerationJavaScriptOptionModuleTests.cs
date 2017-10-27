using JosephM.Application.ViewModel.SettingTypes;
using JosephM.CodeGenerator.JavaScriptOptions;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.CodeGenerator.Test
{
    [TestClass]
    public class CodeGenerationJavaScriptOptionModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void CodeGenerationJavaScriptOptionModuleTest()
        {
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<JavaScriptOptionsModule>();

            var request = new JavaScriptOptionsRequest()
            {
                NamespaceOfTheJavaScriptObject = "accountJS",
                RecordType = new RecordType(Entities.account, Entities.account),
                AllOptionSetFields = true
            };

            testApplication.ClearTabs();
            testApplication.NavigateAndProcessDialog<JavaScriptOptionsModule, JavaScriptOptionsDialog>(request);
            //todo get the response object and verify contains javascript

            FileUtility.DeleteFiles(TestingFolder);
        }
    }
}