using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Service;
using JosephM.Deployment;
using JosephM.Deployment.CreatePackage;
using JosephM.XrmModule.XrmConnection;
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
    }
}
