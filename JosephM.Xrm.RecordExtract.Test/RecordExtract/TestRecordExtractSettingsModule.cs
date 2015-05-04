using JosephM.Prism.Infrastructure.Attributes;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    [DependantModule(typeof(TestingModule))]
    public class TestRecordExtractSettingsModule :
        SettingsModule<TestRecordExtractSettingsDialog, IRecordExtractSettings, RecordExtractSettings>
    {
    }
}