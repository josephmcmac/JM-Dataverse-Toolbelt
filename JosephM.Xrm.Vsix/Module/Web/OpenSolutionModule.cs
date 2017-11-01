using System;
using JosephM.Application.Modules;
using JosephM.Record.Xrm.XrmRecord;
using System.Diagnostics;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Application.Application;
using JosephM.XRM.VSIX;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class OpenSolutionModule : ActionModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }

        public override void DialogCommand()
        {
            var xrmRecordService = ApplicationController.ResolveType(typeof(XrmRecordService)) as XrmRecordService;
            if (xrmRecordService == null)
                throw new NullReferenceException("xrmRecordService");
            var url = xrmRecordService.WebUrl;
            var settingsManager = ApplicationController.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
            if (settingsManager == null)
                throw new NullReferenceException("settingsManager");
            var packageSettings = settingsManager.Resolve<XrmPackageSettings>();
            var solution = packageSettings.Solution;
            if (solution == null)
                throw new NullReferenceException("No solution selected in the settings");

            var solutionUrl = string.Format("{0}{1}tools/solution/edit.aspx?id={2}", url, url.EndsWith("/") ? "" : "/", solution.Id.ToString());
            Process.Start(solutionUrl);
        }
    }
}
