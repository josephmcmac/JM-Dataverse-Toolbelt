using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XRM.VSIX.Utilities;

namespace JosephM.XRM.VSIX.Commands.DeployWebResource
{
    public class DeployWebResourcesService :
        ServiceBase<DeployWebResourcesRequest, DeployWebResourcesResponse, DeployWebResourcesResponseItem>
    {
        public DeployWebResourcesService(XrmRecordService service, XrmPackageSettings packageSettings)
        {
            Service = service;
            PackageSettings = packageSettings;
        }

        public XrmPackageSettings PackageSettings { get; set; }
        private XrmRecordService Service { get; set; }

        public override void ExecuteExtention(DeployWebResourcesRequest request, DeployWebResourcesResponse response,
            LogController controller)
        {
            var records = new List<IRecord>();
            var type = Entities.webresource;

            controller.UpdateProgress(1, 3, "Loading Files");
            foreach (var file in request.Files)
            {
                var fileInfo = new FileInfo(file);

                var record = Service.NewRecord(type);
                var content = File.ReadAllBytes(file);
                record.SetField(Fields.webresource_.name, fileInfo.Name, Service);
                record.SetField(Fields.webresource_.displayname, fileInfo.Name, Service);
                record.SetField(Fields.webresource_.content, Convert.ToBase64String(content), Service);
                record.SetField(Fields.webresource_.webresourcetype, GetWebResourceType(fileInfo.Extension), Service);
                records.Add(record);
            }

            var matchField = Fields.webresource_.name;
            controller.UpdateProgress(2, 3, "Deploying Files");
            var loadResponse = VsixUtility.LoadIntoCrm(Service, records, matchField);
            foreach (var item in records)
            {
                var responseItem = new DeployWebResourcesResponseItem();
                responseItem.Name = item.GetStringField(Fields.webresource_.name);
                responseItem.Created = loadResponse.Created.Contains(item);
                responseItem.Updated = loadResponse.Updated.Contains(item);
                if (loadResponse.Errors.ContainsKey(item))
                    responseItem.Exception = loadResponse.Errors[item];
                response.AddResponseItem(responseItem);
            }

            if (loadResponse.Updated.Any())
            {
                controller.UpdateProgress(3, 3, "Publising Files");
                var xml = new StringBuilder();
                xml.Append("<importexportxml><webresources>");
                foreach (var record in loadResponse.Updated)
                    xml.Append("<webresource>" + record.Id + "</webresource>");
                xml.Append("</webresources></importexportxml>");
                Service.Publish(xml.ToString());
            }

            //add plugin assembly to the solution
            var componentType = OptionSets.SolutionComponent.ObjectTypeCode.WebResource;
            var itemsToAdd = loadResponse.Created.Union(loadResponse.Updated);
            VsixUtility.AddSolutionComponents(Service, PackageSettings, componentType, itemsToAdd);

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