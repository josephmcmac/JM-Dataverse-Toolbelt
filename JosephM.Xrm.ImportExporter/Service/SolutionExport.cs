using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Prism.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.ImportExporter.Service
{
    public class SolutionExport
    {
        [RequiredProperty]
        [GridWidth(400)]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [ConnectionFor("Solution")]
        [ReadOnlyWhenSet]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [RequiredProperty]
        [GridWidth(400)]
        [ReferencedType("solution")]
        [LookupCondition("ismanaged", false)]
        [LookupCondition("isvisible", true)]
        [PropertyInContextByPropertyNotNull("Connection")]
        public Lookup Solution { get; set; }

        [RequiredProperty]
        public bool Managed { get; set; }
    }
}
