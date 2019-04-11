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
        [PropertyInContextByPropertyValue(nameof(Stage), PluginStage.PostEvent)]
        public PluginMode? Mode { get; set; }

        [DisplayOrder(52)]
        [GridWidth(75)]
        [RequiredProperty]
        public int Rank { get; set; }

        [GridWidth(150)]
        [DisplayName("Run In User Context (Optional)")]
        [ReferencedType(Entities.systemuser)]
        [DisplayOrder(60)]
        [UsePicklist]
        [DoNotAllowAdd]
        public Lookup SpecificUserContext { get; set; }

        [DisplayOrder(65)]
        [GridWidth(225)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValue(nameof(Message), "Update")]
        public IEnumerable<RecordField> FilteringFields { get; set; }

        [DisplayOrder(70)]
        [GridWidth(125)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        [PropertyInContextByPropertyValues(nameof(Message), "Update", "Delete")]
        public bool PreImageAllFields { get; set; }

        [DisplayOrder(75)]
        [GridWidth(225)]
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


        [Hidden]
        public string PreImageId { get; set; }

        [Hidden]
        public string PreImageIdUnique { get; set; }

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
