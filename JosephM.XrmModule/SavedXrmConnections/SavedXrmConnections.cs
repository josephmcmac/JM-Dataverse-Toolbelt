using System.Collections.Generic;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [GridOnlyEntry(nameof(Connections))]
    public class SavedXrmConnections : ISavedXrmConnections
    {
        [FormEntry]
        public IEnumerable<SavedXrmRecordConfiguration> Connections { get; set; }
    }
}
