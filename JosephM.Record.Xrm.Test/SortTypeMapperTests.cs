using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using JosephM.ObjectMapping.Test;
using JosephM.Record.Query;
using JosephM.Record.Xrm.Mappers;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class SortTypeMapperTests : EnumMapperTests<SortTypeMapper, SortType, OrderType>
    {
        [TestMethod]
        public void SortTypeMapperTest()
        {
            EnumMapperTest();
        }
    }
}