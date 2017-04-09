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
        public IVisualStudioService VisualStudioService { get; set; }
        public bool SaveSettings { get; set; }

        public XrmPackageSettingDialog(IDialogController dialogController, XrmPackageSettings objectToEnter, IVisualStudioService visualStudioService, bool saveSettings, XrmRecordService xrmRecordService)
            : base(dialogController, objectToEnter, xrmRecordService)
        {
            VisualStudioService = visualStudioService;
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
                VisualStudioService.AddSolutionItem("xrmpackage.xrmsettings", XrmPackageSettings);
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