using Microsoft.Xrm.Sdk.Discovery;

namespace JosephM.Xrm
{
    public class Organisation
    {
        private OrganizationDetail _organisation;

        public Organisation(OrganizationDetail organisation)
        {
            _organisation = organisation;
        }

        public string UniqueName => _organisation.UniqueName;

        public string FriendlyName => _organisation.FriendlyName;
    }
}
