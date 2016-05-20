using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;

namespace JosephM.XRM.VSIX.Commands.ManagePluginTriggers
{
    public class PluginTriggers
    {
        [Hidden]
        [LookupConditionFor(nameof(PluginTrigger.Plugin), Fields.plugintype_.pluginassemblyid)]
        public Lookup Assembly { get; set; }
        public IEnumerable<PluginTrigger> Triggers { get; set; } 
    }
    public class PluginTrigger
    {
        [Hidden]
        public string Id { get; set; }

        [GridWidth(300)]
        [RequiredProperty]
        public string Name { get; set; }

        //todo check re improve multiple load picklist
        [GridWidth(300)]
        [RequiredProperty]
        [ReferencedType(Entities.plugintype)]
        [UsePicklist]
        [LookupCondition(Fields.plugintype_.isworkflowactivity, false)]
        public Lookup Plugin { get; set; }

        [GridWidth(150)]
        [RequiredProperty]
        [ReferencedType(Entities.sdkmessage)]
        [LookupCondition(Fields.sdkmessage_.isprivate, false)]
        [UsePicklist]
        public Lookup Message { get; set; }

        [GridWidth(150)]
        public RecordType RecordType { get; set; }

        [RequiredProperty]
        [GridWidth(150)]
        public PluginStage? Stage { get; set; }

        [RequiredProperty]
        [GridWidth(100)]
        public PluginMode? Mode { get; set; }

        [RequiredProperty]
        [GridWidth(100)]
        public int Rank { get; set; }

        public enum PluginStage
        {
            PreValidationEvent = 10,
            PreOperationEvent = 20,
            PostEvent = 40
        }

        public enum PluginMode
        {
            Synchronous = 0,
            Asynchronous = 1
        }
    }
}
