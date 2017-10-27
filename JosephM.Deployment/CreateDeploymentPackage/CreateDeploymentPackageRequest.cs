#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace JosephM.Deployment.CreateDeploymentPackage
{
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Connection, true, 20)]
    [Group(Sections.PackageSolution, true, 25)]
    [Group(Sections.DataIncluded, true, 30)]
    public class CreateDeploymentPackageRequest : ServiceRequestBase, IValidatableObject
    {
        public static CreateDeploymentPackageRequest CreateForCreatePackage(string folder, Lookup solution)
        {
            return new CreateDeploymentPackageRequest()
            {
                FolderPath = new Folder(folder),
                Solution = solution,
                HideTypeAndFolder = true
            };
        }

        public IsValidResponse Validate()
        {
            var response = new IsValidResponse();
            if (FolderPath != null && Directory.Exists(FolderPath.FolderPath))
            {
                if (FileUtility.GetFiles(FolderPath.FolderPath).Any(f => f.EndsWith("zip")))
                {
                    response.AddInvalidReason(string.Format("{0} Already Contains .ZIP files. Remove The .ZIP Files Or Select Another Folder", GetType().GetProperty(nameof(FolderPath)).GetDisplayName()));
                }
            }
            return response;
        }

        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [DisplayName("Folder To Export The Files Into")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public Folder FolderPath { get; set; }

        [Group(Sections.Main)]
        [DisplayOrder(40)]
        [DisplayName("Deploy Package Into (Optional)")]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public SavedXrmRecordConfiguration DeployPackageInto { get; set; }

        [DisplayOrder(510)]
        [RequiredProperty]
        [Group(Sections.PackageSolution)]
        [DisplayName("Solution")]
        [GridWidth(400)]
        [ReferencedType(Entities.solution)]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupFieldCascade(nameof(ThisReleaseVersion), Fields.solution_.version)]
        [UsePicklist(Fields.solution_.uniquename)]
        public Lookup Solution { get; set; }

        [Group(Sections.PackageSolution)]
        [DisplayOrder(500)]
        public bool ExportAsManaged { get; set; }

        [Group(Sections.PackageSolution)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [CascadeOnChange(nameof(SetVersionPostRelease))]
        [DisplayOrder(520)]
        [RequiredProperty]
        public string ThisReleaseVersion { get; set; }

        [Group(Sections.PackageSolution)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [DisplayOrder(530)]
        [RequiredProperty]
        public string SetVersionPostRelease { get; set; }

        [Group(Sections.DataIncluded)]
        [DisplayOrder(1050)]
        public bool IncludeNotes { get; set; }

        [Group(Sections.DataIncluded)]
        [DisplayOrder(1060)]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [DisplayOrder(1070)]
        [GridWidth(400)]
        public IEnumerable<ExportRecordType> DataToInclude { get; set; }

        [Hidden]
        public bool HideTypeAndFolder { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string PackageSolution = "PackageSolution";
            public const string Connection = "Connection";
            public const string DataIncluded = "Data Included Options";
        }

    }
}