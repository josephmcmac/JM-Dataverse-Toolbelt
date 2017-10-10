
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Attributes;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using System.Collections.Generic;

namespace JosephM.XRM.VSIX
{
    [Group(Sections.ObjectPrefixes, true, 10)]
    [Group(Sections.Solution, true, 20)]
    [Group(Sections.ConnectionInstances, true, 30)]
    public class XrmPackageSettings : ISavedXrmConnections
    {
        //todo get rid of these somehow
        [Group(Sections.ObjectPrefixes)]
        [RequiredProperty]
        public string SolutionObjectPrefix { get; set; }
        [Group(Sections.ObjectPrefixes)]
        [RequiredProperty]
        public string SolutionDynamicsCrmPrefix { get; set; }

        [Group(Sections.Solution)]
        public bool AddToSolution { get; set; }

        [Group(Sections.Solution)]
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

        [FormEntry]
        [Group(Sections.ConnectionInstances)]
        public IEnumerable<SavedXrmRecordConfiguration> Connections { get; set; }

        private static class Sections
        {
            public const string ObjectPrefixes = "Object Prefixes";
            public const string Solution = "Active Dev Solution";
            public const string ConnectionInstances = "Instance Connections";
        }
    }
}