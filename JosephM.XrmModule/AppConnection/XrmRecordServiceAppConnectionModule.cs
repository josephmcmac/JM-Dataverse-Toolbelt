using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JosephM.XrmModule.AppConnection
{
    /// <summary>
    /// Base module used to register the applicaiton to redirect to enter a dynamics connection when one is required by a dialog which is navigated to
    /// </summary>
    public abstract class XrmRecordServiceAppConnectionModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
            //okay lets register a navigation event
            //where if the dialog being navigated to has the RequiresConnectionSettings attribute
            //then we will inject a connection dialog
            Action<object> theEvent = (o) =>
            {
                var attr = o.GetType().GetCustomAttribute<RequiresConnection>();
                if (attr != null)
                {
                    var dialog = o as DialogViewModel;
                    var xrmRecordService = dialog.ApplicationController.ResolveType<XrmRecordService>();
                    if (string.IsNullOrWhiteSpace(xrmRecordService.XrmRecordConfiguration.OrganizationUniqueName))
                    {
                        //if there was no connection then lets redirect to the connection entry first
                        var connectionEntryDialog = CreateRedirectDialog(dialog, xrmRecordService);
                        if (attr.ProcessEnteredSettingsMethodName != null)
                        {
                            Action processEnteredSettings = () => dialog.InvokeMethod(attr.ProcessEnteredSettingsMethodName, dialog.ApplicationController);
                            connectionEntryDialog.AddOnCompletedEvent(processEnteredSettings);
                            
                        }
                        var subDialogList = new List<DialogViewModel>();
                        subDialogList.Add(connectionEntryDialog);
                        subDialogList.AddRange(dialog.SubDialogs);
                        dialog.SubDialogs = subDialogList;
                        ApplicationController.LogEvent("Injected Connection Entry To Dialog", new Dictionary<string, string>
                        {
                            { "Dialog Type", dialog.GetType().Name }
                        });
                    }
                }
            };
            this.AddNavigationEvent(theEvent);
        }

        protected abstract DialogViewModel CreateRedirectDialog(DialogViewModel dialog, XrmRecordService xrmRecordService);
    }
}