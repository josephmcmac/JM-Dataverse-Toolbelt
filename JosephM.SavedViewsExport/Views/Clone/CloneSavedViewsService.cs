using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Views.Clone
{
    public class CloneSavedViewsService :
        ServiceBase<CloneSavedViewsRequest, CloneSavedViewsResponse, CloneSavedViewsResponseItem>
    {
        public XrmRecordService RecordService { get; set; }
        public CloneSavedViewsService(XrmRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(CloneSavedViewsRequest request, CloneSavedViewsResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            controller.UpdateProgress(0, countToUpdate, "Cloning saved views");
            var objectsToClone = request.GetObjectsToClone();

            var done = 0;
            var todo = objectsToClone.Count();
            var cloneService = RecordService.CloneForParellelProcessing() as XrmRecordService;
            cloneService.ImpersonatingUserId = new Guid(request.CloneToUser.Id);
            foreach (var objectToClone in objectsToClone)
            {
                controller.UpdateProgress(done, todo, $"Cloning saved views {done}/{todo}");
                done++;
                try
                {
                    var record = new XrmRecord(Entities.userquery);
                    record.SetField(Fields.userquery_.querytype, 0);
                    record.SetField(Fields.userquery_.returnedtypecode, objectToClone.RecordType);
                    record.SetField(Fields.userquery_.fetchxml, objectToClone.FetchXml);
                    record.SetField(Fields.userquery_.layoutxml, objectToClone.LayoutXml);
                    record.SetField(Fields.userquery_.name, $"Clone ({objectToClone.Owner}) {objectToClone.Name}".Left(RecordService.GetMaxLength(Fields.userquery_.name, Entities.userquery)));
                    record.SetField(Fields.userquery_.description, objectToClone.Description);
                    cloneService.Create(record);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new CloneSavedViewsResponseItem(objectToClone.Name, ex));
                    response.NumberOfErrors++;
                }
                response.TotalRecordsProcessed++;
            }

            response.Message = "Clone process completed";
        }
    }
}