﻿using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkDelete
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.AdditionalOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30, displayLabel: false)]
    public class BulkDeleteRequest : ServiceRequestBase
    {
        private bool _allowExecuteMultiples = true;

        public BulkDeleteRequest(RecordType recordType, IEnumerable<IRecord> recordsToDelete)
            : this()
        {
            RecordType = recordType;
            _recordsToDelete = recordsToDelete;
        }

        public BulkDeleteRequest()
        {
            ExecuteMultipleSetSize = 50;
        }

        private IEnumerable<IRecord> _recordsToDelete { get; set; }

        public IEnumerable<IRecord> GetRecordsToDelete()
        {
            return _recordsToDelete;
        }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(10)]
        public RecordType RecordType { get; private set; }

        [Group(Sections.RecordDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _recordsToDelete?.Count() ?? 0; } }

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

        [MyDescription("Specify for cloud flow, plugin, and workflow logic not to trigger from update operations being performed")]
        [DisplayName("Bypass Flows, Plugins and Workflows")]
        [Group(Sections.AdditionalOptions)]
        [DisplayOrder(60)]
        public bool BypassFlowsPluginsAndWorkflows { get; set; }

        private static class Sections
        {
            public const string RecordDetails = "Bulk Delete";
            public const string FieldUpdate = "Field Value To Update";
            public const string AdditionalOptions = "Additional Options";
        }
    }
}