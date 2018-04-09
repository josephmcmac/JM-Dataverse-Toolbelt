using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class XrmTextSearchSettingsModule :
        SettingsModule<XrmTextSearchSettingsDialog, ITextSearchSettings, TextSearchSettings>
    {

    }
}