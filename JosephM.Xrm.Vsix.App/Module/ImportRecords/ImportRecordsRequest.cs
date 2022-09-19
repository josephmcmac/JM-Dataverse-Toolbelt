using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Xrm.DataImportExport.XmlExport;
using JosephM.XrmModule.SavedXrmConnections;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    [Group(Sections.Connection, true, 20)]
    public class ImportRecordsRequest : ServiceRequestBase, IImportXmlRequest
    {
        [Group(Sections.Connection)]
        [DisplayName("Saved Connection To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [Hidden]
        [RequiredProperty]
        [FileMask(FileMasks.XmlFile)]
        public IEnumerable<FileReference> XmlFiles{ get; set; }

        public void ClearLoadedEntities()
        {
            _loadedEntities = null;
        }

        private IDictionary<string, Entity> _loadedEntities;
        public IDictionary<string, Entity> GetOrLoadEntitiesForImport(LogController logController)
        {
            if (XmlFiles == null)
                throw new NullReferenceException($"Cannot load files {nameof(XmlFiles)} property is null");
            if (_loadedEntities == null)
            {
                _loadedEntities = ImportXmlService.LoadEntitiesFromXmlFiles(XmlFiles.Select(fr => fr.FileName).ToArray());
            }
            return _loadedEntities;
        }

        private static class Sections
        {
            public const string Connection = "Connection";
        }
    }
}