using JosephM.Application.ViewModel.Email;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Sql;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportSql
{
    public class ImportSqlService :
        ServiceBase<ImportSqlRequest, ImportSqlResponse, ImportSqlResponseItem>
    {
        public ImportSqlService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(ImportSqlRequest request, ImportSqlResponse response,
            ServiceRequestController controller)
        {
            Exception tempEx = null; ;
            try
            {
                controller.Controller.UpdateProgress(0, 1, "Loading Records For Import");
                var dictionary = LoadMappingDictionary(request);
                var importService = new SpreadsheetImportService(XrmRecordService);
                var responseItems = importService.DoImport(dictionary, request.MaskEmails, request.MatchRecordsByName, request.UpdateOnly, controller);
                response.Connection = XrmRecordService.XrmRecordConfiguration;
                response.LoadSpreadsheetImport(responseItems);
                response.Message = "The Import Process Has Completed";
            }
            catch (Exception ex)
            {
                tempEx = ex;
                throw;
            }
            finally
            {
                if (request.SendNotificationAtCompletion
                    && (!request.OnlySendNotificationIfError
                        || tempEx != null || response.HasResponseItemError))
                {
                    try
                    {
                        //todo place the html generator somewhere appropriate
                        var htmlGenerator = new HtmlEmailGenerator();
                        //okay lets try create an email with the summary
                        //plus perhaps a log of the errors

                        if (tempEx != null)
                        {
                            htmlGenerator.AppendParagraph(tempEx.XrmDisplayString());
                        }

                        htmlGenerator.WriteObject(response);

                        var subject = $"Sql Import '{request.Name ?? "No Name"}'";
                        if (tempEx != null)
                            subject = subject + " Fatal Error";
                        else if (response.HasResponseItemError)
                            subject = subject + " Completed With Errors";
                        else
                            subject = subject + " Completed";



                        XrmRecordService.SendEmail(request.SendNotificationFromQueue, request.SendNotificationToQueue
                            , subject.Left(XrmRecordService.GetMaxLength(Fields.email_.subject, Entities.email))
                            , htmlGenerator.GetContent());
                    }
                    catch(Exception ex)
                    {
                        response.AddResponseItem(new ImportSqlResponseItem(new DataImportResponseItem("Error Sending Notification Email", ex)));
                    }
                }
            }
        }

        public Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>> LoadMappingDictionary(ImportSqlRequest request)
        {
            var excelService = new SqlRecordService(new SqlConnectionString(request.ConnectionString));

            var dictionary = new Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>>();

            foreach (var tabMapping in request.Mappings)
            {
                if (tabMapping.TargetType != null)
                {
                    var queryRows = excelService.RetrieveAll(tabMapping.SourceTable.Key, null);
                    dictionary.Add(tabMapping, queryRows);
                }
            }

            return dictionary;
        }
    }
}