using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
{
    [Group(Sections.Plugin, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10, displayLabel: false)]
    [Group(Sections.MessagePipeline, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20)]
    [Group(Sections.PreImage, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30)]
    [Group(Sections.OtherOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40)]
    [DoNotAllowGridEdit]
    public class PluginTrigger
    {
        public PluginTrigger()
        {
            Mode = PluginMode.Synch;
            PreImageAllFields = true;
            PreImageName = "PreImage";
        }

        [Hidden]
        public string Id { get; set; }

        [Group(Sections.Plugin)]
        [DisplayOrder(10)]
        [EditableFormWidth(250)]
        [GridWidth(300)]
        [RequiredProperty]
        [ReferencedType(Entities.plugintype)]
        [UsePicklist]
        [InitialiseIfOneOption]
        [LookupCondition(Fields.plugintype_.isworkflowactivity, false)]
        public Lookup Plugin { get; set; }

        [Group(Sections.MessagePipeline)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [GridWidth(150)]
        public PluginStage? Stage { get; set; }

        [Group(Sections.MessagePipeline)]
        [DisplayOrder(30)]
        [GridWidth(150)]
        [RequiredProperty]
        [ReferencedType(Entities.sdkmessage)]
        [LookupCondition(Fields.sdkmessage_.isprivate, false)]
        [UsePicklist]
        [OrderPriority("Create", "Update", "Delete")]
        public Lookup Message { get; set; }

        [Group(Sections.MessagePipeline)]
        [DisplayOrder(40)]
        [GridWidth(150)]
        [RecordTypeFor(nameof(FilteringFields))]
        [RecordTypeFor(nameof(PreImageFields))]
        public RecordType RecordType { get; set; }

        [Group(Sections.MessagePipeline)]
        [DisplayOrder(50)]
        [RequiredProperty]
        [GridWidth(100)]
        [PropertyInContextByPropertyValue(nameof(Stage), PluginStage.PostEvent)]
        public PluginMode? Mode { get; set; }

        [Group(Sections.MessagePipeline)]
        [DisplayOrder(60)]
        [GridWidth(75)]
        [RequiredProperty]
        public int Rank { get; set; }

        [Group(Sections.OtherOptions)]
        [GridWidth(150)]
        [DisplayName("Run In User Context (Optional)")]
        [ReferencedType(Entities.systemuser)]
        [DisplayOrder(60)]
        [UsePicklist]
        [DoNotAllowAdd]
        public Lookup SpecificUserContext { get; set; }

        [Group(Sections.OtherOptions)]
        [DisplayOrder(65)]
        [GridWidth(225)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValue(nameof(Message), "Update")]
        public IEnumerable<RecordField> FilteringFields { get; set; }

        [Group(Sections.PreImage)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValues(nameof(Message), "Update", "Delete")]
        [RequiredProperty]
        [DisplayOrder(70)]
        [GridWidth(100)]
        public string PreImageName { get; set; }

        [Group(Sections.PreImage)]
        [DisplayOrder(80)]
        [GridWidth(125)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValues(nameof(Message), "Update", "Delete")]
        public bool PreImageAllFields { get; set; }

        [Group(Sections.PreImage)]
        [DisplayOrder(90)]
        [GridWidth(225)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValues(nameof(Message), "Update", "Delete")]
        [PropertyInContextByPropertyValue(nameof(PreImageAllFields), false)]
        public IEnumerable<RecordField> PreImageFields { get; set; }


        [Hidden]
        public string PreImageId { get; set; }

        [Hidden]
        public string PreImageIdUnique { get; set; }

        private static class Sections
        {
            public const string Plugin = "Plugin";
            public const string MessagePipeline = "Message Pipeline";
            public const string PreImage = "Pre Image";
            public const string OtherOptions = "Other Options";
        }

        public enum PluginStage
        {
            PreValidationEvent = 10,
            PreOperationEvent = 20,
            PostEvent = 40
        }

        public enum PluginMode
        {
            Synch = 0,
            Asynch = 1
        }
    }
}
