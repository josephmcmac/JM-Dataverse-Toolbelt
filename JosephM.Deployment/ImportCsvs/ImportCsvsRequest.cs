#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

#endregion

namespace JosephM.Deployment.ImportCsvs
{
    [Instruction("All CSV Files Will Be Imported Into The Dynamics Instance. Matches To Update Records In The Target Will By Done By Either Primary Key, Then Name, Else If No Match Is Found A New Record Will Be Created")]
    [DisplayName("Import CSVs")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.CsvImport, true, 20)]
    [Group(Sections.Misc, true, 40)]
    public class ImportCsvsRequest : ServiceRequestBase
    {
        public ImportCsvsRequest()
        {
            MatchByName = true;
        }

        [DisplayOrder(15)]
        [Group(Sections.Main)]
        [RequiredProperty]
        public CsvImportOption FolderOrFiles { get; set; }

        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [DisplayName("Select The Folder Containing The CSV Files")]
        [PropertyInContextByPropertyValue("FolderOrFiles", CsvImportOption.Folder)]
        public Folder Folder { get; set; }

        [Group(Sections.Main)]
        [DisplayOrder(30)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("FolderOrFiles", CsvImportOption.SpecificFiles)]
        public IEnumerable<CsvToImport> CsvsToImport { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.CsvImport)]
        [DisplayName("Match Existing Records By Name When Importing")]
        [RequiredProperty]
        public bool MatchByName { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.CsvImport)]
        [DisplayName("Select The Format Of Any Dates In The CSV File")]
        [RequiredProperty]
        public DateFormat DateFormat { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        public enum CsvImportOption
        {
            Folder,
            SpecificFiles
        }

        [DoNotAllowGridOpen]
        public class CsvToImport
        {
            [RequiredProperty]
            [FileMask(FileMasks.CsvFile)]
            public FileReference Csv { get; set; }
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string CsvImport = "CSV Import Options";
            public const string Misc = "Misc";
        }
    }
}