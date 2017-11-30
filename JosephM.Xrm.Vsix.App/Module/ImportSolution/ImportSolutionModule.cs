using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Application;
using System;
using System.Linq;
using JosephM.Core.FieldType;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    [MenuItemVisibleZip]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class ImportSolutionModule : ServiceRequestModule<ImportSolutionDialog, ImportSolutionService, ImportSolutionRequest, ImportSolutionResponse, ImportSolutionResponseItem>
    {
        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");
            var selectedItems = visualStudioService.GetSelectedFileNamesQualified();
            if (selectedItems.Count() != 1)
            {
                ApplicationController.UserMessage("Only one file may be selected to import");
                return;
            }

            var request = new ImportSolutionRequest()
            {
                SolutionZip = new FileReference(selectedItems.First())
            };
            var uri = new UriQuery();
            uri.AddObject(nameof(ImportSolutionDialog.Request), request);
            ApplicationController.RequestNavigate("Main", typeof(ImportSolutionDialog), uri);
        }
    }
}
