using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System.Collections.Generic;
using JosephM.XRM.VSIX.Commands.PackageSettings;

namespace JosephM.XRM.VSIX.Wizards
{
    public class XrmSolutionWizard : MyWizardBase
    {
        public XrmRecordConfiguration XrmRecordConfiguration { get; set; }
        public XrmPackageSettings XrmPackageSettings { get; set; }

        public override void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            var xrmConfig = new XrmRecordConfiguration();
            #if DEBUG
                        xrmConfig.AuthenticationProviderType = XrmRecordAuthenticationProviderType.ActiveDirectory;
                        xrmConfig.DiscoveryServiceAddress = "http://qa2012/XRMServices/2011/Discovery.svc";
                        xrmConfig.Name = "TEST";
                        xrmConfig.OrganizationUniqueName = "TEST";
                        xrmConfig.Username = "joseph";
                        xrmConfig.Domain = "auqa2012";
            #endif
            var dialog = new ConnectionEntryDialog(DialogUtility.CreateDialogController(), xrmConfig,
                VisualStudioService, false);

            DialogUtility.LoadDialog(dialog, showCompletion: false, isModal: true);
            XrmRecordConfiguration = xrmConfig;

            XrmPackageSettings = new XrmPackageSettings();
            #if DEBUG
                XrmPackageSettings.SolutionDynamicsCrmPrefix = "template";
                XrmPackageSettings.SolutionObjectPrefix = "Template";
            #endif
            var settingsDialog = new XrmPackageSettingDialog(DialogUtility.CreateDialogController(), XrmPackageSettings, VisualStudioService, false, new XrmRecordService(XrmRecordConfiguration));
            DialogUtility.LoadDialog(settingsDialog, showCompletion: false, isModal: true);

            AddReplacements(replacementsDictionary, XrmPackageSettings);
        }

        public override void RunFinishedExtention()
        {
            VisualStudioService.AddSolutionItem("xrmpackage.xrmsettings", XrmPackageSettings);
            VsixUtility.AddXrmConnectionToSolution(XrmRecordConfiguration, VisualStudioService);


            VisualStudioService.CloseAllDocuments();
        }
    }
}
