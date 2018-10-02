using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.CreatePackage
{
    [DisplayName("Create Deployment Package")]
    [Instruction("A Folder Will Be Created Containing The Solution Zip And Xml Files For The Data To Be Included In The Deployment Package. The Deploy Package Process May Then Be Run On That Folder To Install The Package Into Another Dynamics Instance")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Connection, true, 20)]
    [Group(Sections.PackageSolution, true, 25)]
    [Group(Sections.DataIncluded, true, 30)]
    [Group(Sections.DataIncludedRecordTypes, true, order: 35, displayLabel: false)]
    public class CreatePackageRequest : ServiceRequestBase, IValidatableObject
    {
        public static CreatePackageRequest CreateForCreatePackage(string folder, Lookup solution)
        {
            return new CreatePackageRequest()
            {
                FolderPath = new Folder(folder),
                Solution = solution,
                HideTypeAndFolder = true
            };
        }

        public CreatePackageRequest()
        {
            IncludeNNRelationshipsBetweenEntities = true;
            IncludeNotes = true;
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

        [GridWidth(150)]
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
        [GridWidth(150)]
        [ReferencedType(Entities.solution)]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupFieldCascade(nameof(ThisReleaseVersion), Fields.solution_.version)]
        [UsePicklist(Fields.solution_.uniquename)]
        public Lookup Solution { get; set; }

        [GridWidth(110)]
        [Group(Sections.PackageSolution)]
        [DisplayOrder(500)]
        public bool ExportAsManaged { get; set; }

        [GridWidth(110)]
        [Group(Sections.PackageSolution)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [CascadeOnChange(nameof(SetVersionPostRelease))]
        [DisplayOrder(520)]
        [RequiredProperty]
        public string ThisReleaseVersion { get; set; }

        [GridWidth(110)]
        [Group(Sections.PackageSolution)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [DisplayOrder(530)]
        [RequiredProperty]
        public string SetVersionPostRelease { get; set; }

        [DisplayName("Include Attached Notes")]
        [GridWidth(110)]
        [Group(Sections.DataIncluded)]
        [DisplayOrder(1050)]
        public bool IncludeNotes { get; set; }

        [DisplayName("Include N:N Links Between Records")]
        [GridWidth(110)]
        [Group(Sections.DataIncluded)]
        [DisplayOrder(1060)]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [DisplayOrder(1070)]
        [GridWidth(400)]
        [Group(Sections.DataIncludedRecordTypes)]
        public IEnumerable<ExportRecordType> DataToInclude { get; set; }

        [Hidden]
        public bool HideTypeAndFolder { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string PackageSolution = "Solution";
            public const string Connection = "Connection";
            public const string DataIncluded = "Data Included";
            public const string DataIncludedRecordTypes = "Record Types";
        }

    }
}