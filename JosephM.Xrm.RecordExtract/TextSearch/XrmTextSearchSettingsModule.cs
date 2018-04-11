using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class XrmTextSearchSettingsModule :
        SettingsModule<XrmTextSearchSettingsDialog, ITextSearchSettings, TextSearchSettings>
    {

    }
}