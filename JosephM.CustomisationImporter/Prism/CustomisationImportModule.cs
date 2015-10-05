#region

using System;
using System.Diagnostics;
using System.IO;
using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Prism.XrmModule.XrmConnection;

#endregion

namespace JosephM.CustomisationImporter.Prism
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class CustomisationImportModule : DialogModule<XrmCustomisationImportDialog>
    {
        protected override string MainOperationName { get { return "Import Customisations"; } }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelp("Import Customisations", "Customisation Importer Help.htm");
            AddOption("Import Customisations Sample", OpenTemplateCommand);
        }

        public void OpenTemplateCommand()
        {
            var templateName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentFiles",
                "Customisations Import Template.xls");
            ApplicationController.StartProcess(templateName);
        }
    }
}