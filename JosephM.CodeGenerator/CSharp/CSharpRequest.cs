using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.CodeGenerator.CSharp
{
    [Instruction("A .CS File Will Be Output Containing Code Constants For The Customisations In The Dynamics Instance")]
    [DisplayName("Generate C# Constants")]
    [Group(Sections.Type, true, 10)]
    [Group(Sections.FileDetails, true, 20)]
    [Group(Sections.IncludeConstantsForTheseItems, true, order: 30, selectAll: true)]
    [Group(Sections.RecordTypes, true, 40)]
    public class CSharpRequest : ServiceRequestBase
    {
        public CSharpRequest()
        {
            IncludeAllRecordTypes = true;
            Namespace = "Schema";
            FileName = "Schema";
        }


        [DisplayOrder(100)]
        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Folder To Save The File Into")]
        public Folder Folder { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Name Of The File To Create")]
        public string FileName { get; set; }

        [DisplayOrder(120)]
        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Namespace Of The Code Object")]
        public string Namespace { get; set; }

        [DisplayOrder(200)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        public bool Entities { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        public bool Fields { get; set; }

        [DisplayOrder(220)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        public bool Relationships { get; set; }

        [DisplayOrder(230)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        public bool FieldOptions { get; set; }

        [DisplayOrder(240)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        public bool SharedOptions { get; set; }

        [DisplayOrder(250)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        public bool Actions { get; set; }

        [DisplayOrder(300)]
        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        public bool IncludeAllRecordTypes { get; set; }

        [DisplayOrder(310)]
        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(IncludeAllRecordTypes), false)]
        [DisplayName("Include These Specific Record Types")]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }

        private static class Sections
        {
            public const string RecordTypes = "Record Types";
            public const string IncludeConstantsForTheseItems = "Include Constants For These Items";
            public const string Type = "Select The Code Generation Type";
            public const string FileDetails = "File Details";
        }
    }
}