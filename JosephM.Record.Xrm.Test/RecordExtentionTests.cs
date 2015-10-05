using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.Test;
using JosephM.Xrm;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace JosephM.Record.Xrm
{
    public class RecordExtentionTests : XrmRecordTest
    {
        [TestMethod]
        public void IndexMatchingGuidsTest()
        {
            PrepareTests();

            DeleteAllData();

            var e1 = new Entity(Entities.jmcg_testentity);
            e1.SetField(Fields.jmcg_testentity_.jmcg_string, "MATCH1");
            e1 = XrmService.CreateAndRetreive(e1);

            var e1x = new Entity(Entities.jmcg_testentity);
            e1x.SetField(Fields.jmcg_testentity_.jmcg_string, "MATCH1");
            e1x = XrmService.CreateAndRetreive(e1x);

            var e2 = new Entity(Entities.jmcg_testentity);
            e2.SetField(Fields.jmcg_testentity_.jmcg_string, "MATCH2");
            e2 = XrmService.CreateAndRetreive(e2);

            var e3 = new Entity(Entities.jmcg_testentity);
            e3.SetField(Fields.jmcg_testentity_.jmcg_string, "MATCH3");
            e3 = XrmService.CreateAndRetreive(e3);

            var eX = new Entity(Entities.jmcg_testentity);
            eX.SetField(Fields.jmcg_testentity_.jmcg_string, "MATCHX");
            eX = XrmService.CreateAndRetreive(eX);

            var indexed = XrmRecordService.IndexMatchingGuids(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_string,
                new[]
                {
                    "MATCH1", "MATCH2", "MATCH3"
                    ,
                    "NONMATCH"
                });

            Assert.IsTrue(indexed.Count() == 4);
            Assert.IsTrue(indexed["MATCH1"] == e1.Id.ToString() || indexed["MATCH1"] == e1x.Id.ToString());
            Assert.IsTrue(indexed["MATCH2"] == e2.Id.ToString());
            Assert.IsTrue(indexed["MATCH3"] == e3.Id.ToString());
            Assert.IsNull(indexed["NONMATCH"]);
        }
    }
}
