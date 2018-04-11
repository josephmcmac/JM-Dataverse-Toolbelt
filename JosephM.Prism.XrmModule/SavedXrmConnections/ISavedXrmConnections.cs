using System.Collections.Generic;

namespace JosephM.XrmModule.SavedXrmConnections
{
    public interface ISavedXrmConnections
    {
        IEnumerable<SavedXrmRecordConfiguration> Connections { get; set; }
    }
}