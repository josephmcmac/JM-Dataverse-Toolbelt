using JosephM.Core.Log;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
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

        public IEnumerable<SolutionImportResult> ImportSolutions(Dictionary<string, byte[]> solutionFiles, LogController controller)
        {
            var results = new List<SolutionImportResult>();
            var xrmService = XrmRecordService.XrmService;

            controller.LogLiteral($"Loading Active {xrmService.GetEntityCollectionName(Entities.duplicaterule)}");
            var duplicateRules = xrmService.RetrieveAllAndClauses(Entities.duplicaterule, new[] { new ConditionExpression(Fields.duplicaterule_.statecode, ConditionOperator.Equal, OptionSets.DuplicateDetectionRule.Status.Active) }, new string[0]);

            foreach (var solutionFile in solutionFiles)
            {
                try
                {
                    controller.LogLiteral(
                        $"Importing Solution {solutionFile.Key} Into {XrmRecordService.XrmRecordConfiguration?.ToString()}");
                    var importId = Guid.NewGuid();
                    var req = new ImportSolutionRequest();
                    req.ImportJobId = importId;
                    req.CustomizationFile = solutionFile.Value;
                    req.PublishWorkflows = true;
                    req.OverwriteUnmanagedCustomizations = true;

                    var finished = new Processor();
                    var extraService = new XrmService(xrmService.XrmConfiguration, new LogController(), xrmService.ServiceFactory);
                    var monitoreProgressThread = new Thread(() => DoProgress(importId, controller.GetLevel2Controller(), finished, extraService));
                    monitoreProgressThread.IsBackground = true;
                    monitoreProgressThread.Start();
                    try
                    {
                        xrmService.Execute(req);
                        var job = xrmService.GetFirst("importjob", "importjobid", importId);
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
                                    results.Add(importResult);
                            }
                        }
                    }
                    finally
                    {
                        lock (_lockObject)
                            finished.Completed = true;
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (ex.Detail != null && ex.Detail.InnerFault != null && ex.Detail.InnerFault.InnerFault != null)
                    {
                        throw new Exception(string.Format("Error Importing Solution {0}\n{1}\n{2}", solutionFile, ex.Message, ex.Detail.InnerFault.InnerFault.Message), ex);
                    }
                    else
                        throw new Exception(
                            string.Format("Error Importing Solution {0}\n{1}", solutionFile, ex.Message), ex);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error Importing Solution {0}", solutionFile), ex);
                }
            }
            controller.TurnOffLevel2();
            controller.LogLiteral("Publishing Customisations");
            xrmService.Publish();

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
            return results;
        }

        public class Processor
        {
            public bool Completed { get; set; }
        }

        public void DoProgress(Guid importId, LogController controller, Processor finished, XrmService xrmService)
        {
            lock (_lockObject)
            {
                if (!finished.Completed)
                    controller.UpdateProgress(0, 100, "Import Progress");
            }
            while (true)
            {
                Thread.Sleep(5000);
                // connect to crm again, don't reuse the connection that's used to import
                lock (_lockObject)
                {
                    if (finished.Completed)
                        return;
                }
                try
                {
                    var job = xrmService.GetFirst("importjob", "importjobid", importId);
                    if (job != null)
                    {
                        var progress = job.GetDoubleValue("progress");
                        lock (_lockObject)
                        {
                            if (finished.Completed)
                                return;
                            controller.UpdateProgress(Convert.ToInt32(progress / 1), 100, "Import Progress");
                        }
                    }
                }

                catch (Exception ex)
                {
                    controller.LogLiteral("Unexpected Error " + ex.Message);
                }

            }
        }
    }
}