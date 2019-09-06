using Microsoft.Xrm.Sdk.Discovery;

namespace JosephM.Xrm
{
    public class Organisation
    {
        private OrganizationDetail _organisation;

        public Organisation(OrganizationDetail organisation)
        {
            _organisation = organisation;
        }

        public string UniqueName => _organisation.UniqueName;

        public string FriendlyName => _organisation.FriendlyName;

        public string Version => _organisation.OrganizationVersion;

        public string WebUrl
        {
            get
            {
                var webUrl = _organisation.Endpoints[EndpointType.WebApplication];
                if (webUrl != null && webUrl.EndsWith("/"))
                    webUrl = webUrl.Substring(0, webUrl.Length - 1);
                return webUrl;
            }
        }
    }
}
