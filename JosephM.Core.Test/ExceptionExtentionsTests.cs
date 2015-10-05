#region

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class ExceptionExtentionsTests
    {
        [TestMethod]
        public void ExceptionGetDisplayStringTest()
        {
            var exception3 = new NotSupportedException("Exception 3");
            var exception2 = new NotImplementedException("Exception 2", exception3);
            var exception1 = new NullReferenceException("Exception 1", exception2);

            var display = exception1.DisplayString();
            Assert.IsTrue(display.Contains("Exception 1"));
            Assert.IsTrue(display.Contains("Exception 2"));
            Assert.IsTrue(display.Contains("Exception 3"));
            Assert.IsTrue(display.Contains(typeof (NotSupportedException).Name));
            Assert.IsTrue(display.Contains(typeof (NotImplementedException).Name));
            Assert.IsTrue(display.Contains(typeof (NullReferenceException).Name));
        }
    }
}