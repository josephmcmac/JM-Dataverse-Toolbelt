using JosephM.Application.ViewModel.SettingTypes;
using JosephM.CodeGenerator.CSharp;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm.Test;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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
            VerifyCompiles(response.CSharpCode);
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
                RecordTypes = new[] { new RecordTypeSetting(Entities.account, Entities.account) }
            };

            response = testApplication.NavigateAndProcessDialog<CSharpModule, CSharpDialog, CSharpResponse>(request);
            Assert.IsFalse(response.HasError);
            VerifyCompiles(response.CSharpCode);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());
        }


        private void VerifyCompiles(string cSharpCode)
        {
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, "foo.exe", true);
            parameters.GenerateExecutable = false;
            var results = csc.CompileAssemblyFromSource(parameters, cSharpCode);
            Assert.IsFalse(results.Errors.Cast<CompilerError>().Any(), results.Errors.Cast<CompilerError>().Any() ? results.Errors.Cast<CompilerError>().First().ErrorText : null);
        }
    }
}