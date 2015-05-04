using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Metadata;
using JosephM.ObjectMapping.Test;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.Mappers;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class StringFormatMapperTests : EnumMapperTests<StringFormatMapper, StringFormat, TextFormat>
    {
        [TestMethod]
        public void StringFormatMapperTest()
        {
            EnumMapperTest();
        }
    }
}