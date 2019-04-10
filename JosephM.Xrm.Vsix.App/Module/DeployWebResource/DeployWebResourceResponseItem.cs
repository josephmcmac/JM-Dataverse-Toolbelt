using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    [DoNotAllowGridOpen]
    public class DeployWebResourceResponseItem : ServiceResponseItem
    {
        [DisplayOrder(30)]
        [GridWidth(400)]
        public string Name { get; set; }

        [GridWidth(75)]
        [DisplayOrder(10)]
        public bool Created { get; set; }

        [GridWidth(75)]
        [DisplayOrder(20)]
        public bool Updated { get; set; }
    }
}