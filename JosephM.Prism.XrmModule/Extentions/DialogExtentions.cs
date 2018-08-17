using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Collections.Generic;

namespace JosephM.XrmModule.Extentions
{
    public static class DialogExtentions
    {
        public static void AddRedirectToConnectionEntryWhenNotConnected(this DialogViewModel dialog, XrmRecordService xrmRecordService)
        {
            if (string.IsNullOrWhiteSpace(xrmRecordService.XrmRecordConfiguration.OrganizationUniqueName))
            {
                //if there was no connection then lets redirect to the connection entry first
                var connectionEntryDialog = new SavedXrmConnectionEntryDialog(dialog, xrmRecordService);
                var subDialogList = new List<DialogViewModel>();
                subDialogList.Add(connectionEntryDialog);
                subDialogList.AddRange(dialog.SubDialogs);
                dialog.SubDialogs = subDialogList;
            }
        }
    }
}
