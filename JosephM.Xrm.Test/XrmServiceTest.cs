#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

#endregion

namespace JosephM.Xrm.Test
{
    [TestClass]
    public class XrmServiceTest : XrmTest
    {
        [TestMethod]
        public void ExecuteMultipleTest()
        {
            Assert.Inconclusive("Costly test so escaping");

            PrepareTests();
            //DeleteAll("new_testentity");

            //var multipleEntities = new List<Entity>();
            //for (var i = 0; i < 1100; i++)
            //{
            //    var entity = new Entity("new_testentity");
            //    entity.SetField("new_name","Execute Multiple " + i);
            //    multipleEntities.Add(entity);
            //}
            //var r = XrmService.CreateMultiple(multipleEntities);
            //var createdEntities = XrmService.RetrieveAllOrClauses("new_testentity",
            //    r.Select(
            //            ri =>
            //            new ConditionExpression("new_testentityid", ConditionOperator.Equal,
            //                                    ((CreateResponse) ri.Response).id))).ToArray();

            //for (var i=0; i < createdEntities.Count(); i++)
            //{
            //    if(i % 2 == 1)
            //        createdEntities.ElementAt(i).SetField("new_name", "Updated Multiple " + i);
            //    else
            //        createdEntities.ElementAt(i).SetField("new_name", 1);
            //}

            //XrmService.UpdateMultiple(createdEntities, new string[] { "new_name"});
        }


        //[TestMethod]
        //public void TestReconnect()
        //{
        //    PrepareTests();

        //    var fakeInnerService = new FakeOrganizationService(new MessageSecurityException("Test token expired"));
        //    var service = new XrmService(fakeInnerService, Controller);
        //    service.XrmConfiguration = XrmConfiguration;

        //    var ping = service.WhoAmI();
        //    Assert.IsNotNull(ping);
        //}

        [TestMethod]
        public void IndexMatchingGuidsTest()
        {
            PrepareTests();

            DeleteAllData();

            var e1 = new Entity("new_testentity");
            e1.SetField("new_teststring", "MATCH1");
            e1 = XrmService.CreateAndRetreive(e1);

            var e1x = new Entity("new_testentity");
            e1x.SetField("new_teststring", "MATCH1");
            e1x = XrmService.CreateAndRetreive(e1x);

            var e2 = new Entity("new_testentity");
            e2.SetField("new_teststring", "MATCH2");
            e2 = XrmService.CreateAndRetreive(e2);

            var e3 = new Entity("new_testentity");
            e3.SetField("new_teststring", "MATCH3");
            e3 = XrmService.CreateAndRetreive(e3);

            var eX = new Entity("new_testentity");
            eX.SetField("new_teststring", "MATCHX");
            eX = XrmService.CreateAndRetreive(eX);

            var indexed = XrmService.IndexMatchingGuids("new_testentity", "new_teststring",
                new[]
                {
                    "MATCH1", "MATCH2", "MATCH3"
                    ,
                    "NONMATCH"
                });

            Assert.IsTrue(indexed.Count() == 4);
            Assert.IsTrue(indexed["MATCH1"].Value == e1.Id || indexed["MATCH1"].Value == e1x.Id);
            Assert.IsTrue(indexed["MATCH2"].Value == e2.Id);
            Assert.IsTrue(indexed["MATCH3"].Value == e3.Id);
            Assert.IsFalse(indexed["NONMATCH"].HasValue);
        }

        [TestMethod]
        public void RetrieveAllOrClauseTest()
        {
            Assert.Inconclusive("Costly test so escaping");

            PrepareTests();
            var orFilters = new List<FilterExpression>();
            for (var i = 0; i < 1000; i++)
            {
                var entity = new Entity("new_testentity");
                entity.SetField("new_testentitycode", i + "Blah" + i);
                entity.SetField("new_teststring", i + "Blah Blah" + i);
                XrmService.Create(entity);
                var filter = new FilterExpression();
                filter.AddCondition("new_testentitycode", ConditionOperator.Equal, i + "Blah" + i);
                filter.AddCondition("new_teststring", ConditionOperator.Equal, i + "Blah Blah" + i);
                orFilters.Add(filter);
            }
            var entities = XrmService.RetrieveAll("new_testentity", orFilters);
            Assert.IsTrue(entities.Count() == 1000);
        }

        [TestMethod]
        public void ParseFieldTest()
        {
            PrepareTests();
            //testing fields getting converted to crm values
            RunTestFunctions(
                new Action[]
                {
                    DateTimeTest,
                    StringTest,
                    MemoTest,
                    IntegerTest,
                    DecimalTest,
                    MoneyTest,
                    BooleanTest,
                    PicklistTest,
                    LookupTest,
                    DoubleTest,
                    StatusTest
                });
        }

