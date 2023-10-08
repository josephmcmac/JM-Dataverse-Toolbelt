using System;

namespace JosephM.Xrm
{
    public class XrmOrganizationConnectionFactory : IOrganizationConnectionFactory
    {
        public virtual GetOrganisationConnectionResponse GetOrganisationConnection(IXrmConfiguration xrmConfiguration)
        {
            if (xrmConfiguration.ConnectionType != XrmConnectionType.ClientSecret)
            {
                throw new NotSupportedException($"Only {nameof(XrmConnectionType.ClientSecret)} connection type is supported by this {(nameof(IOrganizationConnectionFactory))}");
            }
            var getConnection = new XrmConnection(xrmConfiguration).GetOrganisationConnection();
            return getConnection;
        }
    }
}
