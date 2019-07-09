using System.Collections.Generic;

namespace JosephM.Record.Query
{
    public class Join
    {
        public Join(string sourceField, string targetType, string targetField)
        {
            SourceField = sourceField;
            TargetType = targetType;
            TargetField = targetField;
            RootFilter = new Filter();
            Sorts = new List<SortExpression>();
            Joins = new List<Join>();
            JoinType = Query.JoinType.Inner;
            Fields = new string[0];
        }

        public string SourceField { get; set; }
        public string TargetType { get; set; }
        public string TargetField { get; set; }
        public JoinType JoinType { get; set; }
        public Filter RootFilter { get; set; }
        public List<Join> Joins { get; set; }
        public List<SortExpression> Sorts { get; set; }
        public IEnumerable<string> Fields { get; set; }
        public string Alias { get; set; }
    }
}
