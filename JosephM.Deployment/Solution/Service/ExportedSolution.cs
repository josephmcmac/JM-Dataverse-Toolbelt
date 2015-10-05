using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.FieldType;
using JosephM.Xrm.ImportExporter.Service;

namespace JosephM.Xrm.ImportExporter.Solution.Service
{
    public class ExportedSolution
    {
        public SolutionExport Export { get; set; }
        public FileReference SolutionFile { get; set; }

        public ExportedSolution(SolutionExport export, FileReference fileName)
        {
            SolutionFile = fileName;
            Export = export;
        }
    }
}
