using System.Collections;
using System.Collections.Generic;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    [AllowSaveAndLoad]
    public class DebugDialogRequest : ServiceRequestBase
    {
        [FileMask(FileMasks.ZipFile)]
        public FileReference SolutionFile { get; set; }

        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [ConnectionFor("Solution")]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [PropertyInContextByPropertyNotNull(nameof(Connection))]
        [ReferencedType("solution")]
        [LookupCondition("ismanaged", false)]
        [LookupCondition("isvisible", true)]
        public Lookup Solution { get; set; }

        [UsePicklist]
        [ReferencedType(Entities.webresource)]
        public Lookup WebResourcePicklistActiveConnection { get; set; }

        [ReferencedType("solution")]
        [LookupCondition("ismanaged", false)]
        [LookupCondition("isvisible", true)]
        [UsePicklist(Fields.solution_.uniquename)]
        public Lookup SolutionPicklistActiveConnection { get; set; }

        //[LookupConditionFor("SolutionPicklistActiveConnection", "uniquename")]
        public string SolutionName
        {
            get { return "Something"; }
        }

        [PropertyInContextByPropertyNotNull(nameof(Connection))]
        public IEnumerable<DebugDialogRequestItem> Items { get; set; }

        public class DebugDialogRequestItem
        {
            [ReferencedType("solution")]
            [LookupCondition("ismanaged", false)]
            [LookupCondition("isvisible", true)]
            [UsePicklist(Fields.solution_.uniquename)]
            public Lookup SolutionPicklistActiveConnection { get; set; }
        }
    }
}