#region

using JosephM.Application.Modules;
using JosephM.CustomisationImporter.Service;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System;
using System.IO;

#endregion

namespace JosephM.CustomisationImporter.Prism
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class CustomisationImportModule
        : ServiceRequestModule<XrmCustomisationImportDialog, XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse, CustomisationImportResponseItem>
    {
        protected override string MainOperationName { get { return "Import Customisations"; } }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Import Customisations", "CustomisationImporter");
            AddOption("Import Customisation Template", OpenTemplateCommand);
        }

        public void OpenTemplateCommand()
        {
            var templateName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentFiles",
                "Customisations Import Template.xlsx");
            ApplicationController.StartProcess(templateName);
        }
    }
}