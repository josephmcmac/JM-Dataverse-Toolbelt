using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkUpdate
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.Options, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    [Group(Sections.FieldUpdate, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30, displayLabel: false)]
    [Group(Sections.FieldUpdate2, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 31, displayLabel: false)]
    [Group(Sections.FieldUpdate3, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 32, displayLabel: false)]
    [Group(Sections.FieldUpdate4, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 33, displayLabel: false)]
    [Group(Sections.FieldUpdate5, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 34, displayLabel: false)]
    public class BulkUpdateRequest : ServiceRequestBase
    {
        private bool _allowExecuteMultiples = true;

        public BulkUpdateRequest(RecordType recordType, IEnumerable<IRecord> recordsToUpdate)
            : this()
        {
            RecordType = recordType;
            _recordsToUpdate = recordsToUpdate;
        }

        public BulkUpdateRequest()
        {
            ExecuteMultipleSetSize = 50;
        }

        private IEnumerable<IRecord> _recordsToUpdate { get; set; }

        public IEnumerable<IRecord> GetRecordsToUpdate()
        {
            return _recordsToUpdate;
        }

        [RecordTypeFor(nameof(FieldToSet))]
        [RecordTypeFor(nameof(FieldToSet2))]
        [RecordTypeFor(nameof(FieldToSet3))]
        [RecordTypeFor(nameof(FieldToSet4))]
        [RecordTypeFor(nameof(FieldToSet5))]
        [Group(Sections.RecordDetails)]
        [DisplayOrder(10)]
        public RecordType RecordType { get; private set; }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _recordsToUpdate?.Count() ?? 0; } }

        [MyDescription("Specify for updates to be submitted the given batch size using execute multple operations")]
        [Group(Sections.Options)]
        [DisplayOrder(21)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        [PropertyInContextByPropertyValue(nameof(AllowExecuteMultiples), true)]
        public int? ExecuteMultipleSetSize { get; set; }

        [MyDescription("Specify for cloud flow, plugin, and workflow logic not to trigger from update operations being performed")]
        [DisplayName("Bypass Flows, Plugins and Workflows")]
        [Group(Sections.Options)]
        [DisplayOrder(22)]
        public bool BypassFlowsPluginsAndWorkflows { get; set; }

        [DisplayName("Field to Update 1")]
        [Group(Sections.FieldUpdate)]
        [DisplayOrder(22)]
        [RequiredProperty]
        [RecordFieldFor(nameof(ValueToSet))]
        public RecordField FieldToSet { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(25)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet))]
        [DisplayName("Field 1 Set to Null")]
        public bool ClearValue { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(30)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet))]
        [PropertyInContextByPropertyValue(nameof(ClearValue), false)]
        [DisplayName("Field 1 Value to Set")]
        public object ValueToSet { get; set; }

        [Hidden]
        public bool Field1Populated
        {
            get {  return FieldToSet != null && (ClearValue || ValueToSet != null); }
        }

        [PropertyInContextByPropertyValue(nameof(Field1Populated), true)]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField2), false)]
        [Group(Sections.FieldUpdate2)]
        [DisplayOrder(35)]
        [RequiredProperty]
        [DisplayName("Add 2nd Field to Update")]
        public bool AddUpdateField2 { get; set; }

        [Group(Sections.FieldUpdate2)]
        [DisplayOrder(40)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField2), true)]
        [RecordFieldFor(nameof(ValueToSet2))]
        [DisplayName("Field to Update 2")]
        public RecordField FieldToSet2 { get; set; }

        [Group(Sections.FieldUpdate2)]
        [DisplayOrder(45)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet2))]
        [DisplayName("Field 2 Set to Null")]
        public bool ClearValue2 { get; set; }

        [Group(Sections.FieldUpdate2)]
        [DisplayOrder(50)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet2))]
        [PropertyInContextByPropertyValue(nameof(ClearValue2), false)]
        [DisplayName("Field 2 Value to Set")]
        public object ValueToSet2 { get; set; }

        [Hidden]
        public bool Field2Populated
        {
            get { return FieldToSet2 != null && (ClearValue2 || ValueToSet2 != null); }
        }

        [PropertyInContextByPropertyValue(nameof(Field2Populated), true)]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField3), false)]
        [Group(Sections.FieldUpdate3)]
        [DisplayOrder(55)]
        [RequiredProperty]
        [DisplayName("Add 3rd Field")]
        public bool AddUpdateField3 { get; set; }

        [Group(Sections.FieldUpdate3)]
        [DisplayOrder(60)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField3), true)]
        [RecordFieldFor(nameof(ValueToSet3))]
        [DisplayName("Field to Update 3")]
        public RecordField FieldToSet3 { get; set; }

        [Group(Sections.FieldUpdate3)]
        [DisplayOrder(65)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet3))]
        [DisplayName("Field 3 Set to Null")]
        public bool ClearValue3 { get; set; }

        [Group(Sections.FieldUpdate3)]
        [DisplayOrder(70)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet3))]
        [PropertyInContextByPropertyValue(nameof(ClearValue3), false)]
        [DisplayName("Field 3 Value to Set")]
        public object ValueToSet3 { get; set; }

        [Hidden]
        public bool Field3Populated
        {
            get { return FieldToSet3 != null && (ClearValue3 || ValueToSet3 != null); }
        }

        [PropertyInContextByPropertyValue(nameof(Field3Populated), true)]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField4), false)]
        [Group(Sections.FieldUpdate4)]
        [DisplayOrder(75)]
        [RequiredProperty]
        [DisplayName("Add 4th Field")]
        public bool AddUpdateField4 { get; set; }

        [Group(Sections.FieldUpdate4)]
        [DisplayOrder(80)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField4), true)]
        [RecordFieldFor(nameof(ValueToSet4))]
        [DisplayName("Field to Update 4")]
        public RecordField FieldToSet4 { get; set; }

        [Group(Sections.FieldUpdate4)]
        [DisplayOrder(85)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet4))]
        [DisplayName("Field 4 Set to Null")]
        public bool ClearValue4 { get; set; }

        [Group(Sections.FieldUpdate4)]
        [DisplayOrder(90)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet4))]
        [PropertyInContextByPropertyValue(nameof(ClearValue4), false)]
        [DisplayName("Field 4 Value to Set")]
        public object ValueToSet4 { get; set; }

        [Hidden]
        public bool Field4Populated
        {
            get { return FieldToSet4 != null && (ClearValue4 || ValueToSet4 != null); }
        }

        [PropertyInContextByPropertyValue(nameof(Field4Populated), true)]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField5), false)]
        [Group(Sections.FieldUpdate5)]
        [DisplayOrder(75)]
        [RequiredProperty]
        [DisplayName("Add 5th Field")]
        public bool AddUpdateField5 { get; set; }

        [Group(Sections.FieldUpdate5)]
        [DisplayOrder(80)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField5), true)]
        [RecordFieldFor(nameof(ValueToSet5))]
        [DisplayName("Field to Update 5")]
        public RecordField FieldToSet5 { get; set; }

        [Group(Sections.FieldUpdate5)]
        [DisplayOrder(85)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet5))]
        [DisplayName("Field 5 Set to Null")]
        public bool ClearValue5 { get; set; }

        [Group(Sections.FieldUpdate5)]
        [DisplayOrder(90)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet5))]
        [PropertyInContextByPropertyValue(nameof(ClearValue5), false)]
        [DisplayName("Field 5 Value to Set")]
        public object ValueToSet5 { get; set; }

        [Hidden]
        public bool AllowExecuteMultiples
        {
            get => _allowExecuteMultiples; set
            {
                _allowExecuteMultiples = value;
                if (!value)
                    ExecuteMultipleSetSize = 1;
            }
        }

        private static class Sections
        {
            public const string RecordDetails = "Bulk Update";
            public const string Options = "Options";
            public const string FieldUpdate = "Field Values To Update";
            public const string FieldUpdate2 = "FieldUpdate2";
            public const string FieldUpdate3 = "FieldUpdate3";
            public const string FieldUpdate4 = "FieldUpdate4";
            public const string FieldUpdate5 = "FieldUpdate5";
        }
    }
}