﻿using JosephM.Application.Modules;
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
    [MyDescription("Import table and field customisations from an Excel file")]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class CustomisationImportModule
        : ServiceRequestModule<CustomisationImportDialog, XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse, CustomisationImportResponseItem>
    {
        public override string MainOperationName { get { return "Import Customisations"; } }

        public override string MenuGroup => "Customisations";

        protected virtual bool AddGetTemplateLink
        {
            get { return true; }
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            if (AddGetTemplateLink)
            {
                this.AddCustomFormFunction(new CustomFormFunction("GETIMPORTTEMPLATE", "Get Import Template", (r) => OpenTemplateCommand(), (r) => true)
                {
                    Description = "Open The Excel Template With Tabs And Columns For Importing Customisations"
                }, typeof(CustomisationImportRequest));
            }
        }

        public void OpenTemplateCommand()
        {
            var templateName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentFiles",
                "Customisations Import Template V2.xlsx");
            ApplicationController.StartProcess(templateName);
        }
    }
}