using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportXml;
using JosephM.Xrm.Vsix.Application;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.IO;

namespace JosephM.Xrm.Vsix.Module.AddReleaseData
{
    [MenuItemVisibleAddData]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class AddReleaseDataModule : ImportXmlModule
    {
        public override void DialogCommand()
        {
            string solutionFolderPath = null;

            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var selectedItems = visualStudioService.GetSelectedItems();

            foreach (var selectedItem in selectedItems)
            {
                var solutionFolder = selectedItem as ISolutionFolder;
                if (solutionFolder != null)
                {
                    var parentFolderName = solutionFolder.ParentProjectName;
                    solutionFolderPath = Path.Combine(visualStudioService.SolutionDirectory, parentFolderName, solutionFolder.Name, "Data");
                }
            }
            if (solutionFolderPath == null)
                throw new Exception("Could not find the package directory");

            var request = ExportXmlRequest.CreateForAddData(solutionFolderPath);

            var uri = new UriQuery();
            uri.AddObject(nameof(AddReleaseDataDialog.Request), request);
            ApplicationController.NavigateTo(typeof(AddReleaseDataDialog), uri);
        }
    }
}
