using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Query;

namespace JosephM.SolutionComponentExporter
{
    [Instruction("An Excel file will be generated with sheets for each export option")]
    [AllowSaveAndLoad]
    [DisplayName("Solution Component Export")]
    public class SolutionComponentExporterRequest : ServiceRequestBase
    {
        [DisplayOrder(10)]
        [RequiredProperty]
        [ReferencedType(Xrm.Schema.Entities.solution)]
        [UsePicklist(Xrm.Schema.Fields.solution_.uniquename)]
        [LookupCondition(Xrm.Schema.Fields.solution_.ismanaged, false)]
        [LookupCondition(Xrm.Schema.Fields.solution_.isvisible, true)]
        [LookupCondition(Xrm.Schema.Fields.solution_.uniquename, ConditionType.NotEqual, "default")]
        public Lookup Solution { get; set; }

        [DisplayOrder(20)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }
    }
}