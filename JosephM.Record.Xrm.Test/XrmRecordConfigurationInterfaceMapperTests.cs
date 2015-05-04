using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.ObjectMapping.Test;
using JosephM.Record.Xrm.Mappers;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class XrmRecordConfigurationInterfaceMapperTests :
        InterfaceMapperTests<XrmRecordConfigurationInterfaceMapper, IXrmRecordConfiguration, XrmRecordConfiguration>
    {
        [TestMethod]
        public void XrmRecordConfigurationInterfaceMapperTest()
        {
            ClassMapperTest();
        }
    }
}