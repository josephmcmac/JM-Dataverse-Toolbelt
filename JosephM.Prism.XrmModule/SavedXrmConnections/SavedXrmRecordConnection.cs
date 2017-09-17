using System;
using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.ObjectMapping;

namespace JosephM.Prism.XrmModule.SavedXrmConnections
{
    public class SavedXrmRecordConfiguration : XrmRecordConfiguration
    {
        [DisplayOrder(1)]
        [UniqueOn]
        [GridWidth(60)]
        public bool Active { get; set; }

        public static SavedXrmRecordConfiguration CreateNew(IXrmRecordConfiguration xrmRecordConfiguration)
        {
            var mapper = new ClassSelfMapper();
            var savedConnection = new SavedXrmRecordConfiguration();
            mapper.Map(xrmRecordConfiguration, savedConnection);
            return savedConnection;
        }
    }
}
