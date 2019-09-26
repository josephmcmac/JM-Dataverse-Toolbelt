using JosephM.CodeGenerator.JavaScriptOptions;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
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
                RecordType = new RecordType(Entities.account, Entities.account),
                AllOptionSetFields = true
            };

            testApplication.ClearTabs();
            var response = testApplication.NavigateAndProcessDialog<JavaScriptOptionsModule, JavaScriptOptionsDialog, JavaScriptOptionsResponse>(request);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Javascript));

            request = new JavaScriptOptionsRequest()
            {
                RecordType = new RecordType(Entities.account, Entities.account),
                AllOptionSetFields = false,
                SpecificOptionSetField = new RecordField(Fields.account_.industrycode, Fields.account_.industrycode)
            };

            testApplication.ClearTabs();
            response = testApplication.NavigateAndProcessDialog<JavaScriptOptionsModule, JavaScriptOptionsDialog, JavaScriptOptionsResponse>(request);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Javascript));

            FileUtility.DeleteFiles(TestingFolder);
        }
    }
}