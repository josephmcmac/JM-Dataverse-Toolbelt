using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace JosephM.Xrm
{
    public class XrmConnection
    {
        private readonly IXrmConfiguration CrmConfig;

        public XrmConnection(IXrmConfiguration crmConfig)
        {
            CrmConfig = crmConfig;
        }

        public GetOrganisationConnectionResponse GetOrganisationConnection()
        {
            try
            {
                if (CrmConfig.ConnectionType != XrmConnectionType.ClientSecret)
                {
                    throw new Exception($"Only {nameof(XrmConnectionType.ClientSecret)} {nameof(IXrmConfiguration.ConnectionType)} is supported");
                }

                var conn = $"AuthType=ClientSecret;url={CrmConfig.WebUrl};ClientId={CrmConfig.ClientId};ClientSecret={CrmConfig.ClientSecret}";
                var client = new CrmServiceClient(conn);
                return new GetOrganisationConnectionResponse(client, new Organisation(client));
            }
            catch (Exception ex)
            {
                throw new Exception("Error connecting to crm instance - check your crm connection details", ex);
            }
        }
    }
}