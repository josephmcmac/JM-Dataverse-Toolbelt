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
            Joins = new List<Join>();
        }

        public string SourceField { get; set; }
        public string TargetType { get; set; }
        public string TargetField { get; set; }


        public Filter RootFilter { get; set; }

        public List<Join> Joins { get; set; }
    }
}
