using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;

namespace JosephM.Record.Xrm.XrmRecord
{
    [Group("Hidden_XrmRecordConfiguration", isHiddenSection: true)]
    [ServiceConnection(typeof(XrmRecordService))]
    public class XrmRecordConfiguration : IXrmRecordConfiguration, IValidatableObject
    {
        public XrmRecordConfiguration()
        {
        }

        [RequiredProperty]
        [GridField]
        [GridWidth(300)]
        [EditableFormWidth(300)]
        [MyDescription("Name for your connection")]
        [DisplayOrder(10)]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name ?? WebUrl;
        }

        [RequiredProperty]
        [GridField]
        [GridReadOnly]
        [EditableFormWidth(125)]
        [GridWidth(125)]
        [MyDescription("Connect using an app user client and secret or use an XRM tooling option provided by Microsoft")]
        [DisplayOrder(20)]
        public XrmRecordConfigurationConnectionType? ConnectionType { get; set; }

        [EditableFormWidth(300)]
        [MyDescription("Client Id used to login with client/secret")]
        [DisplayOrder(30)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(ConnectionType), XrmRecordConfigurationConnectionType.ClientSecret)]
        public string ClientId { get; set; }

        [EditableFormWidth(300)]
        [MyDescription("Secret used to login with client/secret")]
        [DisplayOrder(40)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(ConnectionType), XrmRecordConfigurationConnectionType.ClientSecret)]
        public Password ClientSecret { get; set; }

        private string _webUrl;
        [EditableFormWidth(300)]
        [PropertyInContextByPropertyValue(nameof(ConnectionType), XrmRecordConfigurationConnectionType.ClientSecret)]
        [MyDescription("URL used to log into the instance")]
        [DisplayOrder(50)]
        [RequiredProperty]
        public string WebUrl
        {
            get
            {
                return _webUrl;
            }
            set
            {
                _webUrl = value;
                if (!string.IsNullOrWhiteSpace(_webUrl) && !ConnectionType.HasValue)
                {
                    ConnectionType = XrmRecordConfigurationConnectionType.ClientSecret;
                }
            }
        }

        [Group("Hidden_XrmRecordConfiguration")]
        [GridField]
        [GridWidth(300)]
        [DisplayName("Web Url")]
        [DisplayOrder(50)]
        public string WebUrlGridDisplayOnly
        {
            get
            {
                return _webUrl;
            }
        }

        private string _toolingConnectionId;
        [DisplayOrder(30)]
        [PropertyInContextByPropertyValue(nameof(ConnectionType), XrmRecordConfigurationConnectionType.XrmTooling)]
        [ReadOnlyWhenSet]
        public string ToolingConnectionId
        {
            get
            {
                return _toolingConnectionId;
            }
            set
            {
                _toolingConnectionId = value;
                if(!string.IsNullOrWhiteSpace(_toolingConnectionId) && !ConnectionType.HasValue)
                {
                    ConnectionType = XrmRecordConfigurationConnectionType.XrmTooling;
                }
            }
        }

        public IsValidResponse Validate()
        {
            if (ConnectionType == XrmRecordConfigurationConnectionType.XrmTooling)
            {
                return new IsValidResponse();
            }
            return new XrmRecordService(this, new LogController(), null).VerifyConnection();
        }
    }
}