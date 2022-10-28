using JosephM.Core.Attributes;

namespace JosephM.Deployment.DeployPackage
{
    public class DeployPackageRecordTypeImport
    {
        [GridWidth(300)]
        [GridField]
        [GridReadOnly]
        [DisplayOrder(10)]
        public string RecordType { get; set; }

        [GridWidth(75)]
        [GridField]
        [GridReadOnly]
        [DisplayOrder(20)]
        public int RecordCount { get; set; }
    }
}