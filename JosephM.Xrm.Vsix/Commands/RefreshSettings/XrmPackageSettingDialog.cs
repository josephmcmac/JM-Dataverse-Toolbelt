using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System;
using System.Linq;

namespace JosephM.XRM.VSIX.Commands.PackageSettings
{
    public class XrmPackageSettingDialog : VsixEntryDialog
    {
        public IVisualStudioService VisualStudioService { get; set; }
        public bool SaveSettings { get; set; }

        private XrmPackageSettings EnteredXrmPackageSettings
        {
            get
            {
                return EnteredObject as XrmPackageSettings;
            }
        }

        private XrmRecordService XrmRecordService { get; set; }

        public XrmPackageSettingDialog(IDialogController dialogController, XrmPackageSettings objectToEnter, IVisualStudioService visualStudioService, bool saveSettings, XrmRecordService xrmRecordService)
            : base(dialogController, objectToEnter, xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
            VisualStudioService = visualStudioService;
            SaveSettings = saveSettings;
        }

        protected override void LoadDialogExtention()
        {
            //if we do not yet have connections added to the package settings
            //then lets add the active connection to the settings as the active connection
            if (EnteredXrmPackageSettings.Connections == null
                || !EnteredXrmPackageSettings.Connections.Any())
            {
                try
                {
                    var savedConnection = SavedXrmRecordConfiguration.CreateNew(XrmRecordService.XrmRecordConfiguration);
                    savedConnection.Active = true;
                    EnteredXrmPackageSettings.Connections = new[] { savedConnection };
                }
                catch(Exception)
                {
                    //No matter if this happens user may just add it in anyway
                }
            }
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            if (SaveSettings)
            {

                VisualStudioService.AddSolutionItem("xrmpackage.xrmsettings", XrmPackageSettings);
                //set the active connection to the connection selected as active
                if (XrmPackageSettings.Connections != null)
                {
                    var activeConnections = XrmPackageSettings.Connections.Where(c => c.Active);
                    if(activeConnections.Any())
                    {
                        var activeConnection = activeConnections.First();
                        VsixUtility.AddXrmConnectionToSolution(activeConnection, VisualStudioService);
                    }
                }
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