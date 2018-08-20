using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System.Collections.Generic;
using JosephM.Core.AppConfig;
using System;

namespace JosephM.Xrm.Vsix.Application.Extentions
{
    public static class DialogExtentions
    {
        public static void AddRedirectToPackageSettingsEntryWhenNotConnected(this DialogViewModel dialog, XrmRecordService xrmRecordService, XrmPackageSettings xrmPackageSettings, Action<XrmPackageSettings> processEnteredSettings = null)
        {
            if (string.IsNullOrWhiteSpace(xrmRecordService.XrmRecordConfiguration.OrganizationUniqueName))
            {
                //if there was no connection then lets redirect to the connection entry first
                dialog.Controller.ApplicationController.UserMessage("You are being redirected to enter package settings as there is no connection configured for the solution");

                var visualStudioService = dialog.ApplicationController.ResolveType<IVisualStudioService>();
                var connectionEntryDialog = new XrmPackageSettingsDialog(dialog, xrmPackageSettings, visualStudioService, xrmRecordService, processEnteredSettings);
                var subDialogList = new List<DialogViewModel>();
                subDialogList.Add(connectionEntryDialog);
                subDialogList.AddRange(dialog.SubDialogs);
                dialog.SubDialogs = subDialogList;
            }
        }
    }
}
