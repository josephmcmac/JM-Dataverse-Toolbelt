using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Prism.Module.Crud.BulkUpdate
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalWrap, 10)]
    [Group(Sections.FieldUpdate, Group.DisplayLayoutEnum.HorizontalWrap, 20)]
    public class BulkUpdateRequest : ServiceRequestBase
    {
        public BulkUpdateRequest(RecordType recordType, IEnumerable<IRecord> recordsToUpdate)
        {
            RecordType = recordType;
            _recordsToUpdate = recordsToUpdate;
        }

        public BulkUpdateRequest()
        {

        }

        private IEnumerable<IRecord> _recordsToUpdate { get; set; }

        public IEnumerable<IRecord> GetRecordsToUpdate()
        {
            return _recordsToUpdate;
        }

        [RecordTypeFor(nameof(FieldToSet))]
        [Group(Sections.RecordDetails)]
        [DisplayOrder(10)]
        public RecordType RecordType { get; private set; }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _recordsToUpdate?.Count() ?? 0; } }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(20)]
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


        private static class Sections
        {
            public const string RecordDetails = "Selected Update Details";
            public const string FieldUpdate = "Field Value To Update";
        }
    }
}