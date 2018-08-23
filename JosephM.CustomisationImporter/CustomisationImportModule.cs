using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.CustomisationImporter.Service;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.IO;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;

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
            this.AddCustomFormFunction(new CustomFormFunction("GETIMPORTTEMPLATE", "Get Import Template", (r) => OpenTemplateCommand(), (r) => true)
            {
                Description = "Open The Excel Template With Tabs And Columns For Importing Customisations"
            }, typeof(CustomisationImportRequest));
        }

        public void OpenTemplateCommand()
        {
            var templateName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentFiles",
                "Customisations Import Template.xlsx");
            ApplicationController.StartProcess(templateName);
        }
    }
}