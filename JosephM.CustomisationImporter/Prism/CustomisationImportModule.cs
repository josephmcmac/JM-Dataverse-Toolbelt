#region

using JosephM.Application.Modules;
using JosephM.Core.Attributes;
using JosephM.CustomisationImporter.Service;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System;
using System.IO;

#endregion

namespace JosephM.CustomisationImporter.Prism
{
    [MyDescription("Import Customisations Defined In An Excel Spreadsheet Into A CRM Instance (Create or Update)")]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class CustomisationImportModule
        : ServiceRequestModule<XrmCustomisationImportDialog, XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse, CustomisationImportResponseItem>
    {
        public override string MainOperationName { get { return "Import"; } }

        public override string MenuGroup => "Customisations";

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Import Customisations", "CustomisationImporter");
            AddOption(MenuGroup, "Open Template", OpenTemplateCommand, "Get An Excel File With The Tabs And Columns For Importing Customisations");
        }

        public void OpenTemplateCommand()
        {
            var templateName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentFiles",
                "Customisations Import Template.xlsx");
            ApplicationController.StartProcess(templateName);
        }
    }
}