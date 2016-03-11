using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;

namespace JosephM.InstanceComparer
{
    [AllowSaveAndLoad]
    [DisplayName("Instance Compare")]
    public class InstanceComparerRequest : ServiceRequestBase
    {
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        public SavedXrmRecordConfiguration ConnectionOne { get; set; }

        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        public SavedXrmRecordConfiguration ConnectionTwo { get; set; }

        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        //public IEnumerable<InstanceCompareDataCompare> DataComparisons { get; set; }

        public class InstanceCompareDataCompare
        {
            [Hidden]
            public string Type { get { return Type == null ? null : RecordType.Key; } }

            public RecordType RecordType { get; set; }
        }
    }
}