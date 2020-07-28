using JosephM.Spreadsheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Core.Test
{
    [TestClass]
    public class CsvUtilityTests : CoreTest
    {
        [TestMethod]
        public void CsvUtilityCreateCsvTest()
        {
            var objects = new List<TestObject>();

            objects.Add(new TestObject()
            {
                StringField = "ABC\"\n,'ABC",
                IntField = -1,
                DateField = null,
                BooleanField = false
            });

            for (var i = 0; i < 500; i++)
            {
                var o = new TestObject()
                {
                    StringField = Replicate('A', i),
                    IntField = i,
                    DateField = DateTime.Now,
                    BooleanField = true
                };
                objects.Add(o);
            }

            CsvUtility.CreateCsv(TestingFolder, "CsvObjects" + DateTime.Now.ToFileTime() + "", objects);
        }

        [DeploymentItem("TestCsv.csv")]
        [TestMethod]
        public void CsvUtilityReadCsvTest()
        {
            var rows = CsvUtility.SelectAllRows("TestCsv.csv");
            Assert.IsTrue(rows.Count() == 4);
            var textString = rows.Last().GetFieldAsString("TextStringField");
            Assert.IsTrue(textString.Length >= 300);
            var phoneString = rows.Last().GetFieldAsString("PhoneNumber");
            Assert.IsTrue(phoneString.StartsWith("0"));
        }
    }
}