#region

using System;
using System.Diagnostics;
using System.IO;
using JosephM.Prism.Infrastructure.Attributes;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Prism.XrmModule.XrmConnection;

#endregion

namespace JosephM.CustomisationImporter.Prism
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class CustomisationImportModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<XrmCustomisationImportDialog>();
        }

        public override void InitialiseModule()
        {
            ApplicationOptions.AddOption("Import Customisations", MenuNames.Crm, CustomisationImportCommand);
            ApplicationOptions.AddHelp("Import Customisations", "Customisation Importer Help.htm");
            ApplicationOptions.AddOption("Import Customisations Sample", MenuNames.Crm, CustomisationImportOpenTemplateCommand);
        }

        private void CustomisationImportCommand()
        {
            NavigateTo<XrmCustomisationImportDialog>();
        }

        private void CustomisationImportOpenTemplateCommand()
        {
            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentFiles",
                    "Customisations Import Template.xls"));
        }
    }
}