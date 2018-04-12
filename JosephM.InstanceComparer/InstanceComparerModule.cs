using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.InstanceComparer
{
    [MyDescription("Compare The State Of Customisations And Data Between 2 CRM Instances. Note This Is Not A Complete Comparison")]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class InstanceComparerModule :
        ServiceRequestModule
            <InstanceComparerDialog, InstanceComparerService, InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>
    {
        public override string MainOperationName
        {
            get { return "Instance Compare"; }
        }
    }
}