using JosephM.Core.Service;

namespace JosephM.XRM.VSIX.Commands.DeployWebResource
{
    public class DeployWebResourcesResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }
        public bool Created { get; set; }
        public bool Updated { get; set; }
    }
}