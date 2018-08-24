using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Deployment.DeployPackage
{
    [Instruction("The Solution In The Package Will Be Installed And Published In The Target Instance. Any Data Included In The Package Will Also Be Imported Matching By Either Id, Then Name, Or If No Match Is Found A New Record Created")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Connection, true, 20)]
    public class DeployPackageRequest : ServiceRequestBase
    {
        public static DeployPackageRequest CreateForDeployPackage(string folder)
        {
            return new DeployPackageRequest()
            {
                FolderContainingPackage = new Folder(folder),
                HideTypeAndFolder = true
            };
        }

        [Group(Sections.Connection)]
        [DisplayOrder(100)]
        [DisplayName("Saved Connection To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration Connection { get; set; }


        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public Folder FolderContainingPackage { get; set; }

        [Hidden]
        public bool HideTypeAndFolder { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Connection = "Connection";
        }
    }
}