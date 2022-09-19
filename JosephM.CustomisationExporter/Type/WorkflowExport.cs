using JosephM.Core.Attributes;

namespace JosephM.CustomisationExporter.Type
{
    public class WorkflowExport
    {
        public WorkflowExport(string state, string category, string name, string primaryEntity, string owner, string mode, string scope, string runAs, string onDemand, string subProcess, string triggerOnCreate, string onUpdateAttributes, string updateStage, string triggerOnDelete, string deleteStage, string isTransacted, string syncworkflowlogOnFailure, string asyncAutoDelete, string businessProcessType)
        {
            State = state;
            Category = category;
            Name = name;
            PrimaryEntity = primaryEntity;
            Owner = owner;
            Mode = mode;
            Scope = scope;
            RunAs = runAs;
            OnDemand = onDemand;
            SubProcess = subProcess;
            TriggerOnCreate = triggerOnCreate;
            OnUpdateAttributes = onUpdateAttributes;
            UpdateStage = updateStage;
            TriggerOnDelete = triggerOnDelete;
            DeleteStage = deleteStage;
            IsTransacted = isTransacted;
            SyncworkflowlogOnFailure = syncworkflowlogOnFailure;
            AsyncAutoDelete = asyncAutoDelete;
            BusinessProcessType = businessProcessType;
        }

        [DisplayOrder(10)]
        public string State { get; private set; }

        [DisplayOrder(20)]
        public string Category { get; private set; }

        [DisplayOrder(30)]
        public string Name { get; private set; }

        [DisplayOrder(40)]
        public string PrimaryEntity { get; private set; }

        [DisplayOrder(50)]
        public string Owner { get; private set; }

        [DisplayOrder(60)]
        public string Mode { get; private set; }

        [DisplayOrder(70)]
        public string Scope { get; private set; }

        [DisplayOrder(80)]
        public string RunAs { get; private set; }

        [DisplayOrder(90)]
        public string OnDemand { get; private set; }

        [DisplayOrder(100)]
        public string SubProcess { get; private set; }

        [DisplayOrder(110)]
        public string TriggerOnCreate { get; private set; }

        [DisplayOrder(120)]
        public string OnUpdateAttributes { get; private set; }

        [DisplayOrder(130)]
        public string UpdateStage { get; private set; }

        [DisplayOrder(140)]
        public string TriggerOnDelete { get; private set; }

        [DisplayOrder(150)]
        public string DeleteStage { get; private set; }

        [DisplayOrder(160)]
        public string IsTransacted { get; private set; }

        [DisplayOrder(170)]
        public string SyncworkflowlogOnFailure { get; private set; }

        [DisplayOrder(180)]
        public string AsyncAutoDelete { get; private set; }

        [DisplayOrder(190)]
        public string BusinessProcessType { get; private set; }
    }
}