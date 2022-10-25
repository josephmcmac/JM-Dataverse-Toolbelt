using System.Collections.Generic;

namespace JosephM.Deployment.SolutionsImport
{
    public class ImportSolutionsRequest
    {
        public IEnumerable<IImportSolutionsRequestItem> Items { get; set; }
    }
}
