using JosephM.Core.Service;

namespace JosephM.Application.Desktop.Module.Crud.ConfigureAutonumber
{
    public class ConfigureAutonumberResponse : ServiceResponseBase<ConfigureAutonumberResponseItem>
    {
        public bool FormatUpdated { get; set; }
        public bool SeedUpdated { get; set; }
    }
}