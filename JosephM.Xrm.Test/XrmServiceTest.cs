#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;
using JosephM.Xrm.Schema;
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
            //DeleteAll(Entities.jmcg_testentity);

            //var multipleEntities = new List<Entity>();
            //for (var i = 0; i < 1100; i++)
            //{
            //    var entity = new Entity(Entities.jmcg_testentity);
            //    entity.SetField("new_name","Execute Multiple " + i);
            //    multipleEntities.Add(entity);
            //}
            //var r = XrmService.CreateMultiple(multipleEntities);
            //var createdEntities = XrmService.RetrieveAllOrClauses(Entities.jmcg_testentity,
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
        public void RetrieveAllOrClauseTest()
        {
            Assert.Inconclusive("Costly test so escaping");

            PrepareTests();
            var orFilters = new List<FilterExpression>();
            for (var i = 0; i < 1000; i++)
            {
                var entity = new Entity(Entities.jmcg_testentity);
                entity.SetField("new_testentitycode", i + "Blah" + i);
                entity.SetField(Fields.jmcg_testentity_.jmcg_string, i + "Blah Blah" + i);
                XrmService.Create(entity);
                var filter = new FilterExpression();
                filter.AddCondition("new_testentitycode", ConditionOperator.Equal, i + "Blah" + i);
                filter.AddCondition(Fields.jmcg_testentity_.jmcg_string, ConditionOperator.Equal, i + "Blah Blah" + i);
                orFilters.Add(filter);
            }
            var entities = XrmService.RetrieveAll(Entities.jmcg_testentity, orFilters);
            Assert.IsTrue(entities.Count() == 1000);
        }

        [TestMethod]
        public void ParseFieldTest()
        {
            PrepareTests(true);
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
                XrmEntity.GetOptionSetValue(XrmService.ParseField(Fields.jmcg_testentity_.statuscode, Entities.jmcg_testentity, "Active")) ==
                    OptionSets.TestEntity.StatusReason.Active);
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField(Fields.jmcg_testentity_.statuscode, Entities.jmcg_testentity,
                    OptionSets.TestEntity.StatusReason.Active)) ==
                OptionSets.TestEntity.StatusReason.Active);
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField(Fields.jmcg_testentity_.statuscode, Entities.jmcg_testentity,
                    XrmEntity.CreateOptionSet(OptionSets.TestEntity.StatusReason.Active))) ==
                OptionSets.TestEntity.StatusReason.Active);
            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.statuscode, Entities.jmcg_testentity, "NOTASTATUS");
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
            var maxDoubleValue = (double) XrmService.GetMaxDoubleValue(Fields.jmcg_testentity_.jmcg_float, Entities.jmcg_testentity);
            var minDoubleValue = (double) XrmService.GetMinDoubleValue(Fields.jmcg_testentity_.jmcg_float, Entities.jmcg_testentity);
            //Valid values
            Assert.IsTrue((double) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_float, Entities.jmcg_testentity, maxDoubleValue) ==
                          maxDoubleValue);
            Assert.IsTrue((double) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_float, Entities.jmcg_testentity, minDoubleValue) ==
                          minDoubleValue);
            Assert.IsTrue((double) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_float, Entities.jmcg_testentity, (double) 5) == 5);
            Assert.IsTrue((double) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_float, Entities.jmcg_testentity, "20") == 20);
            //Outside range values
            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_float, Entities.jmcg_testentity, maxDoubleValue + 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }

            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_float, Entities.jmcg_testentity, minDoubleValue - 1);
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
            var expectedLookup = XrmEntity.CreateLookup(Entities.account, Guid.Empty);
            var actualLookup = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_account, Entities.jmcg_testentity, expectedLookup);
            Assert.IsTrue(XrmEntity.GetLookupGuid(expectedLookup) == Guid.Empty);
            Assert.IsTrue(XrmEntity.GetLookupType(expectedLookup) == Entities.account);
        }

        private void PicklistTest()
        {
            /*
            * Picklist field
            * */
            //Valid values
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_picklist, Entities.jmcg_testentity, "Option 1")) ==
                OptionSets.TestEntity.Picklist.Option1);
            Assert.IsTrue(
                XrmEntity.GetOptionSetValue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_picklist, Entities.jmcg_testentity,
                    OptionSets.TestEntity.Picklist.Option1)) ==
                OptionSets.TestEntity.Picklist.Option1);
            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_picklist, Entities.jmcg_testentity, "NOTANOPTION");
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
            Assert.IsTrue((bool) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_boolean, Entities.jmcg_testentity, true));
            Assert.IsTrue((bool) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_boolean, Entities.jmcg_testentity, false) == false);
        }

        private void MoneyTest()
        {
            /*
             * Money field
             * */
            var maxMoneyValue = (double) XrmService.GetMaxMoneyValue(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity);
            var minMoneyValue = (double) XrmService.GetMinMoneyValue(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity);
            //Valid values
            Assert.IsTrue(
                XrmEntity.GetMoneyValue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity, maxMoneyValue)) ==
                new decimal(maxMoneyValue));
            Assert.IsTrue(
                XrmEntity.GetMoneyValue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity, minMoneyValue)) ==
                new decimal(minMoneyValue));
            Assert.IsTrue(
                XrmEntity.GetMoneyValue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity,
                    new Money(new decimal(6)))) ==
                new decimal(6));
            Assert.IsTrue(
                XrmEntity.GetMoneyValue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity, new decimal(2))) ==
                new decimal(2));
            Assert.IsTrue(XrmEntity.GetMoneyValue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity, "4")) ==
                          new decimal(4));
            //Outside range values
            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity, maxMoneyValue + 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }

            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_money, Entities.jmcg_testentity, minMoneyValue - 1);
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
            var maxDecimalValue = (decimal) XrmService.GetMaxDecimalValue(Fields.jmcg_testentity_.jmcg_decimal, Entities.jmcg_testentity);
            var minDecimalValue = (decimal) XrmService.GetMinDecimalValue(Fields.jmcg_testentity_.jmcg_decimal, Entities.jmcg_testentity);
            //Valid values
            Assert.IsTrue((decimal) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_decimal, Entities.jmcg_testentity, maxDecimalValue) ==
                          maxDecimalValue);
            Assert.IsTrue((decimal) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_decimal, Entities.jmcg_testentity, minDecimalValue) ==
                          minDecimalValue);
            Assert.IsTrue((decimal) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_decimal, Entities.jmcg_testentity, 5) == new decimal(5));
            //Outside range values
            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_decimal, Entities.jmcg_testentity, maxDecimalValue + 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }

            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_decimal, Entities.jmcg_testentity, minDecimalValue - 1);
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
            var maxIntValue = XrmService.GetMaxIntValue(Fields.jmcg_testentity_.jmcg_integer, Entities.jmcg_testentity);
            var minIntValue = XrmService.GetMinIntValue(Fields.jmcg_testentity_.jmcg_integer, Entities.jmcg_testentity);
            //Valid values
            Assert.IsTrue((int) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_integer, Entities.jmcg_testentity, maxIntValue) == maxIntValue);
            Assert.IsTrue((int) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_integer, Entities.jmcg_testentity, minIntValue) == minIntValue);
            Assert.IsTrue((int) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_integer, Entities.jmcg_testentity, "2") == 2);
            //Outside range values
            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_integer, Entities.jmcg_testentity, maxIntValue + 1);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }

            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_integer, Entities.jmcg_testentity, minIntValue - 1);
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
            var maxMemoLength = XrmService.GetMaxLength(Fields.jmcg_testentity_.jmcg_stringmultiline, Entities.jmcg_testentity);
            //Valid values
            var validMemo = ReplicateString("X", maxMemoLength);
            Assert.IsTrue((string) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_stringmultiline, Entities.jmcg_testentity, validMemo) ==
                          validMemo);
            //invalid value
            var exceedMemoLength = ReplicateString("X", maxMemoLength + 1);

            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_stringmultiline, Entities.jmcg_testentity, exceedMemoLength);
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
            var maxTextLength = XrmService.GetMaxLength(Fields.jmcg_testentity_.jmcg_string, Entities.jmcg_testentity);
            //Valid values
            var validString = ReplicateString("X", maxTextLength);
            Assert.IsTrue((string) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_string, Entities.jmcg_testentity, validString) == validString);
            //invalid value
            var exceedLength = ReplicateString("X", maxTextLength + 1);

            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_string, Entities.jmcg_testentity, exceedLength);
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
            Assert.IsTrue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_date, Entities.jmcg_testentity, null) == null);
            var expectedDate = DateTime.Now;
            expectedDate = expectedDate.AddMilliseconds(-1*expectedDate.Millisecond);
            var actualDate = (DateTime) XrmService.ParseField(Fields.jmcg_testentity_.jmcg_date, Entities.jmcg_testentity, expectedDate);
            expectedDate = expectedDate.ToUniversalTime();
            Assert.IsTrue(expectedDate.Hour == actualDate.Hour && expectedDate.Minute == actualDate.Minute &&
                          expectedDate.Second == actualDate.Second && expectedDate.Day == actualDate.Day);
            //valid string value
            var expected = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            expected = expected.AddMilliseconds(-1 * expected.Millisecond);
            var actual =
                (DateTime)
                    XrmService.ParseField(Fields.jmcg_testentity_.jmcg_date, Entities.jmcg_testentity,
                        expected.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern));
            expected = expected.ToUniversalTime();
            Assert.IsTrue(expected.Hour == actual.ToUniversalTime().Hour && expected.Minute == actual.ToUniversalTime().Minute &&
                          expected.Second == actual.ToUniversalTime().Second && expected.Day == actual.ToUniversalTime().Day);
            //empty string value
            Assert.IsTrue(XrmService.ParseField(Fields.jmcg_testentity_.jmcg_date, Entities.jmcg_testentity, "") == null);
            //invalid string value
            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_date, Entities.jmcg_testentity, "BLAH");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
            try
            {
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_date, Entities.jmcg_testentity, "BLAH");
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
                var blah = XrmService.ParseField(Fields.jmcg_testentity_.jmcg_date, Entities.jmcg_testentity, belowMin);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                CheckException(ex);
            }
        }

        [TestMethod]
        public void GetThisSideIdTest()
        {
            PrepareTests(true);
            Assert.IsTrue(XrmService.GetThisSideId(Relationships.jmcg_testentity_.jmcg_testentity_jmcg_testentity.Name, Entities.jmcg_testentity, true) ==
                          "jmcg_testentityidone");
            Assert.IsTrue(XrmService.GetThisSideId(Relationships.jmcg_testentity_.jmcg_testentity_jmcg_testentity.Name, Entities.jmcg_testentity, false) ==
                          "jmcg_testentityidtwo");
            Assert.IsTrue(XrmService.GetThisSideId(Relationships.jmcg_testentity_.jmcg_testentity_account.Name, Entities.jmcg_testentity, false) ==
                          "jmcg_testentityid");
            Assert.IsTrue(XrmService.GetThisSideId(Relationships.jmcg_testentity_.jmcg_testentity_account.Name, Entities.jmcg_testentity, true) ==
                          "jmcg_testentityid");
            Assert.IsTrue(XrmService.GetThisSideId(Relationships.jmcg_testentity_.jmcg_testentity_account.Name, Entities.account, false) == Fields.account_.accountid);
            Assert.IsTrue(XrmService.GetThisSideId(Relationships.jmcg_testentity_.jmcg_testentity_account.Name, Entities.account, true) == Fields.account_.accountid);
        }
    }
}