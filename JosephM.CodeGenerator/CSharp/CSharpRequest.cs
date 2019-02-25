using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.CodeGenerator.CSharp
{
    [Instruction("A .CS File Will Be Output Containing Code Constants For The Customisations In The Dynamics Instance")]
    [DisplayName("Generate C# Constants")]
    [Group(Sections.Type, true, 10)]
    [Group(Sections.FileDetails, true, 20)]
    [Group(Sections.IncludeConstantsForTheseItems, true, order: 30, selectAll: true)]
    [Group(Sections.RecordTypes, true, 40)]
    public class CSharpRequest : ServiceRequestBase, IValidatableObject
    {
        public CSharpRequest()
        {
            IncludeAllRecordTypes = true;
            Namespace = "Schema";
            FileName = "Schema";
        }

        public IsValidResponse Validate()
        {
            //lets just ensure at leats one valid oiton is selected
            var validProperties = new[] { nameof(Entities), nameof(Fields), nameof(Relationships), nameof(FieldOptions), nameof(SharedOptions), nameof(Actions) };
            var isOneSelected = validProperties.Any(p => (bool)this.GetPropertyValue(p));
            var isValidResponse = new IsValidResponse();
            if (!isOneSelected)
            {
                var thisType = GetType();
                isValidResponse.AddInvalidReason($"At Least One Of {validProperties.Select(p => thisType.GetProperty(p).GetDisplayName()).JoinGrammarAnd()} Is Required To Be Selected");
            }
            return isValidResponse;
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