using JosephM.Core.Extentions;
using System.Xml;

namespace JosephM.Deployment.DeployPackage
{
    public class SolutionImportResult
    {
        public SolutionImportResult(XmlNode xmlNode)
        {
            XmlNode = xmlNode;
        }

        private XmlNode XmlNode { get; }

        public bool IsSuccess
        {
            get
            {
                return XmlNode.Attributes["result"]?.Value == "success";
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
    }
}
