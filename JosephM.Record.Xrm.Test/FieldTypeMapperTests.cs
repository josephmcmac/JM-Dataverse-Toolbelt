using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Metadata;
using JosephM.ObjectMapping.Test;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.Mappers;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class FieldTypeMapperTests : EnumMapperTests<FieldTypeMapper, AttributeTypeCode, RecordFieldType>
    {
        [TestMethod]
        public void FieldTypeMapperTest()
        {
            EnumMapperTest();
        }
    }
}