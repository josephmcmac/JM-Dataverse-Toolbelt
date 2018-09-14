using JosephM.Core.Attributes;

namespace JosephM.Deployment.DataImport
{
    [DoNotAllowGridOpen]
    public class ImportedRecords
    {
        public string Type { get; set; }
        [GridWidth(125)]
        public int Created { get; set; }
        [GridWidth(125)]
        public int Updated { get; set; }
    }
}