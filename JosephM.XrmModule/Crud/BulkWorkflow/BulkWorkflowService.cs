using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace JosephM.XrmModule.Crud.BulkWorkflow
{
    public class BulkWorkflowService :
        ServiceBase<BulkWorkflowRequest, BulkWorkflowResponse, BulkWorkflowResponseItem>
    {
        public XrmRecordService RecordService { get; set; }
        public BulkWorkflowService(XrmRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(BulkWorkflowRequest request, BulkWorkflowResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Workflows");
            var estimator = new TaskEstimator(countToUpdate);
            var recordsRemaining = request.GetRecordsToUpdate().ToList();
            while(recordsRemaining.Any())
            {
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Updates"));

                var thisSetOfRecords = recordsRemaining
                    .Take(request.ExecuteMultipleSetSize ?? 50)
                    .ToList();

                recordsRemaining.RemoveRange(0, thisSetOfRecords.Count);

                var errorsThisIteration = 0;

                //old versions dont have execute multiple so if 1 then do each request
                if (thisSetOfRecords.Count() == 1)
                {
                    var record = thisSetOfRecords.First();
                    try
                    {
                        RecordService.XrmService.Execute(new ExecuteWorkflowRequest
                        {
                            EntityId = new Guid(record.Id),
                            WorkflowId = new Guid(request.Workflow.Id)
                        });
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new BulkWorkflowResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                        errorsThisIteration++;
                    }
                }
                else
                {
                    var requests = thisSetOfRecords.Select(r => new ExecuteWorkflowRequest
                    {
                        EntityId = new Guid(r.Id),
                        WorkflowId = new Guid(request.Workflow.Id)
                    });
                    var multipleResponse = RecordService.XrmService.ExecuteMultiple(requests);
                    var key = 0;
                    foreach (var item in multipleResponse)
                    {
                        var originalRecord = thisSetOfRecords[key];
                        if (item.Fault != null)
                        {
                            response.AddResponseItem(new BulkWorkflowResponseItem(originalRecord.Id, originalRecord.GetStringField(RecordService.GetPrimaryField(originalRecord.Type)), new FaultException<OrganizationServiceFault>(item.Fault, item.Fault.Message)));
                            errorsThisIteration++;
                        }
                        key++;
                    }
                }

                countUpdated += thisSetOfRecords.Count();
                response.NumberOfErrors += errorsThisIteration;
                response.TotalRecordsProcessed = countUpdated;

                Thread.Sleep(request.WaitPerMessage * 1000);
            }
            controller.UpdateProgress(1, 1, "All Workflows Requests Have Completed");
            response.Message = "Workflows Requests Completed";
        }
    }
}