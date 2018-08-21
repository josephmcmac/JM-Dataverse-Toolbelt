using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.XrmModule.XrmConnection;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Diagnostics;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class OpenSolutionModule : OptionActionModule
    {
        public override string MainOperationName => "Open Solution";

        public override string MenuGroup => "Web";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(OpenSolutionDialog), null);
        }
    }
}
