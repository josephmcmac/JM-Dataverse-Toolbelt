#region

using System.Collections.Generic;

#endregion

namespace JosephM.Record.Metadata
{
    public class ViewMetadata
    {
        public ViewMetadata(IEnumerable<ViewField> fields)
        {
            Fields = fields;
        }

        public IEnumerable<ViewField> Fields { get; private set; }

        public ViewType ViewType { get; set; }
    }
}