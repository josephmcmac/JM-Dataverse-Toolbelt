using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.WebServiceClient;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace $safeprojectname$.Xrm
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

        private IDiscoveryService GetDiscoveryService()
        {
            var authenticationType = CrmConfig.AuthenticationProviderType;

            if (authenticationType == AuthenticationProviderType.LiveId)
            {
                string discoveryUrl = "https://disco.crm.dynamics.com";// CrmConfig.DiscoveryServiceAddress;
                Uri discoveryUri = new Uri(CrmConfig.DiscoveryServiceAddress + "/web");

                string authority = @"https://login.microsoftonline.com/common";
                AuthenticationContext authContext = new AuthenticationContext(authority, false);
                string username = CrmConfig.Username;
                string password = CrmConfig.Password;

                AuthenticationResult authResult = null;
                if (username != string.Empty && password != string.Empty)
                {
                    var cred = new UserPasswordCredential(username, password);
                    authResult = authContext.AcquireTokenAsync(discoveryUrl, clientId, cred).Result;
                }

                var svc = new DiscoveryWebProxyClient(discoveryUri);
                svc.HeaderToken = authResult.AccessToken;
                return svc;
            }
            else
            {
                var serviceManagement =
                    ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                        new Uri(CrmConfig.DiscoveryServiceAddress));
                var authCredentials = GetCredentials(authenticationType);
                return GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials);
            }
        }

        public IEnumerable<Organisation> GetActiveOrganisations()
        {
            if (CrmConfig.AuthenticationProviderType != AuthenticationProviderType.LiveId)
            {
                //old online and on premise
                var serviceManagement =
                    ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                        new Uri(CrmConfig.DiscoveryServiceAddress));
                var authCredentials = GetCredentials(CrmConfig.AuthenticationProviderType);
                using (var discoveryService = GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials))
                {
                    var orgResponse = (RetrieveOrganizationsResponse)discoveryService.Execute(new RetrieveOrganizationsRequest());
                    return orgResponse.Details
                        .Where(o => o.State == OrganizationState.Enabled)
                        .Select(d => new Organisation(d))
                        .ToArray();
                }
            }
            else
            {
                //new global discovery service
                var GlobalDiscoUrl = "https://globaldisco.crm.dynamics.com/";
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", GetAccessToken(CrmConfig.Username, CrmConfig.Password,
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
        }

        public static string GetAccessToken(string userName, string password, Uri serviceRoot)
        {
            var targetServiceUrl = GetUriBuilderWithVersion(serviceRoot);
            // Obtain the Azure Active Directory Authentication Library (ADAL) authentication context.
            AuthenticationParameters ap = GetAuthorityFromTargetService(targetServiceUrl.Uri);
            AuthenticationContext authContext = new AuthenticationContext(ap.Authority, false);
            //Note that an Azure AD access token has finite lifetime, default expiration is 60 minutes.
            AuthenticationResult authResult;

            if (userName != string.Empty && password != string.Empty)
            {

                UserPasswordCredential cred = new UserPasswordCredential(userName, password);
                authResult = authContext.AcquireTokenAsync(ap.Resource, clientId, cred).Result;
            }
            else
            {
                // Note that PromptBehavior.Always is why the user is aways prompted when this path is executed.
                // Look up PromptBehavior to understand what other options exist. 
                PlatformParameters platformParameters = new PlatformParameters(PromptBehavior.Always);
                authResult = authContext.AcquireTokenAsync(ap.Resource, clientId, new Uri(redirectUrl), platformParameters).Result;
            }

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

        public static IOrganizationService GetOrgServiceProxy(IXrmConfiguration configuration)
        {
            return new XrmConnection(configuration).GetOrgServiceProxy();
        }

        public IOrganizationService GetOrgServiceProxy()
        {
            try
            {
                if (CrmConfig.AuthenticationProviderType == AuthenticationProviderType.LiveId)
                {
                    string conn = $@"
                            Url = {FindOrganisation().WebUrl};
                            AuthType = OAuth;
                            UserName = {CrmConfig.Username};
                            Password = {CrmConfig.Password};
                            AppId = 51f81489-12ee-4a9e-aaae-a2591f45987d;
                            RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;
                            LoginPrompt=Auto;
                            RequireNewInstance = True";

                    var client = new CrmServiceClient(conn);
                    return client;
                }
                else
                {
                    var organisation = FindOrganisation();
                    var organisationServiceUri = organisation.OrganisationServiceUri;

                    if (string.IsNullOrWhiteSpace(organisationServiceUri))
                        throw new NullReferenceException("organizationUri");
                    //noted this code throws a WCF nametable quote exceeded error
                    //when connecting to CRM 2016 with old SDK assembly versions
                    //if this occurs the assemblies will require updating to v8.1+
                    //note this may cause regression for old CRM versions due to new request properties
                    //e.g. the HasFeedback property on the CreateEntityRequest
                    var orgServiceManagement =
                    ServiceConfigurationFactory.CreateManagement<IOrganizationService>(
                        new Uri(organisationServiceUri));

                    // Set the credentials.
                    var credentials = GetCredentials(CrmConfig.AuthenticationProviderType);

                    var organizationProxy = GetProxy<IOrganizationService, OrganizationServiceProxy>(orgServiceManagement,
                        credentials);

                    return organizationProxy;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error connecting to crm instance - check your crm connection details", ex);
            }
        }

        private AuthenticationCredentials GetCredentials(AuthenticationProviderType endpointType)
        {
            var authCredentials = new AuthenticationCredentials();
            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    authCredentials.ClientCredentials.Windows.ClientCredential =
                        new NetworkCredential(CrmConfig.Username,
                            CrmConfig.Password,
                            CrmConfig.Domain);
                    break;
                case AuthenticationProviderType.LiveId:
                    authCredentials.ClientCredentials.UserName.UserName = CrmConfig.Username;
                    authCredentials.ClientCredentials.UserName.Password = CrmConfig.Password;
                    authCredentials.SupportingCredentials = new AuthenticationCredentials();
                    authCredentials.SupportingCredentials.ClientCredentials =
                        DeviceIdManager.LoadOrRegisterDevice();
                    break;
                case AuthenticationProviderType.None:
                    authCredentials.ClientCredentials = new ClientCredentials();
                    authCredentials.ClientCredentials.Windows.ClientCredential =
                        CredentialCache.DefaultNetworkCredentials;
                    break;
                default: // For Federated and OnlineFederated environments.                    
                    authCredentials.ClientCredentials.UserName.UserName = CrmConfig.Username;
                    authCredentials.ClientCredentials.UserName.Password = CrmConfig.Password;
                    // For OnlineFederated single-sign on, you could just use current UserPrincipalName instead of passing user name and password.
                    // authCredentials.UserPrincipalName = UserPrincipal.Current.UserPrincipalName;  //Windows/Kerberos
                    break;
            }

            return authCredentials;
        }

        public Organisation FindOrganisation()
        {
            var organisations = GetActiveOrganisations();

            if (string.IsNullOrWhiteSpace(CrmConfig.OrganizationUniqueName))
            {
                throw new ArgumentNullException(nameof(IXrmConfiguration.OrganizationUniqueName));
            }

            foreach (var organisation in organisations)
            {
                if (string.Compare(organisation.UniqueName, CrmConfig.OrganizationUniqueName,
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return organisation;
                }
            }
            throw new NullReferenceException($"No organisation found with name {CrmConfig.OrganizationUniqueName}");
        }


        private TProxy GetProxy<TService, TProxy>(IServiceManagement<TService> serviceManagement,
            AuthenticationCredentials authCredentials) where TService : class
            where TProxy : ServiceProxy<TService>
        {
            var classType = typeof(TProxy);

            if (serviceManagement.AuthenticationType !=
                AuthenticationProviderType.ActiveDirectory)
            {
                var tokenCredentials =
                    serviceManagement.Authenticate(authCredentials);
                // Obtain discovery/organization service proxy for Federated, LiveId and OnlineFederated environments. 
                // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and SecurityTokenResponse.

                if (tokenCredentials.SecurityTokenResponse == null)
                    throw new NullReferenceException("There was an error creating the service connection as the SecurityTokenResponse is null. Check you have the correct authentication type and connection details");

                return (TProxy)classType
                    .GetConstructor(new[]
                    {
                            typeof (IServiceManagement<TService>),
                            typeof (SecurityTokenResponse)
                    })
                    .Invoke(new object[] { serviceManagement, tokenCredentials.SecurityTokenResponse });
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and ClientCredentials.
            return (TProxy)classType
                .GetConstructor(new[]
                {typeof (IServiceManagement<TService>), typeof (ClientCredentials)})
                .Invoke(new object[] { serviceManagement, authCredentials.ClientCredentials });
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

        public class Organisation
        {
            public Organisation(OrganizationDetail organisation)
                : this(organisation.UniqueName, organisation.FriendlyName, organisation.OrganizationVersion, organisation.Endpoints)
            {
            }

            public Organisation(CrmServiceClient serviceClient)
        : this(serviceClient.ConnectedOrgUniqueName, serviceClient.ConnectedOrgFriendlyName, serviceClient.ConnectedOrgVersion?.ToString(), serviceClient.ConnectedOrgPublishedEndpoints)
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
                if (endPoints.ContainsKey(EndpointType.OrganizationService))
                {
                    OrganisationServiceUri = endPoints[EndpointType.OrganizationService];
                    if (OrganisationServiceUri != null && OrganisationServiceUri.EndsWith("/"))
                        OrganisationServiceUri = OrganisationServiceUri.Substring(0, OrganisationServiceUri.Length - 1);
                }
            }

            public Organisation(string uniqueName, string friendlyName, string version, string webUrl)
            {
                UniqueName = uniqueName;
                FriendlyName = friendlyName;
                Version = version;
                WebUrl = webUrl;
            }

            public string UniqueName { get; }

            public string FriendlyName { get; }

            public string Version { get; }

            public string WebUrl { get; }

            public string OrganisationServiceUri { get; }
        }
    }
}