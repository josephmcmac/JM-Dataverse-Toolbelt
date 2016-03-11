using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace JosephM.Xrm.Test
{
    [TestClass]
    public class XrmDebugScript : XrmTest
    {
        [TestMethod]
        public void XrmDebug()
        {

            var relationships = XrmService.RetrieveAllOrClauses("systemform",
                new[]
                {
                    new ConditionExpression("objecttypecode", ConditionOperator.Equal, 1)
                });

            var relationships2 = XrmService.RetrieveAllOrClauses("systemform",
                new[]
                {
                                new ConditionExpression("objecttypecode", ConditionOperator.Equal, "account")
                });

            //var match = relationships.Where(r => r.IntersectEntityName == "roleprivileges");

            var count = relationships.Count();
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