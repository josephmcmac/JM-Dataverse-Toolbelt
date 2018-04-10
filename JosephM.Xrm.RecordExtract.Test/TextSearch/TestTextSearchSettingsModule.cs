using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.Settings;
using JosephM.Application.Prism.Test;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [DependantModule(typeof (TestingModule))]
    public class TestTextSearchSettingsModule :
        SettingsModule<TestTextSearchSettingsDialog, ITextSearchSettings, TextSearchSettings>
    {
    }
}