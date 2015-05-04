using JosephM.Prism.Infrastructure.Attributes;
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