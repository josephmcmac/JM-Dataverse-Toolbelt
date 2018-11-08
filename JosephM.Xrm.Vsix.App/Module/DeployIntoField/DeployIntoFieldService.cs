using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    public class DeployIntoFieldService :
        ServiceBase<DeployIntoFieldRequest, DeployIntoFieldResponse, DeployIntoFieldResponseItem>
    {
        public DeployIntoFieldService(XrmRecordService service, XrmPackageSettings packageSettings)
        {
            Service = service;
            PackageSettings = packageSettings;
        }

        public XrmPackageSettings PackageSettings { get; set; }
        public XrmRecordService Service { get; set; }

        public override void ExecuteExtention(DeployIntoFieldRequest request, DeployIntoFieldResponse response,
            ServiceRequestController controller)
        {
            var records = new List<IRecord>();

            var publishIds = new List<string>();

            var numberToDo = request.Files.Count();
            var numberDone = 0;
            foreach (var file in request.Files)
            {
                var fileInfo = new FileInfo(file);
                controller.UpdateProgress(++numberDone, numberToDo, "Importing " + fileInfo.Name);
                var thisResponseItem = new DeployIntoFieldResponseItem()
                {
                    Name = fileInfo.Name
                };
                response.AddResponseItem(thisResponseItem);
                try
                {

                    var containingFolderName = fileInfo.Directory.Name;
                    //get target record type
                    string recordType = null;
                    if (Service.GetAllRecordTypes().Any(r => r == containingFolderName))
                    {
                        recordType = Service.GetAllRecordTypes().First(r => r == containingFolderName);
                    }
                    else if (Service.GetAllRecordTypes().Any(r => Service.GetDisplayName(r)?.ToLower() == containingFolderName.ToLower()))
                    {
                        recordType = Service.GetAllRecordTypes().First(r => Service.GetDisplayName(r)?.ToLower() == containingFolderName.ToLower());
                    }
                    else
                    {
                        throw new NullReferenceException($"Could not find matching type by logical or display name for folder name of {containingFolderName}");
                    }

                    if (recordType == Entities.adx_webfile)
                    {
                        //this one goes into an attachment
                        var matchingRecord = Service.GetFirst(recordType, Service.GetPrimaryField(recordType), fileInfo.Name);
                        if (matchingRecord == null)
                        {
                            throw new NullReferenceException($"There is no {Service.GetDisplayName(recordType)} record name {fileInfo.Name} to load the file attachment into");
                        }
                        //get matching attachment by name else create a new one
                        var fileAttachments = Service.RetrieveAllAndClauses(Entities.annotation, new[]
                        {
                            new Condition(Fields.annotation_.filename, ConditionType.Equal, fileInfo.Name),
                            new Condition(Fields.annotation_.objectid, ConditionType.Equal, matchingRecord.Id)
                        }).OrderBy(n => n.GetDateTime(Fields.annotation_.createdon)).ToArray();

                        var contentBytes = File.ReadAllBytes(file);
                        var contentBase64String = Convert.ToBase64String(contentBytes);
                        if (fileAttachments.Any())
                        {
                            //lets update the last modifed one
                            var attachmentToUpdate = fileAttachments.First();
                            if (attachmentToUpdate.GetStringField(Fields.annotation_.documentbody) != contentBase64String)
                            {
                                attachmentToUpdate.SetField(Fields.annotation_.documentbody, contentBase64String, Service);
                                Service.Update(attachmentToUpdate, new[] { Fields.annotation_.documentbody });
                                thisResponseItem.Updated = true;
                            }
                        }
                        else
                        {
                            //lets create a new attachment
                            var newAttachment = Service.NewRecord(Entities.annotation);
                            newAttachment.SetLookup(Fields.annotation_.objectid, matchingRecord.Id, matchingRecord.Type);
                            newAttachment.SetField(Fields.annotation_.subject, fileInfo.Name, Service);
                            newAttachment.SetField(Fields.annotation_.filename, fileInfo.Name, Service);
                            newAttachment.SetField(Fields.annotation_.documentbody, contentBase64String, Service);
                            Service.Create(newAttachment);
                            thisResponseItem.Created = true;
                        }
                    }
                    else
                    {
                        //get the record with the same name as the file
                        var fileNameSansExtention = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf("."));

                        var conditions = new List<Condition>
                        {
                            new Condition(Service.GetPrimaryField(recordType), ConditionType.Equal, fileNameSansExtention)
                        };
                        if (recordType == Entities.adx_webpage)
                            conditions.Add(new Condition(Fields.adx_webpage_.adx_rootwebpageid, ConditionType.NotNull));

                        var matchingRecords = Service.RetrieveAllAndClauses(recordType, conditions);
                        if (matchingRecords.Count() != 1)
                        {
                            throw new NullReferenceException($"Could not find {Service.GetDisplayName(recordType)} named {fileInfo.Name} to update");
                        }
                        var matchingRecord = matchingRecords.First();

                        var targetField = GetTargetField(fileInfo, recordType);

                        var contentText = File.ReadAllText(file);
                        if (matchingRecord.GetStringField(targetField) != contentText)
                        {
                            matchingRecord.SetField(targetField, contentText, Service);
                            Service.Update(matchingRecord, new[] { targetField });
                            thisResponseItem.Updated = true;
                        }
                    }
                }
                catch(Exception ex)
                {
                    thisResponseItem.Exception = ex;
                }
            }
        }

        private string GetTargetField(FileInfo fileInfo, string recordType)
        {
            //adx_webtemplate -> adx_source (Source)
            //adx_entitylist -> adx_registerstartupscript (Custom JavaScript)
            //adx_webpage -> adx_customjavascript (Custom JavaScript)
            //adx_webpage -> adx_customcss (Custom CSS)

            if (recordType == "adx_webfile")
                return "adx_source";
            if (fileInfo.Name.EndsWith("css"))
            {
                var matchingFields = Service.GetFields(recordType).Where(f => f.Contains("css"));
                if (matchingFields.Count() != 1)
                    throw new Exception($"Could not find unique field in {recordType} with name containing 'css'");
                return matchingFields.First();
            }
            if (fileInfo.Name.EndsWith("js"))
            {
                var matchingFields = Service.GetFields(recordType).Where(f => Service.GetFieldLabel(f,recordType)?.ToLower()?.Contains("javascript") ?? false);
                if (matchingFields.Count() != 1)
                    throw new Exception($"Could not find unique field in {recordType} where label contains 'javascript'");
                return matchingFields.First();
            }
            if (fileInfo.Name.EndsWith("htm") || fileInfo.Name.EndsWith("html"))
            {
                if (recordType == Entities.adx_webpage)
                    return "adx_copy";
                var matchingFields = Service.GetFields(recordType).Where(f => f.Contains("source"));
                if (matchingFields.Count() != 1)
                    throw new Exception($"Could not find unique field in {recordType} with name containing 'source'");
                return matchingFields.First();
            }
            throw new NotImplementedException($"The file extention {fileInfo.Extension} is not implemented");
        }

        public static IEnumerable<string> IntoFieldTypes
        {
            get
            {
                return new [] { "js", "css", "html", "htm" };
            }
        }
    }
}