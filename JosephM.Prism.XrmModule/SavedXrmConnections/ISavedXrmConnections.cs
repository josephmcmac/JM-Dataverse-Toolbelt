using System.Collections.Generic;

namespace JosephM.Prism.XrmModule.SavedXrmConnections
{
    public interface ISavedXrmConnections
    {
        IEnumerable<SavedXrmRecordConfiguration> Connections { get; set; }
    }
}