using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;

namespace JosephM.RecordCounts
{
    [DisplayName("Record Counts")]
    [Instruction("A report will output with counts of records")]
    [AllowSaveAndLoad]
    [Group(Sections.CountOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20)]
    public class RecordCountsRequest : ServiceRequestBase
    {
        public RecordCountsRequest()
        {
            AllRecordTypes = true;
        }

        [GridWidth(150)]
        [DisplayOrder(10)]
        [DisplayName("Include Total For All Record Types")]
        [Group(Sections.CountOptions)]
        [RequiredProperty]
        public bool AllRecordTypes { get; set; }

        [GridWidth(150)]
        [DisplayOrder(20)]
        [DisplayName("Only Count Records Owned By Specific User")]
        [Group(Sections.CountOptions)]
        [RequiredProperty]
        public bool OnlyIncludeSelectedOwner { get; set; }

        [DisplayOrder(30)]
        [DisplayName("User")]
        [Group(Sections.CountOptions)]
        [PropertyInContextByPropertyValue("OnlyIncludeSelectedOwner", true)]
        [ReferencedType(Entities.systemuser)]
        [UsePicklist]
        [RequiredProperty]
        public Lookup Owner { get; set; }

        [GridWidth(150)]
        [DisplayOrder(40)]
        [DisplayName("Group Counts By Record Owner")]
        [Group(Sections.CountOptions)]
        [PropertyInContextByPropertyValue("OnlyIncludeSelectedOwner", false)]
        [RequiredProperty]
        public bool GroupCountsByOwner { get; set; }


        [GridWidth(500)]
        [DisplayOrder(210)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("AllRecordTypes", false)]
        public IEnumerable<RecordTypeSetting> RecordTypesToInclude { get; set; }

        private static class Sections
        {
            public const string CountOptions = "Count Options";
        }
    }
}