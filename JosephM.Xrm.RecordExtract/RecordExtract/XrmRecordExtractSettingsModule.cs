using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.Xrm;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    [DependantModule(typeof(XrmModuleModule))]
    public class XrmRecordExtractSettingsModule :
        SettingsModule<XrmRecordExtractSettingsDialog, IRecordExtractSettings, RecordExtractSettings>
    {
    }
}