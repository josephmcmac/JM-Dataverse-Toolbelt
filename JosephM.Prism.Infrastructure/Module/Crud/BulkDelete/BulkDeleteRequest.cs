using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Prism.Module.Crud.BulkDelete
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalWrap, 10)]
    public class BulkDeleteRequest : ServiceRequestBase
    {
        public BulkDeleteRequest(RecordType recordType, IEnumerable<IRecord> recordsToUpdate)
        {
            RecordType = recordType;
            _recordsToUpdate = recordsToUpdate;
        }

        public BulkDeleteRequest()
        {

        }

        private IEnumerable<IRecord> _recordsToUpdate { get; set; }

        public IEnumerable<IRecord> GetRecordsToUpdate()
        {
            return _recordsToUpdate;
        }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(10)]
        public RecordType RecordType { get; private set; }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _recordsToUpdate?.Count() ?? 0; } }

        private static class Sections
        {
            public const string RecordDetails = "Selected Deletion Details";
        }
    }
}