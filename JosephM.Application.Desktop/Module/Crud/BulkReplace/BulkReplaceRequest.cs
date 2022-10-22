using JosephM.Application.ViewModel.Attributes;
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
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.AdditionalOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    [Group(Sections.FieldUpdate, Group.DisplayLayoutEnum.VerticalCentered, order: 30, displayLabel: false)]
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

        [RecordTypeFor(nameof(FieldsToReplace) + "." + nameof(FieldToReplace.RecordField))]
        [Group(Sections.RecordDetails)]
        [DisplayOrder(10)]
        public RecordType RecordType { get; private set; }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _recordsToUpdate?.Count() ?? 0; } }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(30)]
        [RequiredProperty]
        public IEnumerable<FieldToReplace> FieldsToReplace { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(40)]
        [RequiredProperty]
        public IEnumerable<ReplacementText> ReplacementTexts { get; set; }

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

        [DoNotAllowGridOpen]
        [BulkAddFieldFunction]
        public class FieldToReplace
        {
            [LookupCondition(nameof(IFieldMetadata.FieldType), ConditionType.In, new[] { RecordFieldType.String, RecordFieldType.Memo })]
            [RequiredProperty]
            public RecordField RecordField { get; set; }
        }

        [DoNotAllowGridOpen]
        public class ReplacementText
        {
            [DisplayOrder(10)]
            [RequiredProperty]
            public string OldText { get; set; }

            [DisplayOrder(20)]
            [RequiredProperty]
            [PropertyInContextByPropertyValue(nameof(ReplaceWithEmptyString), false)]
            public string NewText { get; set; }

            [DisplayOrder(30)]
            [RequiredProperty]
            public bool ReplaceWithEmptyString { get; set; }
        }

        private static class Sections
        {
            public const string RecordDetails = "Bulk Replace Field Text";
            public const string FieldUpdate = "Field Value To Replace";
            public const string AdditionalOptions = "Additional Options";
        }
    }
}