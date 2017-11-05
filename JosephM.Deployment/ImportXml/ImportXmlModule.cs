#region

using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.ImportXml
{
    public class ImportXmlModule
        : ServiceRequestModule<ImportXmlDialog, ImportXmlService, ImportXmlRequest, ImportXmlResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}