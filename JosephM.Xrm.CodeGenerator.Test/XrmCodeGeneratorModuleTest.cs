using System;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Xrm.CodeGenerator.Prism;
using JosephM.Xrm.CodeGenerator.Service;

namespace JosephM.Xrm.CodeGenerator.Test
{
    [TestClass]
    public class CodeGeneratorModuleTests : ServiceRequestModuleTest<XrmCodeGeneratorModule, XrmCodeGeneratorDialog, XrmCodeGeneratorService, CodeGeneratorRequest, CodeGeneratorResponse, CodeGeneratorResponseItem>
    {
        [TestMethod]
        public void TestTextSearchSettingsDialogTest()
        {
            //wouldn't copy assembly to folder unless explicitly used where actually uses elsewhere
            ISavedXrmConnections something = null;
            Assert.Inconclusive("Need To Load Xrm Service To Lookup Service");
            ExecuteTest();
        }
    }
}
