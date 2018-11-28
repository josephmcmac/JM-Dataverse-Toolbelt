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
    public class AddPortalCodeService :
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

            var exportConfigs = ExportConfigs;
            var toDo = exportConfigs.Count();
            var done = 0;
            //okay so lets iterate the configurations
            //and for each item not null add somehting to the vs solution
            foreach (var config in exportConfigs)
            {
                controller.UpdateProgress(done++, toDo, "Exporting " + config.RecordType + " Code");
                var query = new QueryDefinition(config.RecordType);
                if (config.Conditions != null)
                    query.RootFilter.Conditions.AddRange(config.Conditions);
                Service.AddJoinCondition(query, config.WebSiteField, ConditionType.Equal, request.WebSite.Id);
                var results = Service.RetreiveAll(query);
                var toDo2 = results.Count();
                var done2 = 0;
                foreach (var result in results)
                {
                    ++done2;
                    if (config.RecordType == Entities.adx_webfile)
                        controller.UpdateLevel2Progress(done2, toDo2, $"Processing {done2}/{toDo2}");
                    var recordName = result.GetStringField(Service.GetPrimaryField(config.RecordType));
                    var rootFolderName = request.WebSite.Name;
                    var thisTypesFolderLabel = Service.GetDisplayName(config.RecordType);
                    try
                    {
                        if (config.RecordType == Entities.adx_webfile)
                        {
                            var notes = Service.GetLinkedRecords(Entities.annotation, config.RecordType, Fields.annotation_.objectid, result.Id);
                            if (notes.Count() > 1)
                                throw new Exception("There is more than 1 note attached to the file. You will need to delete the excess notes to export");
                            else if (notes.Any())
                            {
                                var note = notes.First();
                                var fileContentBase64 = note.GetStringField(Fields.annotation_.documentbody);
                                if(fileContentBase64 != null)
                                {
                                    var data = Convert.FromBase64String(fileContentBase64);
                                    var content = Encoding.UTF8.GetString(data);
                                    var fileName = recordName;
                                    project.AddItem(fileName, content, new[] { rootFolderName, thisTypesFolderLabel });
                                }
                            }
                        }
                        else
                        {
                            foreach (var field in config.FieldsToExport)
                            {
                                var content = result.GetStringField(field.FieldName);
                                if (content != null)
                                {
                                    var fileExtention = field.Extention;
                                    var fileName = recordName + "." + fileExtention;
                                    project.AddItem(fileName, content, new[] { rootFolderName, thisTypesFolderLabel });
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

        private IEnumerable<ExportConfiguration> ExportConfigs
        {
            get
            {
                return new []
                {
                    new ExportConfiguration
                    {
                        RecordType = Entities.adx_webpage,
                        Conditions = new [] { new Condition(Fields.adx_webpage_.adx_rootwebpageid, ConditionType.NotNull) },
                        WebSiteField = Fields.adx_webpage_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new ExportConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webpage_.adx_copy,
                                Extention = "html"
                            },
                            new ExportConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webpage_.adx_customjavascript,
                                Extention = "js"
                            },
                            new ExportConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webpage_.adx_customcss,
                                Extention = "css"
                            }
                        }
                    },
                    new ExportConfiguration
                    {
                        RecordType = Entities.adx_webformstep,
                        WebSiteField = Fields.adx_webformstep_.adx_webform + "." + Fields.adx_webform_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new ExportConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webformstep_.adx_registerstartupscript,
                                Extention = "js"
                            }
                        }
                    },
                    new ExportConfiguration
                    {
                        RecordType = Entities.adx_webtemplate,
                        WebSiteField = Fields.adx_webtemplate_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new ExportConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_webtemplate_.adx_source,
                                Extention = "html"
                            }
                        }
                    },
                    new ExportConfiguration
                    {
                        RecordType = Entities.adx_entityform,
                        WebSiteField = Fields.adx_entityform_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new ExportConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_entityform_.adx_registerstartupscript,
                                Extention = "js"
                            }
                        }
                    },
                    new ExportConfiguration
                    {
                        RecordType = Entities.adx_entitylist,
                        WebSiteField = Fields.adx_entitylist_.adx_websiteid,
                        FieldsToExport = new []
                        {
                            new ExportConfiguration.ExportFieldConfigration
                            {
                                FieldName = Fields.adx_entitylist_.adx_registerstartupscript,
                                Extention = "js"
                            }
                        }
                    },
                    new ExportConfiguration
                    {
                        RecordType = Entities.adx_webfile,
                        WebSiteField = Fields.adx_webfile_.adx_websiteid
                    }
                };
            }
        }


        private class ExportConfiguration
        {
            public string RecordType { get; set; }
            public string WebSiteField { get; set; }
            public IEnumerable<ExportFieldConfigration> FieldsToExport { get; set; }
            public IEnumerable<Condition> Conditions { get; set; }

            public class ExportFieldConfigration
            {
                public string FieldName { get; set; }
                public string Extention { get; set; }
            }
        }
    }
}