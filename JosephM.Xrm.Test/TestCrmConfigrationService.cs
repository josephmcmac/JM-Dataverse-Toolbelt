using Microsoft.Xrm.Sdk.Client;

namespace JosephM.Xrm.Test
{
    public class TestCrmConfigurationService
    {
        public const string SqlServerName = "DEMOSERVER";
        public const string SqlServerDatabaseName = "Test Database";
        public const string CrmDatabase = "TEST_MSCRM";

        public static AuthenticationProviderType CrmEndpointType
        {
            get { return AuthenticationProviderType.Federation; }
        }

        public static string CrmDiscoveryServiceAddress
        {
            get { return "https://dontconnect/XRMServices/2011/Discovery.svc"; }
        }

        public static string CrmOrganizationUniqueName
        {
            get { return ""; }
        }

        public static string CrmDomain
        {
            get { return ""; }
        }

        public static string CrmUserName
        {
            get { return ""; }
        }

        public static string CrmPassword
        {
            get { return ""; }
        }
    }
}