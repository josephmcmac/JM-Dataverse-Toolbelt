using JosephM.Prism.Infrastructure.Module;

namespace JosephM.RecordCounts.Exporter
{
    public class RecordCountsModule :
        ServiceRequestModule
            <RecordCountsDialog, RecordCountsService, RecordCountsRequest,
                RecordCountsResponse, RecordCountsResponseItem>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Record Counts", "RecordCounts");
        }
    }
}