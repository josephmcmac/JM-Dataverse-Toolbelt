using JosephM.Prism.Infrastructure.Attributes;
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