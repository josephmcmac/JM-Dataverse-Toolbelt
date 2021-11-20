using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.Wizards
{
    public class SolutionWizardPackageSettingsDialog : XrmPackageSettingsDialog
    {
        public SolutionWizardPackageSettingsDialog(IDialogController dialogController, XrmPackageSettings objectToEnter, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService, string saveButtonLabel)
            : base(dialogController, objectToEnter, visualStudioService, xrmRecordService, saveButtonLabel)
        {
        }
    }
}
