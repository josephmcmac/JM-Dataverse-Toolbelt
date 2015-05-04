using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using JosephM.Core.FieldType;
using JosephM.ObjectMapping.Test;
using JosephM.Record.Xrm.Mappers;

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class LookupMapperTests : ClassMapperTests<LookupMapper, EntityReference, Lookup>
    {
        [TestMethod]
        public void LookupMapperTest()
        {
            ClassMapperTest();
        }
    }
}