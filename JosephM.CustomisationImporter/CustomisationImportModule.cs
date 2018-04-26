using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.CustomisationImporter.Service;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.IO;

namespace JosephM.CustomisationImporter
{
    [MyDescription("Import Customisations Defined In An Excel Spreadsheet Into A CRM Instance (Create or Update)")]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class CustomisationImportModule
        : ServiceRequestModule<CustomisationImportDialog, XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse, CustomisationImportResponseItem>
    {
        public override string MainOperationName { get { return "Import"; } }

        public override string MenuGroup => "Customisations";

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddOption(MenuGroup, "Open Template", OpenTemplateCommand, "Get An Excel File With The Tabs And Columns For Importing Customisations");
        }

        public void OpenTemplateCommand()
        {
            var templateName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentFiles",
                "Customisations Import Template V2.xlsx");
            ApplicationController.StartProcess(templateName);
        }
    }
}