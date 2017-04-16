using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    [DependantModule(typeof(TestingModule))]
    public class TestRecordExtractModule :
        ServiceRequestModule
            <TestRecordExtractDialog, TestRecordExtractService, RecordExtractRequest, RecordExtractResponse, RecordExtractResponseItem>
    {
    }
}