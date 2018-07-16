using JosephM.Core.Service;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    public class DeployIntoFieldResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }
        public bool Created { get; set; }
        public bool Updated { get; set; }
    }
}