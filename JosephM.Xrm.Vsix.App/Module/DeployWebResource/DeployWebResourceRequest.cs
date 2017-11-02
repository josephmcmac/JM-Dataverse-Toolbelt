using System.Collections.Generic;
using JosephM.Core.Service;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    public class DeployWebResourceRequest : ServiceRequestBase
    {
        public IEnumerable<string> Files { get; set; }
    }
}