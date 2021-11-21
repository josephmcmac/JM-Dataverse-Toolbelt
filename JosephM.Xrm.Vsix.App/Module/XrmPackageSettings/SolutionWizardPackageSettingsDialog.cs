using JosephM.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using JosephM.XrmModule.Crud;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    public class SolutionWizardPackageSettingsDialog : XrmPackageSettingsDialog
    {
        public SolutionWizardPackageSettingsDialog(IDialogController dialogController, XrmPackageSettings objectToEnter, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService, string saveButtonLabel)
            : base(dialogController, objectToEnter, visualStudioService, xrmRecordService, saveButtonLabel)
        {
        }

        public static void Run(XrmPackageSettings packageSettings, VsixApplicationController applicationController, string solutionName)
        {
            //ensure the package settings resolves when the app settings dialog runs
            var resolvePackageSettings = applicationController.ResolveType(typeof(XrmPackageSettings));
            if (resolvePackageSettings == null)
                applicationController.RegisterInstance(typeof(XrmPackageSettings), new XrmPackageSettings());

            if (solutionName != null && string.IsNullOrWhiteSpace(packageSettings.SolutionObjectPrefix))
            {
                packageSettings.SolutionObjectPrefix = solutionName.Split('.').First();
            }

            var serviceFactory = applicationController.ResolveType<IOrganizationConnectionFactory>();
            var recordService = new XrmRecordService(new XrmRecordConfiguration(), serviceFactory, formService: new XrmFormService());
            var settingsDialog = new SolutionWizardPackageSettingsDialog(new DialogController(applicationController), packageSettings, null, recordService, saveButtonLabel: "Next");
            settingsDialog.SaveSettings = false;
            var uriQuery = new UriQuery();
            uriQuery.Add("Modal", true.ToString());
            applicationController.NavigateTo(settingsDialog, uriQuery, showCompletionScreen: false, isModal: true);
        }
    }
}
