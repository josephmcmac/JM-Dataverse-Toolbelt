using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.PackageSettings
{
    public class XrmPackageSettingDialog : VsixEntryDialog
    {
        public Solution2 Solution { get; set; }
        public string Directory { get; set; }
        public bool SaveSettings { get; set; }

        public XrmPackageSettingDialog(IDialogController dialogController, XrmPackageSettings objectToEnter, Solution2 solution, string directory, bool saveSettings)
            : base(dialogController, objectToEnter)
        {
            Solution = solution;
            Directory = directory;
            SaveSettings = saveSettings;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            if (SaveSettings)
            {
                VsixUtility.AddSolutionItem(Solution, "xrmpackage.xrmsettings", XrmPackageSettings, Directory);
            }

            CompletionMessage = "Settings Updated";
        }

        private XrmPackageSettings XrmPackageSettings
        {
            get
            {
                return EnteredObject as XrmPackageSettings;
            }
        }
    }
}