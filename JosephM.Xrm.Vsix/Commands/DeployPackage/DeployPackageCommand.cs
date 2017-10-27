using EnvDTE;
using EnvDTE80;
using JosephM.Deployment.DeployPackage;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System;
using System.IO;

namespace JosephM.XRM.VSIX.Commands.DeployPackage
{
    internal sealed class DeployPackageCommand : CommandBase<DeployPackageCommand>
    {
        public override int CommandId
        {
            get { return 0x0110; }
        }

        protected override void menuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            //Display Only If A Solution Folder With A Parent Folder Named "Releases"
            SetHidden();

            var selectedItems = GetSelectedItems();
            var validSelection = false;
            var selectedItemCount = 0;
            foreach (SelectedItem item in selectedItems)
            {
                selectedItemCount++;
                if (item.Project != null && item.Project.Object != null && item.Project.Object is SolutionFolder)
                {
                    var selectedSolutionFolder = item.Project.Object as SolutionFolder;
                    if (selectedSolutionFolder.Parent != null && selectedSolutionFolder.Parent.ParentProjectItem != null)
                    {
                        var containingProject = selectedSolutionFolder.Parent.ParentProjectItem.ContainingProject;
                        validSelection = containingProject?.Name == "Releases";
                    }
                }
            }
            if (validSelection && selectedItemCount == 1)
                SetVisible();
        }

        public override void DoDialog()
        {
            string folder = null;

            var selectedItems = GetSelectedItems();
            foreach (SelectedItem selectedItem in selectedItems)
            {
                var project = selectedItem.Project;
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item.FileCount == 1)
                    {
                        //this strange array index repeated in the get solution items (package settings)
                        var fileName = item.FileNames[1];
                        if (fileName.EndsWith(".zip"))
                        {
                            folder = new FileInfo(fileName).DirectoryName;
                            break;
                        }
                    }
                }
            }
            if (folder == null)
                throw new Exception("Could not find the package directory. There is no zip file in the solution folder");

            var settings = VsixUtility.GetPackageSettings(GetDte2());
            if (settings == null)
                settings = new XrmPackageSettings();

            if (settings.Solution == null)
                throw new NullReferenceException("Solution is not populated in the package settings");

            var xrmRecordService = GetXrmRecordService();
            var visualStudioService = GetVisualStudioService();

            var savedConnection = SavedXrmRecordConfiguration.CreateNew(xrmRecordService.XrmRecordConfiguration);
            var request = DeployPackageRequest.CreateForDeployPackage(folder);
            var controller = CreateDialogController(settings);

            var service = new DeployPackageService(xrmRecordService);
            var dialog = new DeployPackageDialog(service, request, controller, settings, visualStudioService);

            DialogUtility.LoadDialog(dialog);
        }
    }
}
