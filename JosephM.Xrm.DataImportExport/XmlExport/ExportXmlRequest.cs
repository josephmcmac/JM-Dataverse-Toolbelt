using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.Xrm.DataImportExport.XmlImport
{
    [DisplayName("Export XML")]
    [Instruction("A Folder Will Be Created Containing An Xml File For Each Record Included. The Import XML Process May Then Be Run On That Folder To Import The Data Into A Dynamics Instance")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.RecordTypesOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30)]
    [Group(Sections.RecordTypes, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 35, displayLabel: false)]
    public class ExportXmlRequest : ServiceRequestBase
    {

        public static ExportXmlRequest CreateForAddData(string folder)
        {
            return new ExportXmlRequest()
            {
                Folder = new Folder(folder),
                HideFolder = true
            };
        }

        public ExportXmlRequest()
        {
            IncludeNNRelationshipsBetweenEntities = true;
            IncludeNotes = true;
        }

        [Hidden]
        public bool HideFolder { get; set; }


        [GridWidth(300)]
        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(HideFolder), false)]
        [DisplayName("Select The Folder To Export The XML Files Into")]
        public Folder Folder { get; set; }

        [GridWidth(110)]
        [DisplayOrder(200)]
        [Group(Sections.RecordTypesOptions)]
        [DisplayName("Include Notes & Attachments")]
        [RequiredProperty]
        public bool IncludeNotes { get; set; }

        [GridWidth(110)]
        [Group(Sections.RecordTypesOptions)]
        [DisplayOrder(205)]
        [DisplayName("Include File & Image Fields")]
        public bool IncludeFileAndImageFields { get; set; }

        [GridWidth(110)]
        [DisplayOrder(210)]
        [Group(Sections.RecordTypesOptions)]
        [DisplayName("Include N:N Links Between Records")]
        [RequiredProperty]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [Group(Sections.RecordTypes)]
        [GridWidth(500)]
        [DisplayOrder(300)]
        [RequiredProperty]
        [AllowGridFullScreen]
        public IEnumerable<ExportRecordType> RecordTypesToExport { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string RecordTypesOptions = "Record Types To Export";
            public const string RecordTypes = "Record Types";
        }
    }
}