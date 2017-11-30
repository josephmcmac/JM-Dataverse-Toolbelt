using System.Collections.Generic;
using JosephM.Core.Service;
using JosephM.Core.Attributes;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Core.FieldType;
using JosephM.Core.Constants;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    [Group(Sections.Connection, true, 20)]
    public class ImportSolutionRequest : ServiceRequestBase
    {
        [Group(Sections.Connection)]
        [DisplayName("Saved Connection To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration Connection { get; set; }

        [Hidden]
        [RequiredProperty]
        [FileMask(FileMasks.ZipFile)]
        public FileReference SolutionZip { get; set; }

        private static class Sections
        {
            public const string Connection = "Connection";
        }
    }
}