using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.XrmModule.AppConnection
{
    /// <summary>
    /// Registers the application to redirect to enter a saved connection when required by a dialog which is navigated to
    /// </summary>
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class SavedConnectionAppConnectionModule : XrmRecordServiceAppConnectionModule
    {
        protected override DialogViewModel CreateRedirectDialog(DialogViewModel dialog, XrmRecordService xrmRecordService)
        {
            return new AppXrmConnectionEntryDialog(dialog, xrmRecordService);
        }
    }
}