#region

using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.ExportXml
{
    public class ExportXmlModule
        : ServiceRequestModule<ExportXmlDialog, ExportXmlService, ExportXmlRequest, ExportXmlResponse, ExportXmlResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}