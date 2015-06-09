using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    public class DebugDialogRequest : ServiceRequestBase
    {
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [ConnectionFor("Solution")]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [ReferencedType("solution")]
        [LookupCondition("ismanaged", false)]
        [LookupCondition("isvisible", true)]
        public Lookup Solution { get; set; }
    }
}