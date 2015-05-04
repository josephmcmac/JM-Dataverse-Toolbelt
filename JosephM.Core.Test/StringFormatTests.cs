using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Constants;

namespace JosephM.Core.Test
{
    [TestClass]
    public class StringFormatTests
    {
        [TestMethod]
        public void StringFormatDecimalFormatTests()
        {
            Assert.AreEqual("0", ((decimal) 0).ToString(StringFormats.DecimalFormat));
            Assert.AreEqual("555.555", ((decimal) 555.555).ToString(StringFormats.DecimalFormat));
            Assert.AreEqual("555", ((decimal) 555).ToString(StringFormats.DecimalFormat));
            Assert.AreEqual("0.555", ((decimal) 0.555).ToString(StringFormats.DecimalFormat));
        }

        [TestMethod]
        public void StringFormatMoneyFormatTests()
        {
            Assert.AreEqual("$0.00", ((decimal) 0).ToString(StringFormats.MoneyFormat));
            Assert.AreEqual("$5,555.55", ((decimal) 5555.55).ToString(StringFormats.MoneyFormat));
            Assert.AreEqual("$555.00", ((decimal) 555).ToString(StringFormats.MoneyFormat));
            Assert.AreEqual("$0.55", ((decimal) 0.55).ToString(StringFormats.MoneyFormat));
        }

        [TestMethod]
        public void StringFormatDateFormatTests()
        {
            var datetime = new DateTime(2000, 1, 2, 3, 4, 5);
            Assert.AreEqual("02/01/2000", datetime.ToString(StringFormats.DateFormat));
        }

        [TestMethod]
        public void StringFormatDateTimeFormatTests()
        {
            var datetime = new DateTime(2000, 1, 2, 13, 4, 5);
            Assert.AreEqual("02/01/2000 01:04:05 PM", datetime.ToString(StringFormats.DateTimeFormat));
        }
    }
}