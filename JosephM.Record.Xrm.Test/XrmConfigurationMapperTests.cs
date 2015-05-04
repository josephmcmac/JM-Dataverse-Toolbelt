using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.ObjectMapping.Test;
using JosephM.Record.Xrm.Mappers;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class XrmConfigurationMapperTests :
        ClassMapperTests<XrmConfigurationMapper, XrmRecordConfiguration, XrmConfiguration>
    {
        [TestMethod]
        public void XrmConfigurationMapperTest()
        {
            ClassMapperTest();
        }
    }
}