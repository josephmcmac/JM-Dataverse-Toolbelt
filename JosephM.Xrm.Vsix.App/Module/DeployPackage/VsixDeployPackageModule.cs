using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Core.Utility;
using JosephM.Deployment.DeployPackage;
using JosephM.Xrm.Vsix.Application;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployPackage
{
    [MenuItemVisibleDeployPackage]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class VsixDeployPackageModule : DeployPackageModule
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
            base.RegisterTypes();
        }

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
                    solutionFolderPath = Path.Combine(visualStudioService.SolutionDirectory, parentFolderName, solutionFolder.Name);
                    foreach (var item in solutionFolder.ProjectItems)
                    {
                        if (item.FileName?.EndsWith(".zip") ?? false)
                        {
                            if (item.FileFolder?.Replace("\\\\","\\") != solutionFolderPath?.Replace("\\\\", "\\"))
                            {
                                throw new Exception($"The Zip File In the Package Folder Is In An Unexpected Directory. You Will Need To Move It Into A Matching Folder Path For The Solution Folder. The Expected Path Was {solutionFolderPath}, The Actual path Is {item.FileFolder}");
                            }
                        }
                    }
                    var isDataFolderInVs = false;
                    var dataFolderOnDisk = Path.Combine(solutionFolderPath, "Data");
                    foreach (var item in solutionFolder.SubFolders)
                    {
                        if (item.Name == "Data")
                        {
                            isDataFolderInVs = true;
                            var filesInSolutionFolder = item
                                .ProjectItems
                                .Select(pi => pi.FileName?.Replace("\\\\", "\\"))
                                .OrderBy(s => s)
                                .ToArray();
                            var filesOnDisk = Directory.Exists(dataFolderOnDisk)
                                ? FileUtility.GetFiles(dataFolderOnDisk)
                                .Select(s => s?.Replace("\\\\", "\\"))
                                .OrderBy(s => s)
                                .ToArray()
                                : new string[0];
                            var itemsOnDiskNotInVs = filesOnDisk.Except(filesInSolutionFolder).ToArray();
                            if(itemsOnDiskNotInVs.Any())
                            {
                                throw new Exception($"At Least One Data File On Disk Is Not In The Visual Studio Solution Folder. For The Deployment Process To Run All Files In The Matching Directory On Disk Must Be Added In The Visual Studio Solution Folder. The First Unexpected Item On Disk Is {itemsOnDiskNotInVs.First()}");
                            }
                            var itemsInVsNotOnDisk = filesInSolutionFolder.Except(filesOnDisk).ToArray();
                            if (itemsInVsNotOnDisk.Any())
                            {
                                throw new Exception($"At Least One Of The Data Files In The Visual Studio Folder Is Not In The Correct Folder On Disk. For The Deployment Process To Run All Items In The Deployment Must Be In A Matching Directory For The Visual Studio Folder. The First Unexpected Item Is Located At {itemsInVsNotOnDisk.First()}");
                            }
                        }
                    }
                    if (!isDataFolderInVs
                        && Directory.Exists(dataFolderOnDisk)
                        && FileUtility.GetFiles(dataFolderOnDisk).Any())
                    {
                        throw new Exception($"There Is A Data Folder On Disk With Items Within The Solution Package Directory, But Those Items Are Not Added In The Visual Studio Release Folder. For The Deployment Process To Run All Items On Disk Within The Folder Must Be In Added In The Visual Studio Folder. The First Unexpected Item Is Located At {FileUtility.GetFiles(dataFolderOnDisk).First()}");
                    }
                }
            }
            if (solutionFolderPath == null)
                throw new Exception("Could not find the package directory");

            var request = DeployPackageRequest.CreateForDeployPackage(solutionFolderPath);

            var uri = new UriQuery();
            uri.AddObject(nameof(DeployPackageDialog.Request), request);
            ApplicationController.NavigateTo(typeof(DeployPackageDialog), uri);
        }
    }
}
