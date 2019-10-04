using Microsoft.Xrm.Sdk;

namespace JosephM.Xrm
{
    public class GetOrganisationConnectionResponse
    {
        public GetOrganisationConnectionResponse(IOrganizationService service, Organisation organisation)
        {
            Organisation = organisation;
            Service = service;
        }

        public Organisation Organisation { get; private set; }
        public IOrganizationService Service { get; private set; }
    }
}