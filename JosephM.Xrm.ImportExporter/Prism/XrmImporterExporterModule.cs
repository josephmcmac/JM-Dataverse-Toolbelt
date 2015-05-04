#region

using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmImporterExporterModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<XrmImporterExporterDialog>();
        }

        public override void InitialiseModule()
        {
            ApplicationOptions.AddOption("Data Import / Export", MenuNames.Crm, XrmImporterExporterCommand);
            ApplicationOptions.AddHelp("Data Import / Export", "Xrm Importer Exporter Help.htm");
        }

        private void XrmImporterExporterCommand()
        {
            NavigateTo<XrmImporterExporterDialog>();
        }
    }
}