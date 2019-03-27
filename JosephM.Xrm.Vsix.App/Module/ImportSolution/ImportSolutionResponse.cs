using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    public class ImportSolutionResponse : ServiceResponseBase<ImportSolutionResponseItem>
    {
        [Hidden]
        public SavedXrmRecordConfiguration Connection { get; internal set; }
    }
}