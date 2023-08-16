using JosephM.Core.Attributes;
using JosephM.Xrm.MetadataImportExport;

namespace JosephM.SolutionComponentExporter.Type
{
    public class FormOrDashboardExport
    {
        public FormOrDashboardExport(string formType, string entityType, string name, string decription, string state)
        {
            FormType = formType;
            EntityType = entityType;
            Name = name;
            Decription = decription;
            State = state;
        }

        public string FormType { get; set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
        public string State { get; set; }
    }
}