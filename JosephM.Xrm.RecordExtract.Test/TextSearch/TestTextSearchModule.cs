using JosephM.Application.Desktop.Test;
using JosephM.Application.Modules;
using JosephM.Xrm.RecordExtract.Test.RecordExtract;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [DependantModule(typeof(TestingModule))]
    [DependantModule(typeof(TestRecordExtractModule))]
    public class TestTextSearchModule :
        TextSearchModuleBase
            <TestTextSearchDialog, TestTextSearchService>
    {
    }
}