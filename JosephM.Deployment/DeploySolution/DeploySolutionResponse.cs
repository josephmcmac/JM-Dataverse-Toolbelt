using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Deployment.DeploySolution
{
    public class DeploySolutionResponse : ServiceResponseBase<DeploySolutionResponseItem>
    {
        [Hidden]
        public SavedXrmRecordConfiguration ConnectionDeployedInto { get; set; }
    }
}