using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Prism.Infrastructure.Module.Crud.BulkUpdate
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalWrap, 10)]
    [Group(Sections.FieldUpdate, Group.DisplayLayoutEnum.HorizontalWrap, 20)]
    public class BulkUpdateRequest : ServiceRequestBase
    {
        public BulkUpdateRequest(RecordType recordType, IEnumerable<string> recordsToUpdate)
        {
            RecordType = recordType;
            _recordsToUpdate = recordsToUpdate;
        }

        public BulkUpdateRequest()
        {

        }

        //todo could extend to multiple fields

        private IEnumerable<string> _recordsToUpdate { get; set; }

        public IEnumerable<string> GetRecordsToUpdate()
        {
            return _recordsToUpdate;
        }

        [RecordTypeFor(nameof(FieldToSet))]
        [Group(Sections.RecordDetails)]
        [DisplayOrder(10)]
        public RecordType RecordType { get; private set; }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(20)]
        public int? RecordCount { get { return _recordsToUpdate?.Count(); } }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [RecordFieldFor(nameof(ValueToSet))]
        public RecordField FieldToSet { get; set; }

        [Group(Sections.FieldUpdate)]
        [DisplayOrder(20)]
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
            public const string RecordDetails = "Record Details";
            public const string FieldUpdate = "Field Update";
        }
    }
}