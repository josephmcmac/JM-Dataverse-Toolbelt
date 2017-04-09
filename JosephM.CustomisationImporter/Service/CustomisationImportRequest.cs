#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    [DisplayName("Import Customisations")]
    public class CustomisationImportRequest : ServiceRequestBase
    {
        public bool AddToSolution { get; set; }
        [RequiredProperty]
        [ReferencedType(Entities.solution)]
        [UsePicklist]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupCondition(Fields.solution_.uniquename, ConditionType.NotEqual, "default")]
        [PropertyInContextByPropertyValue(nameof(AddToSolution), true)]
        public Lookup Solution { get; set; }
        [RequiredProperty]
        [FileMask(FileMasks.ExcelFile)]
        public FileReference ExcelFile { get; set; }
        public bool IncludeEntities { get; set; }
        public bool IncludeFields { get; set; }
        public bool IncludeRelationships { get; set; }
        public bool UpdateOptionSets { get; set; }
        public bool UpdateViews { get; set; }
    }
}