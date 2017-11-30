using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    public class ImportSolutionDialog
        : ServiceRequestDialog<ImportSolutionService, ImportSolutionRequest, ImportSolutionResponse, ImportSolutionResponseItem>
    {
        public ImportSolutionDialog(ImportSolutionService service, IDialogController dialogController)
            : base(service, dialogController)
        {

        }
    }
}
