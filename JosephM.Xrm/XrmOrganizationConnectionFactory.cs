using System;

namespace JosephM.Xrm
{
    public class XrmOrganizationConnectionFactory : IOrganizationConnectionFactory
    {
        public virtual GetOrganisationConnectionResponse GetOrganisationConnection(IXrmConfiguration xrmConfiguration)
        {
            if (xrmConfiguration.UseXrmToolingConnector)
                throw new NotSupportedException($"{nameof(xrmConfiguration.UseXrmToolingConnector)} Is Not Supported By This {(nameof(IOrganizationConnectionFactory))}");
            var getConnection = new XrmConnection(xrmConfiguration).GetOrganisationConnection();
            return getConnection;
        }
    }
}
