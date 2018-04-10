using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Application.Prism.Test;
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