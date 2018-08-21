using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    public class DeployWebResourceService :
        ServiceBase<DeployWebResourceRequest, DeployWebResourceResponse, DeployWebResourceResponseItem>
    {
        public DeployWebResourceService(XrmRecordService service, XrmPackageSettings packageSettings)
        {
            Service = service;
            PackageSettings = packageSettings;
        }

        public XrmPackageSettings PackageSettings { get; set; }
        public XrmRecordService Service { get; set; }

        public override void ExecuteExtention(DeployWebResourceRequest request, DeployWebResourceResponse response,
            LogController controller)
        {
            var records = new List<IRecord>();

            var publishIds = new List<string>();

            var totalTasks = request.Files.Count() + 1;
            var tasksCompleted = 0;
            foreach (var file in request.Files)
            {
                var fileInfo = new FileInfo(file);
                var fileName = fileInfo.Name;
                controller.UpdateProgress(++tasksCompleted, totalTasks, "Deploying " + fileName);

                var responseItem = new DeployWebResourceResponseItem();
                responseItem.Name = fileName;
                response.AddResponseItem(responseItem);

                try
                {

                    var content = File.ReadAllBytes(file);
                    var contentString = Convert.ToBase64String(content);

                    //okay lets allow match on either name or display name
                    var match = Service.GetFirst(Entities.webresource, Fields.webresource_.name, fileName);
                    if (match == null)
                    {
                        match = Service.GetFirst(Entities.webresource, Fields.webresource_.displayname, fileName);
                    }
                    if (match != null)
                    {
                        if (match.GetStringField(Fields.webresource_.content) != contentString)
                        {
                            match.SetField(Fields.webresource_.content, contentString, Service);
                            Service.Update(match, new[] { Fields.webresource_.content });
                            publishIds.Add(match.Id);
                            responseItem.Updated = true;
                        }
                    }
                    else
                    {
                        var record = Service.NewRecord(Entities.webresource);
                        record.SetField(Fields.webresource_.name, fileInfo.Name, Service);
                        record.SetField(Fields.webresource_.displayname, fileInfo.Name, Service);
                        record.SetField(Fields.webresource_.content, Convert.ToBase64String(content), Service);
                        record.SetField(Fields.webresource_.webresourcetype, GetWebResourceType(fileInfo.Extension), Service);
                        record.Id = Service.Create(record);
                        publishIds.Add(record.Id);
                        responseItem.Created = true;
                    }
                }
                catch(Exception ex)
                {
                    responseItem.Exception = ex;
                }
            }

            if (publishIds.Any())
            {
                controller.UpdateProgress(totalTasks, totalTasks, "Publising Files");
                var xml = new StringBuilder();
                xml.Append("<importexportxml><webresources>");
                foreach (var id in publishIds)
                    xml.Append("<webresource>" + id + "</webresource>");
                xml.Append("</webresources></importexportxml>");
                Service.Publish(xml.ToString());
            }

            //add plugin assembly to the solution
            var componentType = OptionSets.SolutionComponent.ObjectTypeCode.WebResource;
            if (PackageSettings.AddToSolution)
                Service.AddSolutionComponents(PackageSettings.Solution.Id, componentType, publishIds);
        }

        public int GetWebResourceType(string extention)
        {
            if (extention != null && extention.StartsWith("."))
                extention = extention.Substring(1);
            if (WebResourceTypes.ContainsKey(extention))
                return WebResourceTypes[extention];
            throw new NotSupportedException(string.Format("No matching web resource type implemented for extention {0}", extention));
        }

        public static Dictionary<string, int> WebResourceTypes
        {
            get
            {
                return new Dictionary<string, int>()
                {
                    { "js",  OptionSets.WebResource.Type.ScriptJScript},
                    { "xml",  OptionSets.WebResource.Type.DataXML},
                    { "gif",  OptionSets.WebResource.Type.GIFformat},
                    { "ico",  OptionSets.WebResource.Type.ICOformat},
                    { "jpg",  OptionSets.WebResource.Type.JPGformat},
                    { "jpeg",  OptionSets.WebResource.Type.JPGformat},
                    { "png",  OptionSets.WebResource.Type.PNGformat},
                    { "xap",  OptionSets.WebResource.Type.SilverlightXAP},
                    { "css",  OptionSets.WebResource.Type.StyleSheetCSS},
                    { "xsl",  OptionSets.WebResource.Type.StyleSheetXSL},
                    { "html",  OptionSets.WebResource.Type.WebpageHTML},
                    { "htm",  OptionSets.WebResource.Type.WebpageHTML},
                };
            }
        }
    }
}