#region

using System.Collections.Generic;
using JosephM.Core.Service;
using JosephM.Xrm.ImportExporter.Solution.Service;
using JosephM.Core.Attributes;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    public class XrmSolutionImporterExporterResponse : ServiceResponseBase<XrmSolutionImporterExporterResponseItem>
    {
        [Hidden]
        public IEnumerable<ExportedSolution> ExportedSolutions { get; internal set; }
    }
}