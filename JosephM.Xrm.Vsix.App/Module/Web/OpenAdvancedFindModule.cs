using System;
using JosephM.Application.Modules;
using JosephM.Record.Xrm.XrmRecord;
using System.Diagnostics;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class OpenAdvancedFindModule : OptionActionModule
    {
        public override string MainOperationName => "Open Advanced Find";

        public override string MenuGroup => "Web";

        public override void DialogCommand()
        {
            var xrmRecordService = ApplicationController.ResolveType(typeof(XrmRecordService)) as XrmRecordService;
            if (xrmRecordService == null)
                throw new NullReferenceException("xrmRecordService");

            var url = xrmRecordService.WebUrl;
            var solutionUrl = string.Format("{0}{1}main.aspx?pagetype=advancedfind", url, url.EndsWith("/") ? "" : "/");
            Process.Start(solutionUrl);
        }
    }
}
