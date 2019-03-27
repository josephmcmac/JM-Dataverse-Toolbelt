using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Deployment.ExportXml
{
    public class ExportXmlResponse : ServiceResponseBase<ExportXmlResponseItem>
    {
        [Hidden]
        public string Folder { get; internal set; }
    }
}