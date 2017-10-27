using JosephM.Application.ViewModel.SettingTypes;
using JosephM.CodeGenerator.FetchToJavascript;
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
    public class CodeGenerationFetchToJavaScriptModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void CodeGenerationFetchToJavaScriptModuleTest()
        {
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<FetchToJavascriptModule>();

            var request = new FetchToJavascriptRequest()
            {
                Fetch = "<fetchxml>" + "</fetchxml>"
            };

            testApplication.ClearTabs();
            testApplication.NavigateAndProcessDialog<FetchToJavascriptModule, FetchToJavascriptDialog>(request);
            //todo get the response object and verify contains javascript

            FileUtility.DeleteFiles(TestingFolder);
        }
    }
}