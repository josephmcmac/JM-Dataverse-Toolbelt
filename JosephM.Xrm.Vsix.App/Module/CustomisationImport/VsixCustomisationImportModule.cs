using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Core.FieldType;
using JosephM.CustomisationImporter;
using JosephM.CustomisationImporter.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.App.Module.CustomisationImport;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.CustomisationImport
{
    [MenuItemVisibleXlsx]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class VsixCustomisationImportModule : CustomisationImportModule
    {
        protected override bool AddGetTemplateLink
        {
            get { return false; }
        }
        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");
            var selectedItems = visualStudioService.GetSelectedFileNamesQualified();
            if (selectedItems.Count() != 1)
            {
                ApplicationController.UserMessage("Only one file may be selected to deploy");
                return;
            }
            var packageSettings = ApplicationController.ResolveType(typeof(XrmPackageSettings)) as XrmPackageSettings;
            if (packageSettings == null)
                throw new NullReferenceException("packageSettings");
            var request = new CustomisationImportRequest()
            {
                ExcelFile = new FileReference(selectedItems.First()),
                AddToSolution = packageSettings.AddToSolution,
                Solution = packageSettings.Solution,
                HideExcelFile = true,
                HideSolutionOptions = true
            };
            //refresh cache in case customisation changes have been made
            var xrmService = ApplicationController.ResolveType(typeof(XrmRecordService)) as XrmRecordService;
            if (xrmService == null)
                throw new NullReferenceException("xrmService");
            xrmService.ClearCache();

            var uri = new UriQuery();
            uri.AddObject(nameof(CustomisationImportDialog.Request), request);
            ApplicationController.NavigateTo(typeof(VsixCustomisationImportDialog), uri);
        }
    }
}
