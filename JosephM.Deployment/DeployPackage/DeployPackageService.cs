#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.ImportXml;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;

#endregion

namespace JosephM.Deployment.DeployPackage
{
    public class DeployPackageService :
        DataImportServiceBase<DeployPackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public DeployPackageService(XrmRecordService xrmRecordService)
            : base(xrmRecordService)
        {
        }

        public override void ExecuteExtention(DeployPackageRequest request, ServiceResponseBase<DataImportResponseItem> response,
            LogController controller)
        {
            DeployPackage(request, controller, response);
        }

        private void DeployPackage(DeployPackageRequest request, LogController controller, ServiceResponseBase<DataImportResponseItem> response)
        {
            var xrmRecordService = new XrmRecordService(request.Connection, controller);
            var packageFolder = request.FolderContainingPackage.FolderPath;
            var solutionFiles = Directory.GetFiles(packageFolder, "*.zip");

            ImportSolutions(solutionFiles, controller, xrmRecordService);

            foreach (var childFolder in Directory.GetDirectories(packageFolder))
            {
                if (new DirectoryInfo(childFolder).Name == "Data")
                {
                    var dataImportService = new ImportXmlService(xrmRecordService);
                    var importResponse = new ImportXmlResponse();
                    dataImportService.ImportXml(childFolder, controller, importResponse);
                    if (importResponse.Exception != null)
                        response.AddResponseItem(new DataImportResponseItem("Fatal Data Import Error", importResponse.Exception));
                    foreach (var item in importResponse.ResponseItems)
                        response.AddResponseItem(item);
                }
            }
        }

        private object _lockObject = new object();

        public void ImportSolutions(IEnumerable<string> solutionFiles, LogController controller, XrmRecordService xrmRecordService)
        {
            var countToDo = solutionFiles.Count();
            var countRecordsImported = 0;

            var xrmService = XrmRecordService.XrmService;

            controller.LogLiteral($"Loading Active {xrmService.GetEntityCollectionName(Entities.duplicaterule)}");
            var duplicateRules = xrmService.RetrieveAllAndClauses(Entities.duplicaterule, new[] { new ConditionExpression(Fields.duplicaterule_.statecode, ConditionOperator.Equal, OptionSets.DuplicateDetectionRule.Status.Active) }, new string[0]);

            foreach (var solutionFile in solutionFiles)
            {
                try
                {
                    controller.UpdateProgress(++countRecordsImported, countToDo + 1,
                        $"Importing solution {new FileInfo(solutionFile).Name} into {XrmRecordService.XrmRecordConfiguration?.ToString()}");
                    var importId = Guid.NewGuid();
                    var req = new ImportSolutionRequest();
                    req.ImportJobId = importId;
                    req.CustomizationFile = File.ReadAllBytes(solutionFile);
                    req.PublishWorkflows = true;
                    req.OverwriteUnmanagedCustomizations = true;

                    var finished = new Processor();
                    var extraService = new XrmService(xrmService.XrmConfiguration);
                    var monitoreProgressThread = new Thread(() => DoProgress(importId, controller.GetLevel2Controller(), finished, extraService));
                    monitoreProgressThread.IsBackground = true;
                    monitoreProgressThread.Start();
                    try
                    {
                        xrmService.Execute(req);
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