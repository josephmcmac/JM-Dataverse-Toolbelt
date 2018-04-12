using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.Desktop.Test;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [DependantModule(typeof (TestingModule))]
    public class TestTextSearchSettingsModule :
        SettingsModule<TestTextSearchSettingsDialog, ITextSearchSettings, TextSearchSettings>
    {
    }
}