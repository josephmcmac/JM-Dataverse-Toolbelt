#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using JosephM.Core.FieldType;
using JosephM.Xrm.ImportExporter.Solution.Service;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Sql;
using JosephM.Core.Utility;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    public class XrmSolutionImporterExporterService:
        ServiceBase<XrmSolutionImporterExporterRequest, XrmSolutionImporterExporterResponse, XrmSolutionImporterExporterResponseItem>
    {
        public override void ExecuteExtention(XrmSolutionImporterExporterRequest request, XrmSolutionImporterExporterResponse response,
            LogController controller)
        {
            switch (request.ImportExportTask)
            {
                case SolutionImportExportTask.ExportSolutions:
                {
                    ExportSolutions(request.SolutionExports, request.FolderPath.FolderPath, controller, response);
                    break;
                }
                case SolutionImportExportTask.ImportSolutions:
                {
                    ImportSolutions(request, controller, response);
                    break;
                }
                case SolutionImportExportTask.MigrateSolutions:
                {
                    MigrateSolutions(request, controller, response);
                    break;
                }
            }
        }

        private void MigrateSolutions(XrmSolutionImporterExporterRequest request, LogController controller, XrmSolutionImporterExporterResponse response)
        {
            var exported = ExportSolutions(request.SolutionMigrations, request.FolderPath.FolderPath, controller, response);
            var imports = new List<ExportedSolutionImport>();
            foreach (var item in request.SolutionMigrations)
            {
                var thisItem = item;
                var matchingExports = exported.Where(e => e.Export == thisItem);
                if(!matchingExports.Any())
                    throw new NullReferenceException(string.Format("Error Preparing Import Solutions - Solution {0} Was Not Matched In The Exports", thisItem.Solution.Name));
                imports.Add(new ExportedSolutionImport(matchingExports.First(), thisItem));
            }
            var importToRecordService = new XrmRecordService(request.ImportToConnection, controller);
            ImportSolutions(imports, controller, response, importToRecordService.XrmService);

            var dataPath = GetDataExportFolder(request.FolderPath.FolderPath);
            if (Directory.Exists(dataPath))
            {
                var dataImportService = new XrmImporterExporterService<XrmRecordService>(importToRecordService);
                var importResponse = new XrmImporterExporterResponse();
                dataImportService.ImportXml(dataPath, controller, importResponse);
                if(importResponse.Exception != null)
                    response.AddResponseItem(new XrmSolutionImporterExporterResponseItem("Fatal Data Import Error", importResponse.Exception));
                foreach (var item in importResponse.ResponseItems)
                    response.AddResponseItem(new XrmSolutionImporterExporterResponseItem(item));
            }
        }

        private object _lockObject = new object();

        private void ImportSolutions(XrmSolutionImporterExporterRequest request, LogController controller,
            XrmSolutionImporterExporterResponse response)
        {
            var xrmRecordService = new XrmRecordService(request.ImportToConnection, controller);
            ImportSolutions(request.SolutionImports, controller, response, xrmRecordService.XrmService);
            if (request.IncludeImportDataInFolder != null && Directory.Exists(request.IncludeImportDataInFolder.FolderPath))
            {
                var dataImportService = new XrmImporterExporterService<XrmRecordService>(xrmRecordService);
                var importResponse = new XrmImporterExporterResponse();
                dataImportService.ImportXml(request.IncludeImportDataInFolder.FolderPath, controller, importResponse);
                if (importResponse.Exception != null)
                    response.AddResponseItem(new XrmSolutionImporterExporterResponseItem("Fatal Data Import Error", importResponse.Exception));
                foreach (var item in importResponse.ResponseItems)
                    response.AddResponseItem(new XrmSolutionImporterExporterResponseItem(item));
            }
        }

        private void ImportSolutions(IEnumerable<ISolutionImport> imports, LogController controller, XrmSolutionImporterExporterResponse response, XrmService xrmService)
        {
            var countToDo = imports.Count();
            var countRecordsImported = 0;
            foreach (var import in imports.OrderBy(s => s.ImportOrder))
            {
                try
                {
                    controller.UpdateProgress(++countRecordsImported, countToDo + 1,
                        "Importing Solution " + import.SolutionFile);
                    var importId = Guid.NewGuid();
                    var req = new ImportSolutionRequest();
                    req.ImportJobId = importId;
                    req.CustomizationFile = File.ReadAllBytes(import.SolutionFile.FileName);
                    req.PublishWorkflows = import.PublishWorkflows;
                    req.OverwriteUnmanagedCustomizations = import.OverwriteCustomisations;

                    var finished = new Processor();
                    var extraService = new XrmService(xrmService.XrmConfiguration);
                    new Thread(() => DoProgress(importId, controller.GetLevel2Controller(), finished, extraService))
                        .Start();
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
                        throw new Exception(string.Format("Error Importing Solution {0}\n{1}\n{2}", import.SolutionFile.FileName, ex.Message, ex.Detail.InnerFault.InnerFault.Message), ex);
                    }
                    else
                        throw new Exception(
                            string.Format("Error Importing Solution {0}\n{1}", import.SolutionFile.FileName, ex.Message), ex);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error Importing Solution {0}", import.SolutionFile.FileName), ex);
                }
            }
            controller.TurnOffLevel2();
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
                            controller.UpdateProgress(Convert.ToInt32(progress/1), 100, "Import Progress");
                        }
                    }
                }

                catch (Exception ex)
                {
                    controller.LogLiteral("Unexpected Error " + ex.Message);
                }

            }
        }

        private string GetDataExportFolder(string rootFolder)
        {
            return rootFolder == null ? null : Path.Combine(rootFolder, "Data");
        }

        private IEnumerable<ExportedSolution> ExportSolutions(IEnumerable<SolutionExport> exports, string folderPath, LogController controller, XrmSolutionImporterExporterResponse response)
        {
            var exportResponse = new List<ExportedSolution>();
            var countToDo = exports.Count();
            var countRecordsImported = 0;
            foreach (var export in exports)
            {
                try
                {
                    controller.UpdateProgress(++countRecordsImported, countToDo + 1,
                        "Exporting Solution " + export.Solution.Name);
                    var xrmRecordService = new XrmRecordService(export.Connection, controller);
                    var service = xrmRecordService.XrmService;
                    var solution = service.Retrieve("solution", new Guid(export.Solution.Id));
                    var uniqueName = (string) solution.GetStringField("uniquename");
                    var req = new ExportSolutionRequest();
                    req.Managed = export.Managed;
                    req.SolutionName = uniqueName;
                    var version = solution.GetStringField("version");
                    var versionText = version == null ? null : version.Replace(".", "_");
                    var eresponse = (ExportSolutionResponse) service.Execute(req);
                    var fileName = string.Format("{0}_{1}{2}.zip", uniqueName, versionText,
                        export.Managed ? "_managed" : null);
                    FileUtility.WriteToFile(folderPath, fileName, eresponse.ExportSolutionFile);
                    exportResponse.Add(new ExportedSolution(export,
                        new FileReference(Path.Combine(folderPath, fileName))));

                    if (export.DataToExport != null && export.DataToExport.Any())
                    {
                        try
                        {
                            var dataExportService = new XrmImporterExporterService<XrmRecordService>(xrmRecordService);
                            dataExportService.ExportXml(export.DataToExport,
                                new Folder(GetDataExportFolder(folderPath)), export.IncludeNotes,
                                export.IncludeNNRelationshipsBetweenEntities, controller);
                        }
                        catch (Exception ex)
                        {
                            response.AddResponseItem(
                                new XrmSolutionImporterExporterResponseItem(
                                    string.Format("Error Exporting Data For {0}", export.Solution.Name), ex));
                        }

                    }
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(
                        new XrmSolutionImporterExporterResponseItem(
                            string.Format("Error Exporting Solution {0}", export.Solution.Name), ex));
                                    }
            }
            return exportResponse;
        }
    }
}