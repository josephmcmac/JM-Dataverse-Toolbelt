using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.Xrm.Vsix.Module.AddPortalCode
{
    public partial class AddPortalCodeService :
        ServiceBase<AddPortalCodeRequest, AddPortalCodeResponse, AddPortalCodeResponseItem>
    {
        public AddPortalCodeService(XrmRecordService service, IVisualStudioService visualStudioService)
        {
            Service = service;
            VisualStudioService = visualStudioService;
        }

        public XrmRecordService Service { get; set; }
        public IVisualStudioService VisualStudioService { get; }

        public override void ExecuteExtention(AddPortalCodeRequest request, AddPortalCodeResponse response,
            ServiceRequestController controller)
        {
            var project = VisualStudioService.GetProject(request.ProjectName);

            var exportConfigs = AddPortalCodeConfiguration.GetExportConfigs()
                .Where(c => request.IncludeType(c.RecordType))
                .ToArray();
            var toDo = exportConfigs.Count();
            var done = 0;
            //okay so lets iterate the configurations
            //and for each item not null add somehting to the vs solution
            foreach (var config in exportConfigs)
            {
                controller.UpdateProgress(done++, toDo, "Exporting " + config.RecordType + " Code");
                var results = AddPortalCodeConfiguration.GetRecordsForConfig(config.RecordType, Service, request.WebSite.Id);
                results = request.FilterInclusionForType(config.RecordType, results);
                var toDo2 = results.Count();
                var done2 = 0;
                foreach (var result in results)
                {
                    ++done2;
                    if (config.RecordType == Entities.adx_webfile)
                        controller.UpdateLevel2Progress(done2, toDo2, $"Processing {done2}/{toDo2}");
                    var recordName = result.GetStringField(Service.GetPrimaryField(config.RecordType));
                    var thisTypesFolderLabel = Service.GetDisplayName(config.RecordType);
                    var path = new List<string>();
                    if (request.CreateFolderForWebsiteName)
                        path.Add(request.WebSite.Name);
                    path.Add(thisTypesFolderLabel);
                    try
                    {
                        if (config.RecordType == Entities.adx_webfile)
                        {
                            var notes = Service.GetLinkedRecords(Entities.annotation, config.RecordType, Fields.annotation_.objectid, result.Id);
                            if (notes.Count() > 1)
                                throw new Exception("There is more than 1 note attached to the file. You will need to delete the excess notes to export");
                            else if (notes.Any() || request.ExportWhereFieldEmpty)
                            {
                                string fileContent = notes.Any()
                                    ? notes.First().GetStringField(Fields.annotation_.documentbody)
                                    : null;
                                if (fileContent != null)
                                {
                                    var data = Convert.FromBase64String(fileContent);
                                    fileContent = Encoding.UTF8.GetString(data);
                                }
                                var fileName = recordName;
                                if (IncludeExtention(request, fileName))
                                    project.AddItem(fileName, fileContent, path.ToArray());
                            }
                        }
                        else
                        {
                            foreach (var field in config.FieldsToExport)
                            {
                                var content = result.GetStringField(field.FieldName);
                                if (content != null || request.ExportWhereFieldEmpty)
                                {
                                    var fileExtention = field.Extention;
                                    var fileName = recordName + "." + fileExtention;
                                    if (IncludeExtention(request, fileName))
                                        project.AddItem(fileName, content, path.ToArray());
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        response.AddResponseItem(new AddPortalCodeResponseItem() { RecordType = config.RecordType, RecordName = recordName, Exception = ex });
                    }
                }
                controller.TurnOffLevel2();
            }
        }

        public bool IncludeExtention(AddPortalCodeRequest request, string fileName)
        {
            if(fileName != null && fileName.LastIndexOf(".") > -1 && fileName.Length > fileName.LastIndexOf("."))
            {
                var extention = fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower();
                if (extention == "htm" || extention == "html")
                    return request.IncludeHtml;
                if (extention == "js")
                    return request.IncludeJavaScript;
                if (extention == "css")
                    return request.IncludeCss;
            }
            return request.IncludeOtherTypes;
        }
    }
}