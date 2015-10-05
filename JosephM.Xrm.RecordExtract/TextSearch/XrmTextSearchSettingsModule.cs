using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DependantModule(typeof(XrmModuleModule))]
    public class XrmTextSearchSettingsModule :
        SettingsModule<XrmTextSearchSettingsDialog, ITextSearchSettings, TextSearchSettings>
    {

    }
}