namespace JosephM.CustomisationExporter.Exporter
{
    public class WorkflowExport
    {
        public WorkflowExport(string state, string category, string name, string primaryEntity, string mode, string scope, string runAs, string onDemand, string subProcess, string triggerOnCreate, string onUpdateAttributes, string updateStage, string triggerOnDelete, string deleteStage, string isTransacted, string syncworkflowlogOnFailure, string asyncAutoDelete, string businessProcessType)
        {
            State = state;
            Category = category;
            Name = name;
            PrimaryEntity = primaryEntity;
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

        public string State { get; private set; }
        public string Category { get; private set; }
        public string Name { get; private set; }
        public string PrimaryEntity { get; private set; }
        public string Mode { get; private set; }
        public string Scope { get; private set; }
        public string RunAs { get; private set; }
        public string OnDemand { get; private set; }
        public string SubProcess { get; private set; }
        public string TriggerOnCreate { get; private set; }
        public string OnUpdateAttributes { get; private set; }
        public string UpdateStage { get; private set; }
        public string TriggerOnDelete { get; private set; }
        public string DeleteStage { get; private set; }
        public string IsTransacted { get; private set; }
        public string SyncworkflowlogOnFailure { get; private set; }
        public string AsyncAutoDelete { get; private set; }
        public string BusinessProcessType { get; private set; }
    }
}