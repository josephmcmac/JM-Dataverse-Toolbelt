using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;

namespace JosephM.RecordCounts.Exporter
{
    [DisplayName("Record Count")]
    public class RecordCountsRequest : ServiceRequestBase
    {
        public RecordCountsRequest()
        {
        }

        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [RequiredProperty]
        public bool OnlyIncludeSelectedOwner { get; set; }

        [PropertyInContextByPropertyValue("OnlyIncludeSelectedOwner", true)]
        [ReferencedType(Entities.systemuser)]
        [UsePicklist]
        [RequiredProperty]
        public Lookup Owner { get; set; }

        [PropertyInContextByPropertyValue("OnlyIncludeSelectedOwner", false)]
        [RequiredProperty]
        public bool GroupCountsByOwner { get; set; }

        [RequiredProperty]
        public bool AllRecordTypes { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("AllRecordTypes", false)]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }
    }
}