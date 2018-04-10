using JosephM.Core.Attributes;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.RecordCounts.Exporter
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