using JosephM.Application.ViewModel.Dialog;
using JosephM.CustomisationImporter;
using JosephM.CustomisationImporter.Service;
using JosephM.Xrm.Vsix.Application.Extentions;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.App.Module.CustomisationImport
{
    public class VsixCustomisationImportDialog : CustomisationImportDialog
    {
        public VsixCustomisationImportDialog(XrmCustomisationImportService service, IDialogController dialogController, XrmPackageSettings xrmPackageSettings)
            : base(service, dialogController, true)
        {
            this.AddRedirectToPackageSettingsEntryWhenNotConnected(service.RecordService, xrmPackageSettings, processEnteredSettings: (s) =>
            {
                Request.AddToSolution = s.AddToSolution;
                Request.Solution = s.Solution;
            });
        }
    }
}
