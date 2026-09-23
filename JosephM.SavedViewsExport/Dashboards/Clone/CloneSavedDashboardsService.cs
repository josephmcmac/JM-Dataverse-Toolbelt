using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Dashboards.Clone
{
    public class CloneSavedDashboardsService :
        ServiceBase<CloneSavedDashboardsRequest, CloneSavedDashboardsResponse, CloneSavedDashboardsResponseItem>
    {
        public XrmRecordService RecordService { get; set; }
        public CloneSavedDashboardsService(XrmRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(CloneSavedDashboardsRequest request, CloneSavedDashboardsResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            controller.UpdateProgress(0, countToUpdate, "Cloning Dashboards");
            var objectsToClone = request.GetObjectsToClone();

            var done = 0;
            var todo = objectsToClone.Count();
            var cloneService = RecordService.CloneForParellelProcessing() as XrmRecordService;
            cloneService.ImpersonatingUserId = new Guid(request.CloneToUser.Id);
            foreach (var objectToClone in objectsToClone)
            {
                controller.UpdateProgress(done, todo, $"Cloning saved Dashboards {done}/{todo}");
                done++;
                try
                {
                    var record = new XrmRecord(Entities.userform);
                    record.SetField(Fields.userform_.type, OptionSets.UserDashboard.FormType.Dashboard, RecordService);
                    record.SetField(Fields.userform_.formxml, objectToClone.FormXml, RecordService);
                    record.SetField(Fields.userform_.name, $"Clone ({objectToClone.Owner}) {objectToClone.Name}".Left(RecordService.GetMaxLength(Fields.userform_.name, Entities.userquery)), RecordService);
                    record.SetField(Fields.userform_.description, objectToClone.Description, RecordService);
                    cloneService.Create(record);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new CloneSavedDashboardsResponseItem(objectToClone.Name, ex));
                    response.NumberOfErrors++;
                }
                response.TotalRecordsProcessed++;
            }

            response.Message = "Clone Process Completed";
        }
    }
}