using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Service;
using JosephM.Deployment;
using JosephM.Deployment.CreatePackage;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;

namespace JosephM.Xrm.Vsix.Module.CreatePackage
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class VsixCreatePackageModule
                : ServiceRequestModule<VsixCreatePackageDialog, CreatePackageService, CreatePackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";

        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var packageSettings = ApplicationController.ResolveType(typeof(XrmPackageSettings)) as XrmPackageSettings;
            if (packageSettings == null)
                throw new NullReferenceException("packageSettings");
            if (packageSettings.Solution == null)
                throw new NullReferenceException("Solution is not populated in the package settings");

            //WARNING THIS FOLDER IS CLEARED BEFORE PROCESSING SO CAREFUL IF CHANGE DIRECTORY
            var folderPath = visualStudioService.SolutionDirectory + "/TempSolutionFolder";
            var request = CreatePackageRequest.CreateForCreatePackage(folderPath, packageSettings.Solution);

            var uri = new UriQuery();
            uri.AddObject(nameof(CreatePackageDialog.Request), request);
            ApplicationController.RequestNavigate("Main", typeof(VsixCreatePackageDialog), uri);
        }
    }
}
