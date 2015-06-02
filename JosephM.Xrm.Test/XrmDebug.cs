using System.Linq;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Client;

namespace JosephM.Xrm.Test
{
    [TestClass]
    public class XrmDebugScript : XrmTest
    {
        [TestMethod]
        public void XrmDebug()
        {
            //var claendars = XrmService.RetrieveAllEntityType("calendar");

            //var subjects = XrmService.RetrieveAllEntityType("subject");
            //var toDelete = subjects.Where(s => s.GetStringField("title") != "Default Subject");

            //foreach (var entity in toDelete)
            //{
            //    XrmService.Delete(entity);
            //}
        }
    }
}