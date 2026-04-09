using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Linq;

namespace JosephM.SavedViewsExport.CloneSavedViews
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
            controller.UpdateProgress(0, countToUpdate, "Cloning Views");
            var viewsToClone = request.GetViewsToClone();

            var done = 0;
            var todo = viewsToClone.Count();
            var cloneService = RecordService.CloneForParellelProcessing() as XrmRecordService;
            cloneService.ImpersonatingUserId = new Guid(request.CloneToUser.Id);
            foreach (var viewToClone in viewsToClone)
            {
                controller.UpdateProgress(done, todo, $"Cloning saved views {done}/{todo}");
                done++;
                try
                {
                    var record = new XrmRecord(Entities.userquery);
                    record.SetField(Fields.userquery_.querytype, 0);
                    record.SetField(Fields.userquery_.returnedtypecode, viewToClone.RecordType);
                    record.SetField(Fields.userquery_.fetchxml, viewToClone.FetchXml);
                    record.SetField(Fields.userquery_.layoutxml, viewToClone.LayoutXml);
                    record.SetField(Fields.userquery_.name, $"Clone ({viewToClone.Owner}) {viewToClone.Name}".Left(RecordService.GetMaxLength(Fields.userquery_.name, Entities.userquery)));
                    record.SetField(Fields.userquery_.description, viewToClone.Description);
                    cloneService.Create(record);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new CloneSavedViewsResponseItem(viewToClone.Name, ex));
                    response.NumberOfErrors++;
                }
                response.TotalRecordsProcessed++;
            }

            response.Message = "Clone Process Completed";
        }
    }
}