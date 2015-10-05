using System.Linq;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace JosephM.Xrm.Test
{
    [TestClass]
    public class XrmDebugScript : XrmTest
    {
        [TestMethod]
        public void XrmDebug()
        {
            //var lead = CreateTestRecord("lead");

            //var anote = CreateTestRecord("annotation",
            //    new System.Collections.Generic.Dictionary<string, object>()
            //    {
            //        { "objectid", lead.ToEntityReference() },
            //        { "subject", "Note Subject"}
            //    });

            //var account = CreateAccount();

            //anote.SetField("objectid", account.ToEntityReference());
            //anote = UpdateFieldsAndRetreive(anote, "objectid");
        }
    }
}