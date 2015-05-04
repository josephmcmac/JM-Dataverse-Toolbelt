using Microsoft.Xrm.Sdk.Client;

namespace JosephM.Xrm.Test
{
    internal class TestXrmConfiguration : IXrmConfiguration
    {
        #region IXrmConfiguration Members

        public AuthenticationProviderType AuthenticationProviderType
        {
            get { return TestCrmConfigurationService.CrmEndpointType; }
        }

        public string DiscoveryServiceAddress
        {
            get { return TestCrmConfigurationService.CrmDiscoveryServiceAddress; }
        }

        public string OrganizationUniqueName
        {
            get { return TestCrmConfigurationService.CrmOrganizationUniqueName; }
        }

        public string Domain
        {
            get { return TestCrmConfigurationService.CrmDomain; }
        }

        public string Username
        {
            get { return TestCrmConfigurationService.CrmUserName; }
        }

        public string Password
        {
            get { return TestCrmConfigurationService.CrmPassword; }
        }

        #endregion
    }
}