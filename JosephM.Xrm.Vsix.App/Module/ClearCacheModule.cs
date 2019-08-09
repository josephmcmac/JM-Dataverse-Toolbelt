using JosephM.Application.Modules;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.SavedXrmConnections;
using System;

namespace JosephM.Xrm.Vsix.Module
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
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
