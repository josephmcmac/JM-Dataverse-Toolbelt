using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Description;

namespace JosephM.Xrm
{
    public class XrmConnection
    {
        private readonly IXrmConfiguration CrmConfig;

        public XrmConnection(IXrmConfiguration crmConfig)
        {
            CrmConfig = crmConfig;
        }

        public IEnumerable<Organisation> GetActiveOrganisations()
        {
            var organisations = new List<Organisation>();

            using (var discoveryProxy = GetDiscoveryService())
            {
                if (discoveryProxy != null)
                {
                    var orgs = DiscoverOrganizations(discoveryProxy);
                    foreach(var org in orgs)
                    {
                        if(org.State == OrganizationState.Enabled)
                        {
                            organisations.Add(new Organisation(org));
                        }
                    }
                }
            }

            return organisations;
        }

        /// <summary>
        ///     Return Organisation Service Proxy
        /// </summary>
        /// <returns></returns>
        public OrganizationServiceProxy GetOrgServiceProxy()
        {
            try
            {
                OrganizationServiceProxy organizationProxy = null;


                var endPointType = EndpointType.OrganizationService;
                var organizationUri = GetEndpointUrl(endPointType);

                if (!String.IsNullOrWhiteSpace(organizationUri))
                {
                    //test branch
                    //noted this code throws a WCF nametable quote exceeded error
                    //when connecting to CRM 2016 with old SDK assembly versions
                    //if this occurs the assemblies will require updating to v8.1+
                    //note this may cause regression for old CRM versions due to new request properties
                    //e.g. the HasFeedback property on the CreateEntityRequest
                    var orgServiceManagement =
                        ServiceConfigurationFactory.CreateManagement<IOrganizationService>(
                            new Uri(organizationUri));

                    // Set the credentials.
                    var credentials = GetCredentials(CrmConfig.AuthenticationProviderType);

                    organizationProxy = GetProxy<IOrganizationService, OrganizationServiceProxy>(orgServiceManagement,
                        credentials);
                }
                return organizationProxy;
            }
            catch (Exception ex)
            {
                throw new Exception("Error connecting to crm instance - check your crm connection details", ex);
            }
        }

        /// <summary>
        ///     Return Organisation Service Proxy
        /// </summary>
        /// <returns></returns>
        public string GetWebUrl()
        {
            try
            {
                var endPointType = EndpointType.WebApplication;
                return GetEndpointUrl(endPointType);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting web url - check your crm connection details", ex);
            }
        }

        private string GetEndpointUrl(EndpointType endPointType)
        {
            string result = null;

            // Set the credentials.
            var authCredentials = GetCredentials(CrmConfig.AuthenticationProviderType);

            var serviceManagement =
                ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                    new Uri(CrmConfig.DiscoveryServiceAddress));

            // Get the discovery service proxy.
            using (var discoveryProxy =
                GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials))
            {
                // Obtain organization information from the Discovery service. 
                if (discoveryProxy != null)
                {
                    // Obtain information about the organizations that the system user belongs to.
                    var orgs = DiscoverOrganizations(discoveryProxy);
                    // Obtains the Web address (Uri) of the target organization.
                    result = FindOrganization(CrmConfig.OrganizationUniqueName,
                        orgs.ToArray()).Endpoints[endPointType];
                }
            }

            return result;
        }

        public DiscoveryServiceProxy GetDiscoveryService()
        {
            var authenticationType = CrmConfig.AuthenticationProviderType;

            var serviceManagement =
                ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                    new Uri(CrmConfig.DiscoveryServiceAddress));

            // Set the credentials.
            var authCredentials = GetCredentials(authenticationType);

            return GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials);
        }

        /// <summary>
        ///     Obtain the AuthenticationCredentials based on AuthenticationProviderType.
        /// </summary>
        /// <param name="endpointType">An AuthenticationProviderType of the CRM environment.</param>
        /// <returns>Get filled credentials.</returns>
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

        /// <summary>
        ///     Discovers the organizations that the calling user belongs to.
        /// </summary>
        /// <param name="service">A Discovery service proxy instance.</param>
        /// <returns>
        ///     Array containing detailed information on each organization that
        ///     the user belongs to.
        /// </returns>
        public OrganizationDetailCollection DiscoverOrganizations(IDiscoveryService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            var orgRequest = new RetrieveOrganizationsRequest();
            var orgResponse =
                (RetrieveOrganizationsResponse) service.Execute(orgRequest);
            
            return orgResponse.Details;
        }

        /// <summary>
        ///     Finds a specific organization detail in the array of organization details
        ///     returned from the Discovery service.
        /// </summary>
        /// <param name="orgUniqueName">The unique name of the organization to find.</param>
        /// <param name="orgDetails">Array of organization detail object returned from the discovery service.</param>
        /// <returns>Organization details or null if the organization was not found.</returns>
        public OrganizationDetail FindOrganization(string orgUniqueName, OrganizationDetail[] orgDetails)
        {
            if (String.IsNullOrWhiteSpace(orgUniqueName))
                throw new ArgumentNullException("orgUniqueName");
            if (orgDetails == null)
                throw new ArgumentNullException("orgDetails");
            OrganizationDetail orgDetail = null;

            foreach (var detail in orgDetails)
            {
                if (String.Compare(detail.UniqueName, orgUniqueName,
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    orgDetail = detail;
                    break;
                }
            }
            if(orgDetail == null)
                throw new NullReferenceException("No Organisation Returned By The Discovery Service Matched The UniqueName Of " + orgUniqueName);
            return orgDetail;
        }

        /// <summary>
        ///     Generic method to obtain discovery/organization service proxy instance.
        /// </summary>
        /// <typeparam name="TService">
        ///     Set IDiscoveryService or IOrganizationService type to request respective service proxy instance.
        /// </typeparam>
        /// <typeparam name="TProxy">
        ///     Set the return type to either DiscoveryServiceProxy or OrganizationServiceProxy type based on TService type.
        /// </typeparam>
        /// <param name="serviceManagement">An instance of IServiceManagement</param>
        /// <param name="authCredentials">The user's Microsoft Dynamics CRM logon credentials.</param>
        /// <returns></returns>
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
    }
}