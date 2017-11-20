#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

#endregion

namespace JosephM.Deployment.ImportXml
{
    public class ImportXmlService :
        DataImportServiceBase<ImportXmlRequest, ImportXmlResponse, DataImportResponseItem>
    {
        public ImportXmlService(XrmRecordService xrmRecordService)
            : base(xrmRecordService)
        {
        }

        public override void ExecuteExtention(ImportXmlRequest request, ImportXmlResponse response,
            LogController controller)
        {
            ImportXml(request.Folder.FolderPath, controller, response, request.MaskEmails);
        }


        public void ImportXml(string folder, LogController controller,
            ImportXmlResponse response, bool maskEmails = false)
        {
            controller.UpdateProgress(0, 1, "Loading XML Files");
            var entities = LoadEntitiesFromXmlFiles(folder);
            var importResponses = DoImport(entities, controller, maskEmails);
            foreach (var item in importResponses)
                response.AddResponseItem(item);
        }

        public IEnumerable<Entity> LoadEntitiesFromXmlFiles(string folder)
        {
            var lateBoundSerializer = new DataContractSerializer(typeof(Entity));

            var entities = new List<Entity>();
            foreach (var file in Directory.GetFiles(folder, "*.xml"))
            {
                using (var fileStream = new FileStream(file, FileMode.Open))
                {
                    entities.Add((Entity)lateBoundSerializer.ReadObject(fileStream));
                }
            }
            return entities;
        }
    }
}