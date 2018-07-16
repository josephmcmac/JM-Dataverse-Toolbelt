using System.Collections.Generic;
using JosephM.Core.Service;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    public class DeployIntoFieldRequest : ServiceRequestBase
    {
        public IEnumerable<string> Files { get; set; }
    }
}