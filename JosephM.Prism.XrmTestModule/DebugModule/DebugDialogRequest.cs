using System.Collections;
using System.Collections.Generic;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Attributes;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    public class DebugDialogRequest : ServiceRequestBase
    {
        [FileMask(FileMasks.ZipFile)]
        public FileReference SolutionFile { get; set; }

        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [ConnectionFor("Solution")]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [ReferencedType("solution")]
        [LookupCondition("ismanaged", false)]
        [LookupCondition("isvisible", true)]
        public Lookup Solution { get; set; }

        [ReferencedType("solution")]
        [LookupCondition("ismanaged", false)]
        [LookupCondition("isvisible", true)]
        [UsePicklist]
        public Lookup SolutionPicklistActiveConnection { get; set; }

        [LookupConditionFor("SolutionPicklistActiveConnection", "uniquename")]
        public string SolutionName
        {
            get { return "Something"; }
        }

        public IEnumerable<DebugDialogRequestItem> Items { get; set; }

        public class DebugDialogRequestItem
        {
            [ReferencedType("solution")]
            [LookupCondition("ismanaged", false)]
            [LookupCondition("isvisible", true)]
            [UsePicklist]
            public Lookup SolutionPicklistActiveConnection { get; set; }
        }
    }
}