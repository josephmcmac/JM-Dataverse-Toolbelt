using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.CustomisationImporter;
using JosephM.CustomisationImporter.Service;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.App.Module.CustomisationImport
{
    [RequiresConnection(nameof(ProcessEnteredSettings))]
    public class VsixCustomisationImportDialog : CustomisationImportDialog
    {
        public VsixCustomisationImportDialog(XrmCustomisationImportService service, IDialogController dialogController, XrmPackageSettings xrmPackageSettings)
            : base(service, dialogController)
        {
        }

        public void ProcessEnteredSettings(XrmPackageSettings packageSettings)
        {
            if(packageSettings != null)
            {
                Request.AddToSolution = packageSettings.AddToSolution;
                Request.Solution = packageSettings.Solution;
            }
        }
    }
}