        private void StatusTest()
        {
            /*
            * Status field
            * */
            //Valid values
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField("statuscode", "new_testentity", "Active")) ==
                TestEntityConstants.Statusses.TestStatus);
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField("statuscode", "new_testentity",
                    TestEntityConstants.Statusses.TestStatus)) ==
                TestEntityConstants.Statusses.TestStatus);
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField("statuscode", "new_testentity",
                    XrmEntity.CreateOptionSet(
                        TestEntityConstants.Statusses.TestStatus))) ==
                TestEntityConstants.Statusses.TestStatus);
            try
            {
                var blah = XrmService.ParseField("statuscode", "new_testentity", "NOTASTATUS");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private void DoubleTest()
        {
            /*
             * Double field
             * */
            var maxDoubleValue = (double) XrmService.GetMaxDoubleValue("new_testfloat", "new_testentity");
            var minDoubleValue = (double) XrmService.GetMinDoubleValue("new_testfloat", "new_testentity");
            //Valid values
            Assert.IsTrue((double) XrmService.ParseField("new_testfloat", "new_testentity", maxDoubleValue) ==
                          maxDoubleValue);
            Assert.IsTrue((double) XrmService.ParseField("new_testfloat", "new_testentity", minDoubleValue) ==
                          minDoubleValue);
            Assert.IsTrue((double) XrmService.ParseField("new_testfloat", "new_testentity", (double) 5) == 5);
            Assert.IsTrue((double) XrmService.ParseField("new_testfloat", "new_testentity", "20") == 20);
            //Outside range values
            try
            {
                var blah = XrmService.ParseField("new_testfloat", "new_testentity", maxDoubleValue + 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }

            try
            {
                var blah = XrmService.ParseField("new_testfloat", "new_testentity", minDoubleValue - 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private void LookupTest()
        {
            /*
            * Lookup field
            * */
            //Valid values
            var expectedLookup = XrmEntity.CreateLookup("account", Guid.Empty);
            var actualLookup = XrmService.ParseField("new_account", "new_testentity", expectedLookup);
            Assert.IsTrue(XrmEntity.GetLookupGuid(expectedLookup) == Guid.Empty);
            Assert.IsTrue(XrmEntity.GetLookupType(expectedLookup) == "account");
        }

        private void PicklistTest()
        {
            /*
            * Picklist field
            * */
            //Valid values
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField("new_testpicklist", "new_testentity", "Option 1")) ==
                TestEntityConstants.TestPicklist.Option1);
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField("new_testpicklist", "new_testentity",
                    TestEntityConstants.TestPicklist.Option1)) ==
                TestEntityConstants.TestPicklist.Option1);
            try
            {
                var blah = XrmService.ParseField("new_testpicklist", "new_testentity", "NOTANOPTION");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private void BooleanTest()
        {
            /*
            * Boolean field
            * */
            //Valid values
            Assert.IsTrue((bool) XrmService.ParseField("new_testboolean", "new_testentity", true));
            Assert.IsTrue((bool) XrmService.ParseField("new_testboolean", "new_testentity", false) == false);
        }

        private void MoneyTest()
        {
            /*
             * Money field
             * */
            var maxMoneyValue = (double) XrmService.GetMaxMoneyValue("new_testmoney", "new_testentity");
            var minMoneyValue = (double) XrmService.GetMinMoneyValue("new_testmoney", "new_testentity");
            //Valid values
            Assert.IsTrue(
                XrmEntity.GetMoneyValue(XrmService.ParseField("new_testmoney", "new_testentity", maxMoneyValue)) ==
                new decimal(maxMoneyValue));
            Assert.IsTrue(
                XrmEntity.GetMoneyValue(XrmService.ParseField("new_testmoney", "new_testentity", minMoneyValue)) ==
                new decimal(minMoneyValue));
            Assert.IsTrue(
                XrmEntity.GetMoneyValue(XrmService.ParseField("new_testmoney", "new_testentity",
                    new Money(new decimal(6)))) ==
                new decimal(6));
            Assert.IsTrue(
                XrmEntity.GetMoneyValue(XrmService.ParseField("new_testmoney", "new_testentity", new decimal(2))) ==
                new decimal(2));
            Assert.IsTrue(XrmEntity.GetMoneyValue(XrmService.ParseField("new_testmoney", "new_testentity", "4")) ==
                          new decimal(4));
            //Outside range values
            try
            {
                var blah = XrmService.ParseField("new_testmoney", "new_testentity", maxMoneyValue + 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }

            try
            {
                var blah = XrmService.ParseField("new_testmoney", "new_testentity", minMoneyValue - 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private void DecimalTest()
        {
            /*
             * Decimal field
             * */
            var maxDecimalValue = (decimal) XrmService.GetMaxDecimalValue("new_testdecimal", "new_testentity");
            var minDecimalValue = (decimal) XrmService.GetMinDecimalValue("new_testdecimal", "new_testentity");
            //Valid values
            Assert.IsTrue((decimal) XrmService.ParseField("new_testdecimal", "new_testentity", maxDecimalValue) ==
                          maxDecimalValue);
            Assert.IsTrue((decimal) XrmService.ParseField("new_testdecimal", "new_testentity", minDecimalValue) ==
                          minDecimalValue);
            Assert.IsTrue((decimal) XrmService.ParseField("new_testdecimal", "new_testentity", 5) == new decimal(5));
            //Outside range values
            try
            {
                var blah = XrmService.ParseField("new_testdecimal", "new_testentity", maxDecimalValue + 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }

            try
            {
                var blah = XrmService.ParseField("new_testdecimal", "new_testentity", minDecimalValue - 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private void IntegerTest()
        {
            /*
             * Integer field
             * */
            var maxIntValue = XrmService.GetMaxIntValue("new_testinteger", "new_testentity");
            var minIntValue = XrmService.GetMinIntValue("new_testinteger", "new_testentity");
            //Valid values
            Assert.IsTrue((int) XrmService.ParseField("new_testinteger", "new_testentity", maxIntValue) == maxIntValue);
            Assert.IsTrue((int) XrmService.ParseField("new_testinteger", "new_testentity", minIntValue) == minIntValue);
            Assert.IsTrue((int) XrmService.ParseField("new_testinteger", "new_testentity", "2") == 2);
            //Outside range values
            try
            {
                var blah = XrmService.ParseField("new_testinteger", "new_testentity", maxIntValue + 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }

            try
            {
                var blah = XrmService.ParseField("new_testinteger", "new_testentity", minIntValue - 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private void MemoTest()
        {
            /*
             * Memo field
             * */
            //
            var maxMemoLength = XrmService.GetMaxLength("new_teststringmultiline", "new_testentity");
            //Valid values
            var validMemo = ReplicateString("X", maxMemoLength);
            Assert.IsTrue((string) XrmService.ParseField("new_teststringmultiline", "new_testentity", validMemo) ==
                          validMemo);
            //invalid value
            var exceedMemoLength = ReplicateString("X", maxMemoLength + 1);

            try
            {
                var blah = XrmService.ParseField("new_teststringmultiline", "new_testentity", exceedMemoLength);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private void StringTest()
        {
            /*
             * String field
             * */
            //
            var maxTextLength = XrmService.GetMaxLength("new_teststring", "new_testentity");
            Assert.IsTrue(maxTextLength == 100);
            //Valid values
            var validString = ReplicateString("X", maxTextLength);
            Assert.IsTrue((string) XrmService.ParseField("new_teststring", "new_testentity", validString) == validString);
            //invalid value
            var exceedLength = ReplicateString("X", maxTextLength + 1);

            try
            {
                var blah = XrmService.ParseField("new_teststring", "new_testentity", exceedLength);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private void DateTimeTest()
        {
            /*
             * DateTime field
             * */
            //valid date
            Assert.IsTrue(XrmService.ParseField("new_testdate", "new_testentity", null) == null);
            var expectedDate = DateTime.Now;
            var actualDate = (DateTime) XrmService.ParseField("new_testdate", "new_testentity", expectedDate);
            Assert.IsTrue(expectedDate.Hour == actualDate.Hour && expectedDate.Minute == actualDate.Minute &&
                          expectedDate.Second == actualDate.Second && expectedDate.Day == actualDate.Day);
            //valid string value
            var expected = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            var actual =
                (DateTime)
                    XrmService.ParseField("new_testdate", "new_testentity",
                        expected.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern));
            Assert.IsTrue(expected.Hour == actual.ToUniversalTime().Hour && expected.Minute == actual.ToUniversalTime().Minute &&
                          expected.Second == actual.ToUniversalTime().Second && expected.Day == actual.ToUniversalTime().Day);
            //empty string value
            Assert.IsTrue(XrmService.ParseField("new_testdate", "new_testentity", "") == null);
            //invalid string value
            try
            {
                var blah = XrmService.ParseField("new_testdate", "new_testentity", "BLAH");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
            try
            {
                var blah = XrmService.ParseField("new_testdate", "new_testentity", "BLAH");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
            //DateTime below minimum
            var belowMin = DateTime.SpecifyKind(new DateTime(1900, 1, 1), DateTimeKind.Utc).AddMilliseconds(-1);
            try
            {
                var blah = XrmService.ParseField("new_testdate", "new_testentity", belowMin);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        private static string ReplicateString(string stringToReplicate, int times)
        {
            var stringer = new StringBuilder();
            for (var i = 0; i < times; i++)
                stringer.Append(stringToReplicate);
            return stringer.ToString();
        }

        [TestMethod]
        public void GetThisSideIdTest()
        {
            PrepareTests();

            Assert.IsTrue(XrmService.GetThisSideId("new_testentity_new_testentity", "new_testentity", true) ==
                          "new_testentityidone");
            Assert.IsTrue(XrmService.GetThisSideId("new_testentity_new_testentity", "new_testentity", false) ==
                          "new_testentityidtwo");
            Assert.IsTrue(XrmService.GetThisSideId("new_testentity_account", "new_testentity", false) ==
                          "new_testentityid");
            Assert.IsTrue(XrmService.GetThisSideId("new_testentity_account", "new_testentity", true) ==
                          "new_testentityid");
            Assert.IsTrue(XrmService.GetThisSideId("new_testentity_account", "account", false) == "accountid");
            Assert.IsTrue(XrmService.GetThisSideId("new_testentity_account", "account", true) == "accountid");
        }
    }
}