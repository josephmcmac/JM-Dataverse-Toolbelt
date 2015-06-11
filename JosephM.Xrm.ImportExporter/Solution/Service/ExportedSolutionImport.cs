using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.FieldType;
using JosephM.Xrm.ImportExporter.Service;

namespace JosephM.Xrm.ImportExporter.Solution.Service
{
    public class ExportedSolutionImport : ISolutionImport
    {
        public ExportedSolution Export { get; set; }
        public SolutionMigration Migration { get; set; }

        public ExportedSolutionImport(ExportedSolution export, SolutionMigration migration)
        {
            Migration = migration;
            Export = export;
        }

        public FileReference SolutionFile
        {
            get { return Export.SolutionFile; }
        }

        public bool OverwriteCustomisations
        {
            get { return Migration.OverwriteCustomisations; }
        }
        public bool PublishWorkflows
        {
            get { return Migration.PublishWorkflows; }
        }
        public int? ImportOrder
        {
            get { return Migration.ImportOrder; }
        }
    }
}
