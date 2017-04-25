using System.Collections.Generic;
using JosephM.Core.Service;

namespace JosephM.XRM.VSIX.Commands.DeployWebResource
{
    public class DeployWebResourcesRequest : ServiceRequestBase
    {
        public IEnumerable<string> Files { get; set; }
    }
}