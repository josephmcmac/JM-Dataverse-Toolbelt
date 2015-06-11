#region

using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Xrm.ImportExporter.Prism
{
    public class XrmSolutionImporterExporterModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<XrmSolutionImporterExporterDialog>();
        }

        public override void InitialiseModule()
        {
            ApplicationOptions.AddOption("Solution Import / Export", MenuNames.Crm, XrmImporterExporterCommand);
            //ApplicationOptions.AddHelp("Data Import / Export", "Xrm Importer Exporter Help.htm");
        }

        private void XrmImporterExporterCommand()
        {
            NavigateTo<XrmSolutionImporterExporterDialog>();
        }
    }
}