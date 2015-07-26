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
            var entity = new Entity("new_testentity");
            var fields = new[]
            {
                "new_testboolean",
                "new_testdate",
                "new_testdecimal",
                "new_testfloat",
                "new_testinteger",
                "new_testmoney",
                "new_testpicklist",
                "new_teststring",
                "new_teststringmultiline",
            };
            foreach(var field in fields)
                entity.SetField(field, CreateNewEntityFieldValue(field, entity.LogicalName, entity));

            entity = CreateAndRetrieve(entity);
        }
    }
}