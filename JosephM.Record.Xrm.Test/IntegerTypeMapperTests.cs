using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Metadata;
using JosephM.ObjectMapping.Test;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.Mappers;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class IntegerTypeMapperTests : EnumMapperTests<IntegerTypeMapper, IntegerType, IntegerFormat>
    {
        [TestMethod]
        public void IntegerTypeMapperTest()
        {
            EnumMapperTest();
        }
    }
}