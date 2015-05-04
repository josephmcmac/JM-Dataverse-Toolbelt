#region

using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Attributes;

#endregion

namespace JosephM.Core.Test
{
    [TestClass]
    public class RecordTypeForTests
    {
        [TestMethod]
        public void RecordTypeForConstructorTests()
        {
            //just to ignore coverage
            var recordTypeFor = new RecordTypeFor("Something");
            Assert.IsNotNull(recordTypeFor);
        }
    }
}