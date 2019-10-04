using Microsoft.Xrm.Sdk.Discovery;

namespace JosephM.Xrm
{
    public class Organisation
    {
        public Organisation(OrganizationDetail organisation)
            : this(organisation.UniqueName, organisation.FriendlyName, organisation.OrganizationVersion, organisation.Endpoints)
        {
        }

        public Organisation(string uniqueName, string friendlyName, string version, EndpointCollection endPoints)
        {
            UniqueName = uniqueName;
            FriendlyName = friendlyName;
            Version = version;
            if (endPoints.ContainsKey(EndpointType.WebApplication))
            {
                WebUrl = endPoints[EndpointType.WebApplication];
                if (WebUrl != null && WebUrl.EndsWith("/"))
                    WebUrl = WebUrl.Substring(0, WebUrl.Length - 1);
            }
        }

        public string UniqueName { get; }

        public string FriendlyName { get; }

        public string Version { get; }

        public string WebUrl { get; }
    }
}
