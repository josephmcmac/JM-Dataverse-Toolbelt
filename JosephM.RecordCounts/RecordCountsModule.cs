using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.RecordCounts
{
    [MyDescription("Generate Counts Of Records In The CRM Instance Either Globally Or Per User/Owner")]
    public class RecordCountsModule :
        ServiceRequestModule
            <RecordCountsDialog, RecordCountsService, RecordCountsRequest,
                RecordCountsResponse, RecordCountsResponseItem>
    {
        public override string MenuGroup => "Reports";
    }
}