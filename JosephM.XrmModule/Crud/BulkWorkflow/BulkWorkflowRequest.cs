using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.XrmModule.Crud.BulkWorkflow
{
    [Group(Sections.RecordDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.Options, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20)]
    [Group(Sections.Workflow, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30)]
    public class BulkWorkflowRequest : ServiceRequestBase
    {
        private bool _allowExecuteMultiples = true;

        public BulkWorkflowRequest(RecordType recordType, IEnumerable<IRecord> recordsToUpdate)
            : this()
        {
            RecordType = recordType;
            _recordsToUpdate = recordsToUpdate;
        }

        public BulkWorkflowRequest()
        {
            ExecuteMultipleSetSize = 50;
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

        [Group(Sections.Workflow)]
        [DisplayOrder(30)]
        [RequiredProperty]
        [LookupCondition(Fields.workflow_.type, OptionSets.Process.Type.Definition)]
        [LookupCondition(Fields.workflow_.ondemand, true)]
        [LookupCondition(Fields.workflow_.statecode, OptionSets.Process.Status.Activated)]
        [LookupCondition(Fields.workflow_.category, OptionSets.Process.Category.Workflow)]
        [LookupCondition(Fields.workflow_.primaryentity, nameof(RecordType), valueIsProperty: true)]
        [DoNotAllowAdd]
        [ReferencedType(Entities.workflow)]
        [UsePicklist()]
        public Lookup Workflow { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(40)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        public int? ExecuteMultipleSetSize { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(50)]
        [RequiredProperty]
        [MinimumIntValue(0)]
        public int WaitPerMessage { get; set; }

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
            public const string Options = "Options";
            public const string Workflow = "Workflow";
        }
    }
}