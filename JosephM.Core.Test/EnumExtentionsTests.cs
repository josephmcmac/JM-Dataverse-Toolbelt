#region

using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class EnumExtentionsTests
    {
        [TestMethod]
        public void EnumExtentionsGetDisplayStringTest()
        {
            Assert.AreEqual(TestConstants.Enum1Description, TestEnum.Enum1.GetDisplayString());
            Assert.AreEqual(TestEnum.Enum2.ToString().SplitCamelCase(), TestEnum.Enum2.GetDisplayString());
        }
    }
}