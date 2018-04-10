using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.Settings;
using JosephM.Prism.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class XrmTextSearchSettingsModule :
        SettingsModule<XrmTextSearchSettingsDialog, ITextSearchSettings, TextSearchSettings>
    {

    }
}