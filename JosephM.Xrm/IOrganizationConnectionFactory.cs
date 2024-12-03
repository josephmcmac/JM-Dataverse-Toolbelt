namespace JosephM.Xrm
{
    public interface IOrganizationConnectionFactory
    {
        GetOrganisationConnectionResponse GetOrganisationConnection(IXrmConfiguration xrmConfiguration);

        IOrganizationConnectionFactory Clone();
    }
}
