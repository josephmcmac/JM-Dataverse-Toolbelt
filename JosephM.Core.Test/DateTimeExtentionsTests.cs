#region

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class DateTimeExtentionsTests
    {
        [TestMethod]
        public void DateTimeExtentionsGetAge()
        {
            Assert.AreEqual(20, new DateTime(1994, 10, 5).GetAge(new DateTime(2014, 10, 5)));
            Assert.AreEqual(19, new DateTime(1994, 10, 5).GetAge(new DateTime(2014, 10, 4)));
            Assert.AreEqual(20, new DateTime(1994, 10, 5).GetAge(new DateTime(2014, 10, 5)));
            Assert.AreEqual(19, new DateTime(1994, 10, 5).GetAge(new DateTime(2014, 9, 5)));
            Assert.AreEqual(20, new DateTime(1994, 10, 5).GetAge(new DateTime(2014, 11, 5)));
        }

        [TestMethod]
        public void DateTimeExtentionsGetYearsDuration()
        {
            Assert.AreEqual(1, new DateTime(2010, 1, 1).GetYearsDuration(new DateTime(2010, 12, 31)));
            Assert.AreEqual(2, new DateTime(2010, 1, 1).GetYearsDuration(new DateTime(2011, 12, 31)));
            Assert.AreEqual(3, new DateTime(2010, 1, 1).GetYearsDuration(new DateTime(2012, 12, 31)));
            Assert.AreEqual(1, new DateTime(2010, 10, 05).GetYearsDuration(new DateTime(2011, 10, 05)));
            Assert.AreEqual(2, new DateTime(2010, 10, 05).GetYearsDuration(new DateTime(2012, 10, 05)));
        }
    }
}