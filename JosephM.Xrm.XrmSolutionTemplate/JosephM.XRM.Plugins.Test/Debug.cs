using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace $safeprojectname$
{
    //this class just for general debug purposes
    [TestClass]
    public class DebugTests : TemplateXrmTest
    {
        [TestMethod]
        public void Debug()
        {
            var me = XrmService.WhoAmI();
        }
    }
}
