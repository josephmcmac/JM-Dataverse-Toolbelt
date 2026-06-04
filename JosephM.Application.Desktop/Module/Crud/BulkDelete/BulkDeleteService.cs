using JosephM.Application.Desktop.Module.Crud.BulkUpdate;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkDelete
{
    public class BulkDeleteService :
        ServiceBase<BulkDeleteRequest, BulkDeleteResponse, BulkDeleteResponseItem>
    {
        public IRecordService RecordService { get; set; }
        public BulkDeleteService(IRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(BulkDeleteRequest request, BulkDeleteResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Deletions");
            var estimator = new TaskEstimator(countToUpdate);

            var recordsRemaining = request.GetRecordsToDelete().ToList();
            if (request.ParallelUpdateProcesses == 1)
            {
                BulkDeleteNestedProcess(request, response, controller, countToUpdate, ref countUpdated, estimator, recordsRemaining, RecordService);
            }
            else
            {
                ParallelTaskHelper.RunParallelTasks(() =>
                {
                    var parallelProcessService = RecordService.CloneForParellelProcessing();
                    BulkDeleteNestedProcess(request, response, controller, countToUpdate, ref countUpdated, estimator, recordsRemaining, parallelProcessService);
                }, request.ParallelUpdateProcesses ?? 0);
            }


            

            controller.UpdateProgress(1, 1, "Deletions Completed");
            response.Message = "Deletions Completed";
        }

        private void BulkDeleteNestedProcess(BulkDeleteRequest request, BulkDeleteResponse response, ServiceRequestController controller, int countToUpdate, ref int countUpdated, TaskEstimator estimator, List<IRecord> recordsRemaining, IRecordService parallelProcessService)
        {
            while (recordsRemaining.Any())
            {
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Deletions"));

                var thisSetOfRecords = GetNextSetOfRecords(request, recordsRemaining);

                var thisSetOfRecordsNew = thisSetOfRecords
                    .Select(r =>
                    {
                        var newRecord = RecordService.NewRecord(request.RecordType.Key);
                        newRecord.Id = r.Id;
                        return newRecord;
                    })
                    .ToArray();

                var errorsThisIteration = 0;

                //old versions dont have execute multiple so if 1 then do each request
                if (thisSetOfRecordsNew.Count() == 1)
                {
                    var record = thisSetOfRecordsNew.First();
                    try
                    {
                        parallelProcessService.Delete(record, bypassWorkflowsAndPlugins: request.BypassFlowsPluginsAndWorkflows);
                    }
                    catch (Exception ex)
                    {
                        var primaryField = RecordService.GetPrimaryField(record.Type);
                        response.AddResponseItem(new BulkDeleteResponseItem(record.Id, primaryField == null ? record.Id : record.GetStringField(primaryField), ex));
                        errorsThisIteration++;
                    }
                }
                else
                {
                    var multipleResponse = parallelProcessService.DeleteMultiple(thisSetOfRecordsNew, bypassWorkflowsAndPlugins: request.BypassFlowsPluginsAndWorkflows);
                    foreach (var item in multipleResponse)
                    {
                        var originalRecord = thisSetOfRecords[item.Key];
                        response.AddResponseItem(new BulkDeleteResponseItem(originalRecord.Id, originalRecord.GetStringField(RecordService.GetPrimaryField(originalRecord.Type)), item.Value));
                    }
                    errorsThisIteration += multipleResponse.Count;
                }

                countUpdated += thisSetOfRecords.Count();
                response.NumberOfErrors += errorsThisIteration;
                response.TotalRecordsProcessed = countUpdated;
            }
        }

        private static readonly object _getNextSetOfRecordsLockObject = new object();
        private static List<IRecord> GetNextSetOfRecords(BulkDeleteRequest request, List<IRecord> recordsRemaining)
        {
            lock (_getNextSetOfRecordsLockObject)
            {
                var thisSetOfRecords = recordsRemaining
                    .Take(request.ExecuteMultipleSetSize ?? 50)
                    .ToList();
                recordsRemaining.RemoveRange(0, thisSetOfRecords.Count);
                return thisSetOfRecords;
            }
        }
    }
}