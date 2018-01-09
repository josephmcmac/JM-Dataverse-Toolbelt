using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Xrm.RecordExtract.Test.RecordExtract;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [DependantModule(typeof(TestingModule))]
    [DependantModule(typeof(TestRecordExtractModule))]
    [DependantModule(typeof(TestTextSearchSettingsModule))]
    public class TestTextSearchModule :
        ServiceRequestModule
            <TestTextSearchDialog, TestTextSearchService, TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
    {
    }
}