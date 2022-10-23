using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Deployment.ImportSolution
{
    [Group(Sections.SolutionFile, Group.DisplayLayoutEnum.VerticalCentered, order: 10, displayLabel: false)]
    [Group(Sections.Connection, Group.DisplayLayoutEnum.VerticalCentered, order: 20, displayLabel: false)]
    public class ImportSolutionRequest : ServiceRequestBase
    {
        public static ImportSolutionRequest CreateForImportSolution(string file)
        {
            return new ImportSolutionRequest()
            {
                SolutionZip = new FileReference(file),
                HideSolutionFile = true
            };
        }

        [Group(Sections.Connection)]
        [DisplayName("Saved connection instance to import into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [Hidden]
        public bool HideSolutionFile { get; set; }

        [Group(Sections.SolutionFile)]
        [RequiredProperty]
        [FileMask(FileMasks.ZipFile)]
        [PropertyInContextByPropertyValue(nameof(HideSolutionFile), false)]
        public FileReference SolutionZip { get; set; }

        private static class Sections
        {
            public const string SolutionFile = "Solution File";
            public const string Connection = "Connection";
        }
    }
}