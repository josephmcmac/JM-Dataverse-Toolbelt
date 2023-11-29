using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Tooling.Connector;

namespace JosephM.Xrm
{
    public class Organisation
    {
        public Organisation(CrmServiceClient serviceClient)
    :       this(serviceClient.ConnectedOrgUniqueName, serviceClient.ConnectedOrgFriendlyName, serviceClient.ConnectedOrgVersion?.ToString(), serviceClient.EnvironmentId, serviceClient.ConnectedOrgPublishedEndpoints)
        {
        }

        private Organisation(string uniqueName, string friendlyName, string version, string environmentId, EndpointCollection endPoints)
        {
            UniqueName = uniqueName;
            FriendlyName = friendlyName;
            Version = version;
            EnvironmentId = environmentId;
            if (endPoints.ContainsKey(EndpointType.WebApplication))
            {
                WebUrl = endPoints[EndpointType.WebApplication];
                if (WebUrl != null && WebUrl.EndsWith("/"))
                    WebUrl = WebUrl.Substring(0, WebUrl.Length - 1);
            }
            if (endPoints.ContainsKey(EndpointType.OrganizationService))
            {
                OrganisationServiceUri = endPoints[EndpointType.OrganizationService];
                if (OrganisationServiceUri != null && OrganisationServiceUri.EndsWith("/"))
                    OrganisationServiceUri = OrganisationServiceUri.Substring(0, OrganisationServiceUri.Length - 1);
            }
        }


        public string UniqueName { get; }

        public string FriendlyName { get; }

        public string Version { get; }

        public string WebUrl { get; }

        public string OrganisationServiceUri { get; }

        public string EnvironmentId { get; }
    }
}
