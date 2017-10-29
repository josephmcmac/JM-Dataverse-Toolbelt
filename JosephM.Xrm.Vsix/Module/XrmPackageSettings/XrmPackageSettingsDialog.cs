using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Utilities;
using JosephM.XRM.VSIX;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    public class XrmPackageSettingsDialog : AppSettingsDialog<XrmPackageSettings, XrmPackageSettings>
    {
        public IVisualStudioService VisualStudioService { get; set; }
        public bool SaveSettings { get; set; }
        private XrmRecordService XrmRecordService { get; set; }

        public XrmPackageSettingsDialog(IDialogController dialogController, XrmPackageSettings objectToEnter, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService)
        : base(dialogController, xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
            VisualStudioService = visualStudioService;
            SaveSettings = true;

            if(string.IsNullOrWhiteSpace(XrmRecordService.XrmRecordConfiguration.OrganizationUniqueName))
            {
                //if there was no connection then lets redirect to the connection entry first
                ApplicationController.UserMessage("You are being redirected to the connection form as there is no connection for the solution");
                var newConnection = new XrmRecordConfiguration();
                Action refreshChildDialogConnection = () => { XrmRecordService.XrmRecordConfiguration = newConnection; };
                var connectionEntryDialog = new ConnectionEntryDialog(this, newConnection, visualStudioService, true, doPostEntry: refreshChildDialogConnection);
                var subDialogList = new List<DialogViewModel>();
                subDialogList.Add(connectionEntryDialog);
                XrmRecordService.XrmRecordConfiguration = newConnection;
                subDialogList.AddRange(SubDialogs);
                SubDialogs = subDialogList;
            }
        }

        protected override void LoadDialogExtention()
        {
            //if we do not yet have connections added to the package settings
            //then lets add the active connection to the settings as the active connection
            if (SettingsObject.Connections == null
                || !SettingsObject.Connections.Any())
            {
                try
                {
                    var savedConnection = SavedXrmRecordConfiguration.CreateNew(XrmRecordService.XrmRecordConfiguration);
                    savedConnection.Active = true;
                    SettingsObject.Connections = new[] { savedConnection };
                }
                catch (Exception)
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
                base.CompleteDialogExtention();
                //set the active connection to the connection selected as active
                if (SettingsObject.Connections != null)
                {
                    var activeConnections = SettingsObject.Connections.Where(c => c.Active);
                    if (activeConnections.Any())
                    {
                        var activeConnection = activeConnections.First();
                        VsixUtility.AddXrmConnectionToSolution(activeConnection, VisualStudioService);
                    }
                }
            }

            CompletionMessage = "Settings Updated";
        }
    }
}