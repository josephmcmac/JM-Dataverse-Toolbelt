using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Constants;

namespace JosephM.Core.Test
{
    [TestClass]
    public class RegularExpressionsTests
    {
        [TestMethod]
        public void RegularExpressionsIntegerTests()
        {
            var matcher = new Regex(RegularExpressions.IntegerOrEmpty);
            Assert.IsTrue(matcher.IsMatch("0"));
            Assert.IsTrue(matcher.IsMatch("55555555"));
            Assert.IsTrue(matcher.IsMatch("-55555555"));
            Assert.IsFalse(matcher.IsMatch("-55555d555"));
            Assert.IsFalse(matcher.IsMatch("-55555d555.5"));
            Assert.IsFalse(matcher.IsMatch("55555d555.5"));
            Assert.IsTrue(matcher.IsMatch(""));
            Assert.IsFalse(matcher.IsMatch(" "));
        }
    }
}