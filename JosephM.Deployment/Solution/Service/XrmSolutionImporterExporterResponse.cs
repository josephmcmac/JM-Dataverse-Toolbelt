#region

using System.Collections.Generic;
using JosephM.Core.Service;
using JosephM.Xrm.ImportExporter.Solution.Service;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    public class XrmSolutionImporterExporterResponse : ServiceResponseBase<XrmSolutionImporterExporterResponseItem>
    {
        public IEnumerable<ExportedSolution> ExportedSolutions { get; internal set; }
    }
}