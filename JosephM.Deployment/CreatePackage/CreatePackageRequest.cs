using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Attributes;
using JosephM.Xrm.DataImportExport.XmlImport;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.CreatePackage
{
    [DisplayName("Create Deployment Package")]
    [Instruction("A folder will be created containing the solution zip and xml files for the data. The deploy package process may then be run to import the package into another instance")]
    [AllowSaveAndLoad]
    [Group(Sections.PackageSolution, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10, displayLabel: false)]
    [Group(Sections.Main, Group.DisplayLayoutEnum.VerticalCentered, order: 20, displayLabel: false)]
    [Group(Sections.DataIncluded, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30)]
    [Group(Sections.DataIncludedRecordTypes, Group.DisplayLayoutEnum.VerticalCentered, order: 35, displayLabel: false)]
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
            IncludeFileAndImageFields = true;
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

        [EditableFormWidth(175)]
        [DoNotAllowAdd]
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

        [DisplayName("Include Notes & Attachments")]
        [GridWidth(110)]
        [Group(Sections.DataIncluded)]
        [DisplayOrder(1050)]
        public bool IncludeNotes { get; set; }

        [GridWidth(110)]
        [Group(Sections.DataIncluded)]
        [DisplayOrder(1055)]
        [DisplayName("Include File & Image Fields")]
        public bool IncludeFileAndImageFields { get; set; }

        [DisplayName("Include N:N Links Between Records")]
        [GridWidth(110)]
        [Group(Sections.DataIncluded)]
        [DisplayOrder(1060)]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [DisplayOrder(1070)]
        [GridWidth(400)]
        [Group(Sections.DataIncludedRecordTypes)]
        [AllowGridFullScreen]
        public IEnumerable<ExportRecordType> DataToInclude { get; set; }

        [Hidden]
        public bool HideTypeAndFolder { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string PackageSolution = "Solution";
            public const string DataIncluded = "Data Included";
            public const string DataIncludedRecordTypes = "Record Types";
        }

    }
}