using System;
using JosephM.Application.Modules;
using JosephM.Record.Xrm.XrmRecord;
using System.Diagnostics;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.Xrm.Vsix.Module
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class ClearCacheModule : OptionActionModule
    {
        public override string MainOperationName => "Cache Cleared";

        public override string MenuGroup => "Settings";

        public override void DialogCommand()
        {
            var xrmRecordService = ApplicationController.ResolveType(typeof(XrmRecordService)) as XrmRecordService;
            if (xrmRecordService == null)
                throw new NullReferenceException("xrmRecordService");

            xrmRecordService.ClearCache();
            ApplicationController.UserMessage("Cache Cleared");
        }
    }
}
