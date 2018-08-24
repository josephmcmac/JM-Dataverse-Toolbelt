#region

using System.Collections.Generic;
using JosephM.Record.Query;

#endregion

namespace JosephM.Record.Metadata
{
    public class ViewMetadata
    {
        public ViewMetadata(IEnumerable<ViewField> fields)
            : this(fields, null)
        {
        }

        public ViewMetadata(IEnumerable<ViewField> fields, IEnumerable<SortExpression> sorts)
        {
            Fields = fields;
            Sorts = sorts ?? new SortExpression[0];
        }

        public IEnumerable<ViewField> Fields { get; private set; }
        public IEnumerable<SortExpression> Sorts { get; private set; }
        public ViewType ViewType { get; set; }
        public string Id { get; set; }
        public string RawQuery { get; set; }
    }
}