using System.Collections.Generic;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
{
    [GridOnlyEntry(nameof(Triggers))]
    public class ManagePluginTriggersRequest : ServiceRequestBase
    {
        private IEnumerable<IRecord> sdkMessageStepsPre;

        public IEnumerable<IRecord> GetSdkMessageStepsPre()
        {
            return sdkMessageStepsPre;
        }

        public void SetSdkMessageStepsPre(IEnumerable<IRecord> value)
        {
            sdkMessageStepsPre = value;
        }

        [Hidden]
        public string AssemblyName { get; set; }

        [Hidden]
        [LookupConditionFor(nameof(PluginTrigger.Plugin), Fields.plugintype_.pluginassemblyid)]
        [LookupConditionFor(nameof(Triggers) + "." + nameof(PluginTrigger.Plugin), Fields.plugintype_.pluginassemblyid)]
        public Lookup Assembly { get; set; }
        [FormEntry]
        public IEnumerable<PluginTrigger> Triggers { get; set; } 
    }
}
