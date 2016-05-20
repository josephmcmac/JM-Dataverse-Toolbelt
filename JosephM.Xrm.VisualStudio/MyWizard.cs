using EnvDTE;
using EnvDTE80;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Wpf.Application;
using Microsoft.VisualStudio.TemplateWizard;
using System.Collections.Generic;
using System.IO;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Dialogs;
using Window = System.Windows.Window;

namespace JosephM.XRM.VSIX
{
    public class MyWizard : IWizard
    {
        private DTE2 DTE { get; set; }
        private string Directory { get; set; }

        private string SolutionName { get; set; }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            DTE = automationObject as DTE2;
            Directory = replacementsDictionary.ContainsKey("$solutiondirectory$")
                ? replacementsDictionary["$solutiondirectory$"]
                : null;

            SolutionName = replacementsDictionary.ContainsKey("$specifiedsolutionname$")
                ? replacementsDictionary["$specifiedsolutionname$"]
                : replacementsDictionary["safeprojectname"];


            replacementsDictionary.Add("$myprefix$", SolutionName);
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {

        }

        public void RunFinished()
        {
            var solution = DTE == null ? null : DTE.Solution as Solution2;
            if (solution != null)
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
                var dialog = new ConnectionEntryDialog(DialogUtility.CreateDialogController(), xrmConfig, solution, Directory);

                DialogUtility.LoadDialog(dialog);
            }
        }
    }
}
