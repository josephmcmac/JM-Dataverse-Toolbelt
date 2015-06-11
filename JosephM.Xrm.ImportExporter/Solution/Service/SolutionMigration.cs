using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Prism.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.ImportExporter.Service
{
    public class SolutionMigration : SolutionExport
    {
        public SolutionMigration()
        {
            OverwriteCustomisations = true;
            PublishWorkflows = true;
        }

        [DisplayOrder(120)]
        public bool OverwriteCustomisations { get; set; }

        [DisplayOrder(130)]
        public bool PublishWorkflows { get; set; }

        [DisplayOrder(140)]
        public int? ImportOrder { get; set; }
    }
}
