using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Record.Query
{
    public class QueryDefinition
    {
        public QueryDefinition(string recordType)
        {
            RecordType = recordType;
            RootFilter = new Filter();
            Sorts = new List<SortExpression>();
            Top = -1;
        }

        public QueryDefinition(string recordType, string quickFindText)
            : this(recordType)
        {
            IsQuickFind = true;
            QuickFindText = quickFindText;
        }

        public bool IsQuickFind { get; set; }

        public string QuickFindText { get; set; }

        public string RecordType { get; set; }

        public IEnumerable<string> Fields { get; set; }

        public Filter RootFilter { get; set; }

        public List<SortExpression> Sorts { get; set; }

        public int Top { get; set; }
    }
}
