using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Record.Metadata;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class XrmRecordServiceTests : XrmRecordTest
    {
        [TestMethod]
        public void XrmRecordServiceGetViewsTest()
        {
            PrepareTests();

            var views = XrmRecordService.GetViews("account");
            var lookupView = views.First(v => v.ViewType == ViewType.LookupView);
            Assert.IsNotNull(lookupView);
        }
    }
}