using System.Collections.Generic;
using JosephM.Core.Attributes;

namespace JosephM.Prism.XrmModule.SavedXrmConnections
{
    public class SavedXrmConnections : ISavedXrmConnections
    {
        [FormEntry]
        public IEnumerable<SavedXrmRecordConfiguration> Connections { get; set; }
    }
}
