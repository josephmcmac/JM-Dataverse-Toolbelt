using JosephM.Core.Log;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Xml;

namespace JosephM.Deployment.SolutionImport
{
    public class SolutionImportService
    {
        public SolutionImportService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        private object _lockObject = new object();

        public XrmRecordService XrmRecordService { get; }

        public ImportSolutionsResponse ImportSolutions(Dictionary<string, byte[]> solutionFiles, LogController controller)
        {
            var response = new ImportSolutionsResponse();

            var xrmService = XrmRecordService.XrmService;
            bool asynch = xrmService.SupportsExecuteAsynch;

            controller.LogLiteral($"Loading Active {xrmService.GetEntityCollectionName(Entities.duplicaterule)}");
            var duplicateRules = xrmService.RetrieveAllAndConditions(Entities.duplicaterule, new[] { new ConditionExpression(Fields.duplicaterule_.statecode, ConditionOperator.Equal, OptionSets.DuplicateDetectionRule.Status.Active) }, new string[0]);

            foreach (var solutionFile in solutionFiles)
            {
                if(!response.Success)
                {
                    break;
                }
                try
                {
                    controller.LogLiteral(
                        $"Import Request for {solutionFile.Key} Submitted to {XrmRecordService.XrmRecordConfiguration?.ToString()}");
                    var importId = Guid.NewGuid();
                    var req = new ImportSolutionRequest();
                    req.ImportJobId = importId;
                    req.CustomizationFile = solutionFile.Value;
                    req.PublishWorkflows = true;
                    req.OverwriteUnmanagedCustomizations = true;

                    var finished = new Processor();

                    if (!asynch)
                    {
                        try
                        {
                            var extraService = new XrmService(xrmService.XrmConfiguration, new LogController(), xrmService.ServiceFactory);
                            var monitoreProgressThread = new Thread(() => DoProgress(importId, controller, finished, extraService, null, solutionFile.Key, response));
                            monitoreProgressThread.IsBackground = true;
                            monitoreProgressThread.Start();
                            xrmService.Execute(req);
                            ProcessCompletedSolutionImportJob(response, xrmService, importId);
                        }
                        finally
                        {
                            lock (_lockObject)
                                finished.Completed = true;
                        }
                    }
                    else
                    {
                        var asynchRequest = new ExecuteAsyncRequest
                        {
                            Request = req
                        };
                        var asynchResponse = (ExecuteAsyncResponse) xrmService.Execute(asynchRequest);
                        DoProgress(importId, controller, finished, xrmService, asynchResponse.AsyncJobId, solutionFile.Key, response);
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (ex.Detail != null && ex.Detail.InnerFault != null && ex.Detail.InnerFault.InnerFault != null)
                    {
                        throw new Exception($"Error Importing {solutionFile}: {ex.Message} - {ex.Detail.InnerFault.InnerFault.Message}", ex);
                    }
                    else
                    {
                        throw new Exception($"Error Importing {solutionFile}: {ex.Message}", ex);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error Importing {solutionFile}: {ex.Message}", ex);
                }
            }
            controller.TurnOffLevel2();

            if (response.Success)
            {
                controller.LogLiteral("Publishing Customisations");
                xrmService.Publish();
            }

            controller.LogLiteral($"Checking Deactivated {xrmService.GetEntityCollectionName(Entities.duplicaterule)}");
            duplicateRules = xrmService.Retrieve(Entities.duplicaterule, duplicateRules.Select(e => e.Id), new[] { Fields.duplicaterule_.statecode, Fields.duplicaterule_.name });
            foreach(var rule in duplicateRules)
            {
                if(rule.GetOptionSetValue(Fields.duplicaterule_.statecode) == OptionSets.DuplicateDetectionRule.Status.Inactive)
                {
                    controller.LogLiteral($"Republishing {xrmService.GetEntityLabel(Entities.duplicaterule)} '{rule.GetStringField(Fields.duplicaterule_.name)}'");
                    xrmService.Execute(new PublishDuplicateRuleRequest()
                    {
                        DuplicateRuleId = rule.Id
                    });
                }
            }
            return response;
        }

        private void ProcessCompletedSolutionImportJob(ImportSolutionsResponse response, XrmService xrmService, Guid importId)
        {
            var job = xrmService.GetFirst(Entities.importjob, Fields.importjob_.importjobid, importId);
            if (job != null)
            {
                var ignoreTheseErrorCodes = new[]
                {
                    "0x80045042",//reactivated workflows
                    "0x8004F039",//reactivated plugins
                    "0x80045043"//deactivated duplicate detection rules - these get activated in next set of code
                };
                var dataString = job.GetStringField(Fields.importjob_.data);
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(dataString);
                var resultNodes = xmlDocument.GetElementsByTagName("result");
                foreach (XmlNode node in resultNodes)
                {
                    var importResult = new SolutionImportResult(node, XrmRecordService);
                    if (!importResult.IsSuccess && (importResult.ErrorCode == null || !ignoreTheseErrorCodes.Contains(importResult.ErrorCode)))
                        response.AddResult(importResult);
                }
            }
        }

        public class Processor
        {
            public bool Completed { get; set; }
        }

        public void DoProgress(Guid importId, LogController controller, Processor finished, XrmService xrmService, Guid? asynchJobId, string solutionFile, ImportSolutionsResponse response)
        {
            var systemJobFailure = false;
            while (true)
            {
                Thread.Sleep(5000);
                lock (_lockObject)
                {
                    if (finished.Completed)
                        return;
                }
                try
                {
                    var job = xrmService.GetFirst(Entities.importjob, Fields.importjob_.importjobid, importId);
                    if (job != null)
                    {
                        if(job.GetDateTimeField(Fields.importjob_.completedon).HasValue)
                        {
                            break;
                        }
                        var progress = job.GetDoubleValue(Fields.importjob_.progress);
                        lock (_lockObject)
                        {
                            if (finished.Completed)
                            {
                                break;
                            }
                            controller.LogLiteral($"Importing {solutionFile} to {XrmRecordService.XrmRecordConfiguration?.ToString()}");
                            controller.GetLevel2Controller().UpdateProgress(Convert.ToInt32(progress / 1), 100, "Import Progress");
                        }
                    }
                    else if(asynchJobId.HasValue)
                    {
                        var systemJobQuery = new QueryExpression(Entities.asyncoperation);
                        systemJobQuery.NoLock = true;
                        systemJobQuery.Criteria.AddCondition(new ConditionExpression(Fields.asyncoperation_.asyncoperationid, ConditionOperator.Equal, asynchJobId.Value));
                        systemJobQuery.ColumnSet = new ColumnSet(Fields.asyncoperation_.statuscode, Fields.asyncoperation_.friendlymessage);
                        var systemJob = xrmService.RetrieveFirst(systemJobQuery);
                        if(systemJob != null)
                        {
                            if(systemJob.GetOptionSetValue(Fields.asyncoperation_.statuscode) == OptionSets.SystemJob.StatusReason.Failed)
                            {
                                systemJobFailure = true;
                                throw new Exception($"{XrmRecordService.GetDisplayName(Entities.asyncoperation)} {XrmRecordService.GetPicklistLabel(Fields.asyncoperation_.statuscode, Entities.asyncoperation, OptionSets.SystemJob.StatusReason.Failed.ToString())} - {systemJob.GetStringField(Fields.asyncoperation_.friendlymessage)}");
                            }
                            else
                            {
                                controller.UpdateProgress(0, 100, $"{XrmRecordService.GetDisplayName(Entities.asyncoperation)} for import of {solutionFile} is {XrmRecordService.GetPicklistLabel(Fields.asyncoperation_.statuscode, Entities.asyncoperation, systemJob.GetOptionSetValue(Fields.asyncoperation_.statuscode).ToString())}");
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    if(systemJobFailure)
                    {
                        throw ex;
                    }
                    controller.LogLiteral("Unexpected Error Skipped. Still Waiting For Import Job To Complete: " + ex.Message);
                }
            }

            if(asynchJobId.HasValue)
            {
                var completedJob = xrmService.GetFirst(Entities.importjob, Fields.importjob_.importjobid, importId);
                var xml = completedJob.GetStringField(Fields.importjob_.data);
                if(xml != null)
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xml);
                    var percentCompleted = Double.Parse(xmlDocument.GetElementsByTagName("importexportxml")[0].Attributes["progress"].Value);
                    if(percentCompleted < 100)
                    {
                        var settings = new XmlWriterSettings { Indent = true };
                        var xmlStringBuilder = new StringBuilder();
                        using (var xmlTextWriter = XmlWriter.Create(xmlStringBuilder, settings))
                        {
                            xmlDocument.WriteTo(xmlTextWriter);
                            xmlTextWriter.Flush();
                        }
                        response.FailedSolution = solutionFile;
                        response.FailedSolutionXml = xmlStringBuilder.ToString();
                    }
                    else
                    {
                        ProcessCompletedSolutionImportJob(response, xrmService, importId);
                    }
                }
            }
        }
    }
}