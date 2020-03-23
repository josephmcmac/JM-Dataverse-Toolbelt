using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkUpdate
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalWrap, order: 10)]
    [Group(Sections.FieldUpdate, Group.DisplayLayoutEnum.HorizontalWrap, order: 20)]
    [Group(Sections.FieldUpdate2, Group.DisplayLayoutEnum.HorizontalWrap, order: 21, displayLabel: false)]
    [Group(Sections.FieldUpdate3, Group.DisplayLayoutEnum.HorizontalWrap, order: 22, displayLabel: false)]
    [Group(Sections.FieldUpdate4, Group.DisplayLayoutEnum.HorizontalWrap, order: 23, displayLabel: false)]
    [Group(Sections.FieldUpdate5, Group.DisplayLayoutEnum.HorizontalWrap, order: 24, displayLabel: false)]
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

        [Group(Sections.RecordDetails)]
        [DisplayOrder(21)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        [PropertyInContextByPropertyValue(nameof(AllowExecuteMultiples), true)]
        public int? ExecuteMultipleSetSize { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(22)]
        [RequiredProperty]
        [RecordFieldFor(nameof(ValueToSet))]
        public RecordField FieldToSet { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(25)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet))]
        public bool ClearValue { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(30)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet))]
        [PropertyInContextByPropertyValue(nameof(ClearValue), false)]
        public object ValueToSet { get; set; }

        [Group(Sections.FieldUpdate2)]
        [DisplayOrder(35)]
        [RequiredProperty]
        [DisplayName("Add 2nd Field")]
        public bool AddUpdateField2 { get; set; }

        [Group(Sections.FieldUpdate2)]
        [DisplayOrder(40)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(AddUpdateField2), true)]
        [RecordFieldFor(nameof(ValueToSet2))]
        public RecordField FieldToSet2 { get; set; }

        [Group(Sections.FieldUpdate2)]
        [DisplayOrder(45)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet2))]
        public bool ClearValue2 { get; set; }

        [Group(Sections.FieldUpdate2)]
        [DisplayOrder(50)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet2))]
        [PropertyInContextByPropertyValue(nameof(ClearValue2), false)]
        public object ValueToSet2 { get; set; }

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
        public RecordField FieldToSet3 { get; set; }

        [Group(Sections.FieldUpdate3)]
        [DisplayOrder(65)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet3))]
        public bool ClearValue3 { get; set; }

        [Group(Sections.FieldUpdate3)]
        [DisplayOrder(70)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet3))]
        [PropertyInContextByPropertyValue(nameof(ClearValue3), false)]
        public object ValueToSet3 { get; set; }

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
        public RecordField FieldToSet4 { get; set; }

        [Group(Sections.FieldUpdate4)]
        [DisplayOrder(85)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet4))]
        public bool ClearValue4 { get; set; }

        [Group(Sections.FieldUpdate4)]
        [DisplayOrder(90)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet4))]
        [PropertyInContextByPropertyValue(nameof(ClearValue4), false)]
        public object ValueToSet4 { get; set; }

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
        public RecordField FieldToSet5 { get; set; }

        [Group(Sections.FieldUpdate5)]
        [DisplayOrder(85)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet5))]
        public bool ClearValue5 { get; set; }

        [Group(Sections.FieldUpdate5)]
        [DisplayOrder(90)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToSet5))]
        [PropertyInContextByPropertyValue(nameof(ClearValue5), false)]
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
            public const string RecordDetails = "Selected Update Details";
            public const string FieldUpdate = "Field Values To Update";
            public const string FieldUpdate2 = "FieldUpdate2";
            public const string FieldUpdate3 = "FieldUpdate3";
            public const string FieldUpdate4 = "FieldUpdate4";
            public const string FieldUpdate5 = "FieldUpdate5";
        }
    }
}