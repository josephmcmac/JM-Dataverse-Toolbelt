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

        public static string redirectUrl = "app://58145B91-0C36-4500-8554-080854F2AC97";
        public static string clientId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
        private static Version _ADALAsmVersion;

        public IEnumerable<Organisation> GetActiveOrganisations()
        {
            //new global discovery service
            var GlobalDiscoUrl = "https://globaldisco.crm.dynamics.com/";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken(CrmConfig.ClientId, CrmConfig.ClientSecret,
                new Uri("https://disco.crm.dynamics.com/api/discovery/")));
            client.Timeout = new TimeSpan(0, 2, 0);
            client.BaseAddress = new Uri(GlobalDiscoUrl);

            var response =
                client.GetAsync("api/discovery/v2.0/Instances", HttpCompletionOption.ResponseHeadersRead).Result;

            if (response.IsSuccessStatusCode)
            {
                //Get the response content and parse it.
                var result = response.Content.ReadAsStringAsync().Result;
                var body = JObject.Parse(result);
                var values = (JArray)body.GetValue("value");

                var instances = !values.HasValues
                    ? new List<OrganisationInstance>()
                    : JsonConvert.DeserializeObject<List<OrganisationInstance>>(values.ToString());

                return instances
                    .Select(i => new Organisation(i.UniqueName, i.FriendlyName, i.Version, i.Url))
                    .ToArray();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public static string GetAccessToken(string clientId, string clientSecret, Uri serviceRoot)
        {
            var targetServiceUrl = GetUriBuilderWithVersion(serviceRoot);
            // Obtain the Azure Active Directory Authentication Library (ADAL) authentication context.
            AuthenticationParameters ap = GetAuthorityFromTargetService(targetServiceUrl.Uri);
            AuthenticationContext authContext = new AuthenticationContext(ap.Authority, false);
            //Note that an Azure AD access token has finite lifetime, default expiration is 60 minutes.
            AuthenticationResult authResult;

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new Exception("Client Id or Secret not supplied");
            }
            var cred = new ClientCredential(clientId, clientSecret);
            authResult = authContext.AcquireTokenAsync(ap.Resource, cred).Result;

            return authResult.AccessToken;
        }

        private static AuthenticationParameters GetAuthorityFromTargetService(Uri targetServiceUrl)
        {
            try
            {
                // if using ADAL > 4.x  return.. // else remove oauth2/authorize from the authority
                if (_ADALAsmVersion == null)
                {
                    // initial setup to get the ADAL version 
                    var AdalAsm = System.Reflection.Assembly.GetAssembly(typeof(IPlatformParameters));
                    if (AdalAsm != null)
                        _ADALAsmVersion = AdalAsm.GetName().Version;
                }

                AuthenticationParameters foundAuthority;
                if (_ADALAsmVersion != null && _ADALAsmVersion >= Version.Parse("5.0.0.0"))
                {
                    foundAuthority = CreateFromUrlAsync(targetServiceUrl);
                }
                else
                {
                    foundAuthority = CreateFromResourceUrlAsync(targetServiceUrl);
                }

                if (_ADALAsmVersion != null && _ADALAsmVersion > Version.Parse("4.0.0.0"))
                {
                    foundAuthority.Authority = foundAuthority.Authority.Replace("oauth2/authorize", "");
                }

                return foundAuthority;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private static AuthenticationParameters CreateFromUrlAsync(Uri targetServiceUrl)
        {
            var result = (Task<AuthenticationParameters>)typeof(AuthenticationParameters)
                .GetMethod("CreateFromUrlAsync").Invoke(null, new[] { targetServiceUrl });

            return result.Result;
        }

        private static AuthenticationParameters CreateFromResourceUrlAsync(Uri targetServiceUrl)
        {
            var result = (Task<AuthenticationParameters>)typeof(AuthenticationParameters)
                .GetMethod("CreateFromResourceUrlAsync").Invoke(null, new[] { targetServiceUrl });
            return result.Result;
        }

        private static UriBuilder GetUriBuilderWithVersion(Uri discoveryServiceUri)
        {
            UriBuilder webUrlBuilder = new UriBuilder(discoveryServiceUri);
            string webPath = "web";

            if (!discoveryServiceUri.AbsolutePath.EndsWith(webPath))
            {
                if (discoveryServiceUri.AbsolutePath.EndsWith("/"))
                    webUrlBuilder.Path = string.Concat(webUrlBuilder.Path, webPath);
                else
                    webUrlBuilder.Path = string.Concat(webUrlBuilder.Path, "/", webPath);
            }

            UriBuilder versionTaggedUriBuilder = new UriBuilder(webUrlBuilder.Uri);
            return versionTaggedUriBuilder;
        }

        public GetOrganisationConnectionResponse GetOrganisationConnection()
        {
            try
            {
                if (CrmConfig.ConnectionType != XrmConnectionType.ClientSecret)
                {
                    throw new Exception($"Only {nameof(XrmConnectionType.ClientSecret)} {nameof(IXrmConfiguration.ConnectionType)} is supported");
                }

                //string conn = $@"
                //    Url = {FindOrganisation().WebUrl};
                //    AuthType = OAuth;
                //    UserName = {CrmConfig.Username};
                //    Password = {CrmConfig.Password};
                //    AppId = 51f81489-12ee-4a9e-aaae-a2591f45987d;
                //    RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;
                //    LoginPrompt=Auto;
                //    RequireNewInstance = True";

                var conn = $"AuthType=ClientSecret;url={CrmConfig.WebUrl};ClientId={CrmConfig.ClientId};ClientSecret={CrmConfig.ClientSecret}";

                var client = new CrmServiceClient(conn);
                return new GetOrganisationConnectionResponse(client, new Organisation(client));
            }
            catch (Exception ex)
            {
                throw new Exception("Error connecting to crm instance - check your crm connection details", ex);
            }
        }

        private TProxy GetProxy<TService, TProxy>(IServiceManagement<TService> serviceManagement,
            AuthenticationCredentials authCredentials) where TService : class
            where TProxy : ServiceProxy<TService>
        {
            var classType = typeof (TProxy);

            if (serviceManagement.AuthenticationType !=
                AuthenticationProviderType.ActiveDirectory)
            {
                var tokenCredentials =
                    serviceManagement.Authenticate(authCredentials);
                // Obtain discovery/organization service proxy for Federated, LiveId and OnlineFederated environments. 
                // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and SecurityTokenResponse.

                if (tokenCredentials.SecurityTokenResponse == null)
                    throw new NullReferenceException("There was an error creating the service connection as the SecurityTokenResponse is null. Check you have the correct authentication type and connection details");

                return (TProxy) classType
                    .GetConstructor(new[]
                    {
                        typeof (IServiceManagement<TService>),
                        typeof (SecurityTokenResponse)
                    })
                    .Invoke(new object[] {serviceManagement, tokenCredentials.SecurityTokenResponse});
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and ClientCredentials.
            return (TProxy) classType
                .GetConstructor(new[]
                {typeof (IServiceManagement<TService>), typeof (ClientCredentials)})
                .Invoke(new object[] {serviceManagement, authCredentials.ClientCredentials});
        }
        private class OrganisationInstance
        {
            public string Id { get; set; }
            public string UniqueName { get; set; }
            public string UrlName { get; set; }
            public string FriendlyName { get; set; }
            public int State { get; set; }
            public string Version { get; set; }
            public string Url { get; set; }
            public string ApiUrl { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}