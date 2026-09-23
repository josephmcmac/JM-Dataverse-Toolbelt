using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Charts.Clone
{
    public class CloneSavedChartsService :
        ServiceBase<CloneSavedChartsRequest, CloneSavedChartsResponse, CloneSavedChartsResponseItem>
    {
        public XrmRecordService RecordService { get; set; }
        public CloneSavedChartsService(XrmRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(CloneSavedChartsRequest request, CloneSavedChartsResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            controller.UpdateProgress(0, countToUpdate, "Cloning saved charts");
            var objectsToClone = request.GetObjectsToClone();

            var done = 0;
            var todo = objectsToClone.Count();
            var cloneService = RecordService.CloneForParellelProcessing() as XrmRecordService;
            cloneService.ImpersonatingUserId = new Guid(request.CloneToUser.Id);
            foreach (var objectToClone in objectsToClone)
            {
                controller.UpdateProgress(done, todo, $"Cloning saved charts {done}/{todo}");
                done++;
                try
                {
                    var record = new XrmRecord(Entities.userqueryvisualization);
                    record.SetField(Fields.userqueryvisualization_.charttype, OptionSets.UserChart.ChartType.ASPNETCharts, RecordService);
                    record.SetField(Fields.userqueryvisualization_.primaryentitytypecode, objectToClone.RecordType, RecordService);
                    record.SetField(Fields.userqueryvisualization_.presentationdescription, objectToClone.ChartPresentationXml, RecordService);
                    record.SetField(Fields.userqueryvisualization_.datadescription, objectToClone.ChartDataXml, RecordService);
                    record.SetField(Fields.userqueryvisualization_.name, $"Clone ({objectToClone.Owner}) {objectToClone.Name}".Left(RecordService.GetMaxLength(Fields.userform_.name, Entities.userquery)), RecordService);
                    record.SetField(Fields.userqueryvisualization_.description, objectToClone.Description, RecordService);
                    cloneService.Create(record);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new CloneSavedChartsResponseItem(objectToClone.Name, ex));
                    response.NumberOfErrors++;
                }
                response.TotalRecordsProcessed++;
            }

            response.Message = "Clone process completed";
        }
    }
}