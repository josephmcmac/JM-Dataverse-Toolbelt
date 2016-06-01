using System;
using EnvDTE;
using EnvDTE80;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.VisualStudio.TemplateWizard;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using JosephM.XRM.VSIX.Commands.PackageSettings;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using Microsoft.VisualStudio.OLE.Interop;

namespace JosephM.XRM.VSIX.Wizards
{
    public class XrmSolutionWizard : MyWizardBase
    {
        public XrmPackageSettings XrmPackageSettings { get; set; }

        public override void RunStartedExtention(Dictionary<string, string> replacementsDictionary)
        {
            XrmPackageSettings = new XrmPackageSettings();
            #if DEBUG
                XrmPackageSettings.SolutionDynamicsCrmPrefix = "template";
                XrmPackageSettings.SolutionObjectPrefix = "Template";
            #endif
            var settingsDialog = new XrmPackageSettingDialog(DialogUtility.CreateDialogController(), XrmPackageSettings, VisualStudioService, false);
            DialogUtility.LoadDialog(settingsDialog, showCompletion: false);

            AddReplacements(replacementsDictionary, XrmPackageSettings);
        }

        public override void RunFinishedExtention()
        {
            VisualStudioService.AddSolutionItem("xrmpackage.xrmsettings", XrmPackageSettings);

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
                VisualStudioService);

            DialogUtility.LoadDialog(dialog, false);

            VisualStudioService.CloseAllDocuments();
        }
    }
}
