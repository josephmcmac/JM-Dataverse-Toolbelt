using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Prism.XrmModule.XrmConnection;
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
