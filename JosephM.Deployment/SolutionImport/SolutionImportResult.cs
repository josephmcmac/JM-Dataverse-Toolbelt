using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System.Linq;
using System.Xml;

namespace JosephM.Deployment.SolutionImport
{
    public class SolutionImportResult
    {
        public SolutionImportResult(XmlNode xmlNode, XrmRecordService xrmRecordService)
        {
            XmlNode = xmlNode;
            XrmRecordService = xrmRecordService;
        }

        private XmlNode XmlNode { get; }
        public XrmRecordService XrmRecordService { get; }

        public bool IsSuccess
        {
            get
            {
                return XmlNode.Attributes["result"]?.Value == "success";
            }
        }

        public Url GetUrl()
        {
            var validUrlType = new[] { Entities.workflow };
            if (TargetId != null && TargetType != null && validUrlType.Contains(TargetType))
                return new Url(XrmRecordService.GetWebUrl(TargetType, TargetId), "Open");
            return null;
        }

        private string TargetType
        {
            get
            {
                if (ParentNodeName == "publishworkflows")
                    return Entities.workflow;
                return null;
            }
        }

        private string TargetId
        {
            get
            {
                var node = XmlNode.ParentNode?.Attributes["id"];
                return node?.Value;
            }
        }

        public string Result
        {
            get
            {
                var node = XmlNode.Attributes["result"];
                return node?.Value?.ToTitleCase();
            }
        }

        public string ErrorCode
        {
            get
            {
                var node = XmlNode.Attributes["errorcode"];
                return node?.Value;
            }
        }

        public string ErrorText
        {
            get
            {
                var node = XmlNode.Attributes["errortext"];
                return node?.Value;
            }
        }

        public string Type
        {
            get
            {
                var type = XmlNode.ParentNode?.ParentNode?.Name;
                if (type == null)
                    return null;
                if (type.StartsWith("publish"))
                    type = type.Insert(7, " ");
                return type.ToTitleCase();
            }
        }

        public string Name
        {
            get
            {
                var node = XmlNode.ParentNode?.Attributes["name"];
                return node?.Value;
            }
        }

        private string ParentNodeName
        {
            get
            {
                return XmlNode.ParentNode?.ParentNode?.Name;
            }
        }
    }
}
