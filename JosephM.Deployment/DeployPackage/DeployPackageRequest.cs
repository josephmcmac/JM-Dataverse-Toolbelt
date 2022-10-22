using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Xrm.DataImportExport.XmlExport;
using JosephM.XrmModule.SavedXrmConnections;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;

namespace JosephM.Deployment.DeployPackage
{
    [Instruction("Select the package folder and instance to import into")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, Group.DisplayLayoutEnum.VerticalCentered, order: 10, displayLabel: false)]
    public class DeployPackageRequest : ServiceRequestBase, IImportXmlRequest
    {
        public static DeployPackageRequest CreateForDeployPackage(string folder)
        {
            return new DeployPackageRequest()
            {
                FolderContainingPackage = new Folder(folder),
                HideTypeAndFolder = true
            };
        }

        [GridWidth(250)]
        [Group(Sections.Main)]
        [DisplayOrder(100)]
        [DisplayName("Saved Connection Instance To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [GridWidth(300)]
        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public Folder FolderContainingPackage { get; set; }

        [Hidden]
        public bool HideTypeAndFolder { get; set; }

        public void ClearLoadedEntities()
        {
            _loadedEntities = null;
        }

        private IDictionary<string, Entity> _loadedEntities;
        public IDictionary<string, Entity> GetOrLoadEntitiesForImport(LogController logController)
        {
            if (FolderContainingPackage == null)
                throw new NullReferenceException($"Cannot load files {nameof(FolderContainingPackage)} property is null");
            if (_loadedEntities == null)
            {
                foreach (var childFolder in Directory.GetDirectories(FolderContainingPackage.FolderPath))
                {
                    if (new DirectoryInfo(childFolder).Name == "Data")
                    {
                        _loadedEntities = ImportXmlService.LoadEntitiesFromXmlFiles(childFolder, logController);
                    }
                }
            }
            if (_loadedEntities == null)
            {
                _loadedEntities = new Dictionary<string, Entity>();
            }
            return _loadedEntities;
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Connection = "Connection";
        }
    }
}