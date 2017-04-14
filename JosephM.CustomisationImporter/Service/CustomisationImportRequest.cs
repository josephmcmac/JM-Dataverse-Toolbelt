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
    [Group("Include These Items In Import", true)]
    public class CustomisationImportRequest : ServiceRequestBase
    {
        public bool AddToSolution { get; set; }
        [RequiredProperty]
        [ReferencedType(Xrm.Schema.Entities.solution)]
        [UsePicklist]
        [LookupCondition(Xrm.Schema.Fields.solution_.ismanaged, false)]
        [LookupCondition(Xrm.Schema.Fields.solution_.isvisible, true)]
        [LookupCondition(Xrm.Schema.Fields.solution_.uniquename, ConditionType.NotEqual, "default")]
        [PropertyInContextByPropertyValue("AddToSolution", true)]
        public Lookup Solution { get; set; }
        [RequiredProperty]
        [FileMask(FileMasks.ExcelFile)]
        public FileReference ExcelFile { get; set; }
        [Group("Include These Items In Import")]
        public bool Entities { get; set; }
        [Group("Include These Items In Import")]
        public bool Fields { get; set; }
        [Group("Include These Items In Import")]
        public bool Relationships { get; set; }
        [Group("Include These Items In Import")]
        public bool FieldOptionSets { get; set; }
        [Group("Include These Items In Import")]
        public bool SharedOptionSets { get; set; }
        [Group("Include These Items In Import")]
        public bool Views { get; set; }
    }
}