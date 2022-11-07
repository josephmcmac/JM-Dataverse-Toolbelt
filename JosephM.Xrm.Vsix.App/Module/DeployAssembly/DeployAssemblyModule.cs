using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Service;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [MenuItemVisibleForPluginProject]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class DeployAssemblyModule : ServiceRequestModule<DeployAssemblyDialog, DeployAssemblyService, DeployAssemblyRequest, DeployAssemblyResponse, DeployAssemblyResponseItem>
    {
        public override string MenuGroup => "Plugins";
    }
}