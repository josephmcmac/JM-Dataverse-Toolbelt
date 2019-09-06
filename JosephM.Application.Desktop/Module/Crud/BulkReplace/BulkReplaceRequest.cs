using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkReplace
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalWrap, 10)]
    [Group(Sections.FieldUpdate, Group.DisplayLayoutEnum.HorizontalWrap, 20)]
    [Group(Sections.AdditionalOptions, Group.DisplayLayoutEnum.HorizontalWrap, 30)]
    public class BulkReplaceRequest : ServiceRequestBase
    {
        private bool _allowExecuteMultiples = true;

        public BulkReplaceRequest(RecordType recordType, IEnumerable<IRecord> recordsToUpdate)
            : this()
        {
            RecordType = recordType;
            _recordsToUpdate = recordsToUpdate;
        }

        public BulkReplaceRequest()
        {
            ExecuteMultipleSetSize = 50;
        }

        private IEnumerable<IRecord> _recordsToUpdate { get; set; }

        public IEnumerable<IRecord> GetRecordsToUpdate()
        {
            return _recordsToUpdate;
        }

        [RecordTypeFor(nameof(FieldToReplaceIn))]
        [Group(Sections.RecordDetails)]
        [DisplayOrder(10)]
        public RecordType RecordType { get; private set; }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _recordsToUpdate?.Count() ?? 0; } }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [LookupCondition(nameof(IFieldMetadata.FieldType), ConditionType.In, new[] { RecordFieldType.String, RecordFieldType.Memo })]
        public RecordField FieldToReplaceIn { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(30)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToReplaceIn))]
        public string OldValue { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(40)]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(FieldToReplaceIn))]
        public string NewValue { get; set; }

        [Group(Sections.AdditionalOptions)]
        [DisplayOrder(50)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        [PropertyInContextByPropertyValue(nameof(AllowExecuteMultiples), true)]
        public int? ExecuteMultipleSetSize { get; set; }

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
            public const string RecordDetails = "Selected Replace Details";
            public const string FieldUpdate = "Field Value To Replace";
            public const string AdditionalOptions = "Additional Options";
        }
    }
}