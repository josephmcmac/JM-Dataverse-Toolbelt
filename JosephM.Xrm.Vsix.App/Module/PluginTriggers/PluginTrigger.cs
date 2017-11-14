using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
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
        public PluginTrigger()
        {
            Mode = PluginMode.Synchronous;
            PreImageAllFields = true;
            PreImageName = "PreImage";
        }

        [Hidden]
        public string Id { get; set; }

        [DisplayOrder(10)]
        [GridWidth(300)]
        [RequiredProperty]
        [ReferencedType(Entities.plugintype)]
        [UsePicklist]
        [InitialiseIfOneOption]
        [LookupCondition(Fields.plugintype_.isworkflowactivity, false)]
        public Lookup Plugin { get; set; }

        [DisplayOrder(20)]
        [GridWidth(150)]
        [RequiredProperty]
        [ReferencedType(Entities.sdkmessage)]
        [LookupCondition(Fields.sdkmessage_.isprivate, false)]
        [UsePicklist]
        [OrderPriority("Create", "Update", "Delete")]
        public Lookup Message { get; set; }

        [DisplayOrder(30)]
        [GridWidth(150)]
        [RecordTypeFor(nameof(FilteringFields))]
        [RecordTypeFor(nameof(PreImageFields))]
        public RecordType RecordType { get; set; }

        [DisplayOrder(40)]
        [RequiredProperty]
        [GridWidth(150)]
        public PluginStage? Stage { get; set; }

        [DisplayOrder(50)]
        [RequiredProperty]
        [GridWidth(100)]
        public PluginMode? Mode { get; set; }

        [DisplayOrder(60)]
        [GridWidth(125)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValues(nameof(Message), "Update", "Delete")]
        public bool PreImageAllFields { get; set; }

        [DisplayOrder(70)]
        [GridWidth(300)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValues(nameof(Message), "Update", "Delete")]
        [PropertyInContextByPropertyValue(nameof(PreImageAllFields), false)]
        public IEnumerable<RecordField> PreImageFields { get; set; }

        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValues(nameof(Message), "Update", "Delete")]
        [RequiredProperty]
        [DisplayOrder(80)]
        [GridWidth(100)]
        public string PreImageName { get; set; }

        [ReferencedType(Entities.systemuser)]
        [DisplayOrder(100)]
        [UsePicklist]
        [DoNotAllowAdd]
        public Lookup SpecificUserContext { get; set; }

        [DisplayOrder(55)]
        [GridWidth(300)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValue(nameof(Message), "Update")]
        public IEnumerable<RecordField> FilteringFields { get; set; }

        [Hidden]
        public string PreImageId { get; set; }

        [DisplayOrder(52)]
        [GridWidth(75)]
        [RequiredProperty]
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
