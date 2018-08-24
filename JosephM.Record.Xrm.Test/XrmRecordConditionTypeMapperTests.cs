using JosephM.ObjectMapping.Test;
using JosephM.Record.Query;
using JosephM.Record.Xrm.Mappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class XrmRecordConditionTypeMapperTests :
         EnumMapperTests<ConditionTypeMapper, ConditionType, ConditionOperator>
    {
        [TestMethod]
        public void XrmRecordConditionTypeMapperTest()
        {
            EnumMapperTest();
        }
    }
}