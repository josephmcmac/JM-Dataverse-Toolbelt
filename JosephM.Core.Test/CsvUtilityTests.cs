using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;
using JosephM.Core.Utility;

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
            Assert.Inconclusive("Need to set the file TestCsv.csv Copy To Output Directory which breaks VSIX build");

            CsvUtility.ConstructTextSchema(null, "TestCsv.csv");
            var rows = CsvUtility.SelectAllRows("TestCsv.csv");
            Assert.IsTrue(rows.Count() == 3);
            var textString = rows.Last().GetFieldAsString("TextStringField");
            Assert.IsTrue(textString.Length >= 300);
            var phoneString = rows.Last().GetFieldAsString("PhoneNumber");
            Assert.IsTrue(phoneString.StartsWith("0"));
        }
    }
}