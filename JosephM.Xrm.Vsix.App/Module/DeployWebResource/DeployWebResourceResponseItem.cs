using JosephM.Core.Service;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    public class DeployWebResourceResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }
        public bool Created { get; set; }
        public bool Updated { get; set; }
    }
}