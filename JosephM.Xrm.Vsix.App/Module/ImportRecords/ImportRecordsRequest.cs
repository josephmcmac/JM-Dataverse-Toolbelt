using System.Collections.Generic;
using JosephM.Core.Service;
using JosephM.Core.Attributes;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Core.FieldType;
using JosephM.Core.Constants;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    public class ImportRecordsRequest : ServiceRequestBase
    {
        [DisplayName("Saved Connection To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [Hidden]
        [RequiredProperty]
        [FileMask(FileMasks.XmlFile)]
        public IEnumerable<FileReference> XmlFiles{ get; set; }
    }
}