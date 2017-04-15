using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;

namespace JosephM.RecordCounts.Exporter
{
    [Group(Sections.Folder, true)]
    [Group(Sections.RecordOwnerOptions, true)]
    [Group(Sections.RecordTypes, true)]
    [DisplayName("Record Count")]
    public class RecordCountsRequest : ServiceRequestBase
    {
        [Group(Sections.Folder)]
        [RequiredProperty]
        public Folder Folder { get; set; }

        [DisplayName("Only Count Records Owned By Specific User")]
        [Group(Sections.RecordOwnerOptions)]
        [RequiredProperty]
        public bool OnlyIncludeSelectedOwner { get; set; }

        [DisplayName("User")]
        [Group(Sections.RecordOwnerOptions)]
        [PropertyInContextByPropertyValue("OnlyIncludeSelectedOwner", true)]
        [ReferencedType(Entities.systemuser)]
        [UsePicklist]
        [RequiredProperty]
        public Lookup Owner { get; set; }

        [DisplayName("Group Counts By Record Owner")]
        [Group(Sections.RecordOwnerOptions)]
        [PropertyInContextByPropertyValue("OnlyIncludeSelectedOwner", false)]
        [RequiredProperty]
        public bool GroupCountsByOwner { get; set; }

        [DisplayName("Include Total For All Record Types")]
        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        public bool AllRecordTypes { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("AllRecordTypes", false)]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }

        private static class Sections
        {
            public const string Folder = "Select The Folder to Save The Generated CSV To";
            public const string RecordOwnerOptions = "Record Owner Options";
            public const string RecordTypes = "Record Types";
        }
    }
}