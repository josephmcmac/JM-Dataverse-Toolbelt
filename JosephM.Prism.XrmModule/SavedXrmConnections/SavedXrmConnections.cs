using System.Collections.Generic;
using JosephM.Core.Attributes;

namespace JosephM.XrmModule.SavedXrmConnections
{
    public class SavedXrmConnections : ISavedXrmConnections
    {
        [FormEntry]
        public IEnumerable<SavedXrmRecordConfiguration> Connections { get; set; }
    }
}
