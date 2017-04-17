using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;

namespace JosephM.RecordCounts.Exporter
{
    [Group(Sections.Folder, true, 10)]
    [Group(Sections.RecordOwnerOptions, true, 20)]
    [Group(Sections.RecordTypes, true, 30)]
    [DisplayName("Record Count")]
    public class RecordCountsRequest : ServiceRequestBase
    {
        [DisplayOrder(10)]
        [Group(Sections.Folder)]
        [RequiredProperty]
        public Folder Folder { get; set; }

        [DisplayOrder(100)]
        [DisplayName("Only Count Records Owned By Specific User")]
        [Group(Sections.RecordOwnerOptions)]
        [RequiredProperty]
        public bool OnlyIncludeSelectedOwner { get; set; }

        [DisplayOrder(110)]
        [DisplayName("User")]
        [Group(Sections.RecordOwnerOptions)]
        [PropertyInContextByPropertyValue("OnlyIncludeSelectedOwner", true)]
        [ReferencedType(Entities.systemuser)]
        [UsePicklist]
        [RequiredProperty]
        public Lookup Owner { get; set; }

        [DisplayOrder(120)]
        [DisplayName("Group Counts By Record Owner")]
        [Group(Sections.RecordOwnerOptions)]
        [PropertyInContextByPropertyValue("OnlyIncludeSelectedOwner", false)]
        [RequiredProperty]
        public bool GroupCountsByOwner { get; set; }

        [DisplayOrder(200)]
        [DisplayName("Include Total For All Record Types")]
        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        public bool AllRecordTypes { get; set; }

        [DisplayOrder(210)]
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