
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Attributes;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;

namespace JosephM.XRM.VSIX
{
    public class XrmPackageSettings
    {
        [RequiredProperty]
        public string SolutionObjectPrefix { get; set; }
        [RequiredProperty]
        public string SolutionDynamicsCrmPrefix { get; set; }
        public bool AddToSolution { get; set; }
        [RequiredProperty]
        [ReferencedType(Entities.solution)]
        [UsePicklist(Fields.solution_.uniquename)]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupCondition(Fields.solution_.uniquename, ConditionType.NotEqual, "default")]
        [PropertyInContextByPropertyValue("AddToSolution", true)]
        public Lookup Solution { get; set; }

        [Hidden]
        public string SolutionObjectInstancePrefix
        {
            get
            {
                if (string.IsNullOrEmpty(SolutionObjectPrefix))
                    return SolutionObjectPrefix;
                if (char.IsLower(SolutionObjectPrefix[0]))
                    return "" + char.ToUpper(SolutionObjectPrefix[0]) + (SolutionObjectPrefix.Length == 1 ? "" : SolutionObjectPrefix.Substring(1));
                if (char.IsUpper(SolutionObjectPrefix[0]))
                    return "" + char.ToLower(SolutionObjectPrefix[0]) + (SolutionObjectPrefix.Length == 1 ? "" : SolutionObjectPrefix.Substring(1));
                return SolutionObjectPrefix;
            }
        }
    }
}
