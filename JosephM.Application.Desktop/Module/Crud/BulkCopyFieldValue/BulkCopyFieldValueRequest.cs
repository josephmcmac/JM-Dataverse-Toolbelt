using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalWrap, 10)]
    [Group(Sections.FieldCopy, Group.DisplayLayoutEnum.HorizontalWrap, 20)]
    [Group(Sections.Options, Group.DisplayLayoutEnum.HorizontalWrap, 30)]
    public class BulkCopyFieldValueRequest : ServiceRequestBase
    {
        private bool _allowExecuteMultiples = true;

        public BulkCopyFieldValueRequest(RecordType recordType, IEnumerable<IRecord> recordsToUpdate)
            : this()
        {
            RecordType = recordType;
            _recordsToUpdate = recordsToUpdate;
        }

        public BulkCopyFieldValueRequest()
        {
            ExecuteMultipleSetSize = 50;
        }

        private IEnumerable<IRecord> _recordsToUpdate { get; set; }

        public IEnumerable<IRecord> GetRecordsToUpdate()
        {
            return _recordsToUpdate;
        }

        [RecordTypeFor(nameof(SourceField))]
        [RecordTypeFor(nameof(TargetField))]
        [Group(Sections.RecordDetails)]
        [DisplayOrder(10)]
        public RecordType RecordType { get; private set; }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _recordsToUpdate?.Count() ?? 0; } }

        [Group(Sections.FieldCopy)]
        [DisplayOrder(30)]
        [RequiredProperty]
        public RecordField SourceField { get; set; }

        [Group(Sections.FieldCopy)]
        [DisplayOrder(40)]
        [RequiredProperty]
        public RecordField TargetField { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(50)]
        [RequiredProperty]
        public bool CopyIfNull { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(60)]
        [RequiredProperty]
        public bool OverwriteIfPopulated { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(70)]
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
            public const string RecordDetails = "Selected Update Details";
            public const string FieldCopy = "Fields";
            public const string Options = "Options";
        }
    }
}