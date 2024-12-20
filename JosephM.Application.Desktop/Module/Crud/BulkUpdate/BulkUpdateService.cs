using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkUpdate
{
    public class BulkUpdateService :
        ServiceBase<BulkUpdateRequest, BulkUpdateResponse, BulkUpdateResponseItem>
    {
        public IRecordService RecordService { get; set; }
        public BulkUpdateService(IRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(BulkUpdateRequest request, BulkUpdateResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Updates");
            var estimator = new TaskEstimator(countToUpdate);
            var recordsRemaining = request.GetRecordsToUpdate().ToList();

            if(request.ParallelUpdateProcesses == 1)
            {
                BulkUpdatedNestedProcess(request, response, controller, countToUpdate, ref countUpdated, estimator, recordsRemaining, RecordService);
            }
            else
            {
                ParallelTaskHelper.RunParallelTasks(() =>
                {
                    var parallelProcessService = RecordService.CloneForParellelProcessing();
                    BulkUpdatedNestedProcess(request, response, controller, countToUpdate, ref countUpdated, estimator, recordsRemaining, parallelProcessService);
                }, request.ParallelUpdateProcesses ?? 0);
            }
            response.Message = "Updates Completed";
        }

        private static void BulkUpdatedNestedProcess(BulkUpdateRequest request, BulkUpdateResponse response, ServiceRequestController controller, int countToUpdate, ref int countUpdated, TaskEstimator estimator, List<IRecord> recordsRemaining, IRecordService recordService)
        {
            while (recordsRemaining.Any())
            {
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Updates"));
                var thisSetOfRecords = GetNextSetOfRecords(request, recordsRemaining);
                if (thisSetOfRecords.Any())
                {
                    var errorsThisIteration = 0;
                    try
                    {
                        var thisSetOfRecordsNew = thisSetOfRecords
                            .Select(r =>
                            {
                                var newRecord = recordService.NewRecord(request.RecordType.Key);
                                newRecord.Id = r.Id;
                                if (request.ClearValue)
                                    newRecord.SetField(request.FieldToSet.Key, null, recordService);
                                else
                                    newRecord.SetField(request.FieldToSet.Key, request.ValueToSet, recordService);
                                if (request.AddUpdateField2)
                                {
                                    if (request.ClearValue2)
                                        newRecord.SetField(request.FieldToSet2.Key, null, recordService);
                                    else
                                        newRecord.SetField(request.FieldToSet2.Key, request.ValueToSet2, recordService);
                                }
                                if (request.AddUpdateField3)
                                {
                                    if (request.ClearValue3)
                                        newRecord.SetField(request.FieldToSet3.Key, null, recordService);
                                    else
                                        newRecord.SetField(request.FieldToSet3.Key, request.ValueToSet3, recordService);
                                }
                                if (request.AddUpdateField4)
                                {
                                    if (request.ClearValue4)
                                        newRecord.SetField(request.FieldToSet4.Key, null, recordService);
                                    else
                                        newRecord.SetField(request.FieldToSet4.Key, request.ValueToSet4, recordService);
                                }
                                if (request.AddUpdateField5)
                                {
                                    if (request.ClearValue5)
                                        newRecord.SetField(request.FieldToSet5.Key, null, recordService);
                                    else
                                        newRecord.SetField(request.FieldToSet5.Key, request.ValueToSet5, recordService);
                                }
                                return newRecord;
                            })
                            .ToArray();

                        //old versions dont have execute multiple so if 1 then do each request
                        if (thisSetOfRecordsNew.Count() == 1)
                        {
                            var record = thisSetOfRecordsNew[0];
                            try
                            {
                                recordService.Update(record, bypassWorkflowsAndPlugins: request.BypassFlowsPluginsAndWorkflows);
                            }
                            catch (Exception ex)
                            {
                                var primaryField = recordService.GetPrimaryField(record.Type);
                                response.AddResponseItem(new BulkUpdateResponseItem(record.Id, primaryField == null ? record.Id : record.GetStringField(primaryField), ex));
                                errorsThisIteration++;
                            }
                        }
                        else
                        {
                            var multipleResponse = recordService.UpdateMultiple(thisSetOfRecordsNew, bypassWorkflowsAndPlugins: request.BypassFlowsPluginsAndWorkflows);
                            foreach (var item in multipleResponse)
                            {
                                var originalRecord = thisSetOfRecords[item.Key];
                                response.AddResponseItem(new BulkUpdateResponseItem(originalRecord.Id, originalRecord.GetStringField(recordService.GetPrimaryField(originalRecord.Type)), item.Value));
                            }
                            errorsThisIteration += multipleResponse.Count;
                        }
                    }
                    catch (Exception ex)
                    {
                        foreach (var thisSetRecord in thisSetOfRecords)
                        {
                            errorsThisIteration++;
                            response.AddResponseItem(new BulkUpdateResponseItem(thisSetRecord.Id, thisSetRecord.GetStringField(recordService.GetPrimaryField(thisSetRecord.Type)), ex));
                        }
                    }
                    countUpdated += thisSetOfRecords.Count;
                    response.NumberOfErrors += errorsThisIteration;
                    response.TotalRecordsProcessed = countUpdated;
                }
            }
        }

        private static readonly object _getNextSetOfRecordsLockObject = new object();
        private static List<IRecord> GetNextSetOfRecords(BulkUpdateRequest request, List<IRecord> recordsRemaining)
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