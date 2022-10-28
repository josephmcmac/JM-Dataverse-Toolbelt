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
using System.Linq;

namespace JosephM.Deployment.DeployPackage
{
    [Instruction("Select the package folder and instance to import into")]
    [Group(Sections.Main, Group.DisplayLayoutEnum.VerticalCentered, order: 10, displayLabel: false)]
    public class DeployPackageRequest : ServiceRequestBase, IImportXmlRequest, IValidatableObject
    {
        public static DeployPackageRequest CreateForDeployPackage(string folder)
        {
            return new DeployPackageRequest()
            {
                FolderContainingPackage = new Folder(folder),
                HideTypeAndFolder = true
            };
        }

        [GridWidth(300)]
        [Group(Sections.Main)]
        [DisplayOrder(10)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public Folder FolderContainingPackage { get; set; }

        [GridWidth(250)]
        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [DisplayName("Saved Connection Instance To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections), allowAddNew: false)]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [Group(Sections.Main)]
        [RequiredProperty]
        [DoNotAllowGridOpen]
        [DoNotAllowAdd]
        [DoNotAllowDelete]
        [DisplayOrder(30)]
        [PropertyInContextByPropertyNotNull(nameof(Connection))]
        [PropertyInContextByPropertyNotNull(nameof(FolderContainingPackage))]

        public IEnumerable<DeployPackageSolutionImportItem> SolutionsForDeployment { get; set; }

        [Hidden]
        public bool HasRecordsForImport
        {
            get
            {
                return RecordsForImport != null && RecordsForImport.Any();
            }
        }

        [Group(Sections.Main)]
        [DoNotAllowGridEdit]
        [DoNotAllowGridOpen]
        [DoNotAllowAdd]
        [DoNotAllowDelete]
        [DisplayOrder(40)]
        [PropertyInContextByPropertyNotNull(nameof(Connection))]
        [PropertyInContextByPropertyNotNull(nameof(FolderContainingPackage))]
        [PropertyInContextByPropertyValue(nameof(HasRecordsForImport), true)]

        public IEnumerable<DeployPackageRecordTypeImport> RecordsForImport { get; set; }

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

        public IsValidResponse Validate()
        {
            var response = new IsValidResponse();
            if(SolutionsForDeployment != null
                && SolutionsForDeployment.Any(s => s.IsManaged == true
                    && s.IsCurrentlyInstalledInTarget == true
                    && s.CurrentTargetVersionManaged == false))
            {
                response.AddInvalidReason("A managed solution cannot be installed into an instance where that solution is already unmanaged. Delete the solution from the target instance and try again");
            }
            return response;
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Connection = "Connection";
        }
    }
}