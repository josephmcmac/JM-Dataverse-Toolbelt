using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.SavedXrmConnections;

namespace JosephM.InstanceComparer
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class InstanceComparerModule :
        ServiceRequestModule
            <InstanceComparerDialog, InstanceComparerService, InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>
    {
        protected override string MainOperationName
        {
            get { return "Compare CRM Instances"; }
        }
    }
}