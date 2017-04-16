using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;

namespace JosephM.CodeGenerator.Service
{
    [Group(Sections.Type, true)]
    [Group(Sections.FileDetails, true)]
    [Group(Sections.OptionSetSelection, true)]
    [Group(Sections.IncludeConstantsForTheseItems, true)]
    [Group(Sections.RecordTypes, true)]
    [DisplayName("Code Generation")]
    public class CodeGeneratorRequest : ServiceRequestBase
    {
        public CodeGeneratorRequest()
        {
            Entities = true;
            Fields = true;
            FieldOptions = true;
            Relationships = true;
            IncludeAllRecordTypes = true;
            SharedOptions = true;
            Actions = true;
        }

        [Group(Sections.Type)]
        [RequiredProperty]
        public CodeGeneratorType? Type { get; set; }

        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Folder To Save The File Into")]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public Folder Folder { get; set; }

        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Name Of The File To Create")]
        [InitialiseFor("Type", CodeGeneratorType.CSharpMetadata, "Schema")]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public string FileName { get; set; }

        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Namespace Of The Code Object")]
        [InitialiseFor("Type", CodeGeneratorType.CSharpMetadata, "Schema")]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public string Namespace { get; set; }

        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Entities { get; set; }

        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Fields { get; set; }

        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Relationships { get; set; }

        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool FieldOptions { get; set; }

        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool SharedOptions { get; set; }

        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Actions { get; set; }

        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata })]
        public bool IncludeAllRecordTypes { get; set; }

        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("IncludeAllRecordTypes", false)]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata })]
        [DisplayName("Include These Specific Record Types")]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }

        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.JavaScriptOptionSets })]
        [RecordTypeFor("SpecificOptionSetField")]
        public RecordType RecordType { get; set; }

        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.JavaScriptOptionSets)]
        [PropertyInContextByPropertyNotNull("RecordType")]
        public bool AllOptionSetFields { get; set; }

        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.JavaScriptOptionSets)]
        [PropertyInContextByPropertyValue("AllOptionSetFields", false)]
        [PropertyInContextByPropertyNotNull("RecordType")]
        [LookupCondition("FieldType", ConditionType.In, new[] { RecordFieldType.Picklist, RecordFieldType.State, RecordFieldType.Status })]
        public RecordField SpecificOptionSetField { get; set; }

        [Group(Sections.Type)]
        [Multiline]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.FetchToJavascript)]
        public string Fetch { get; set; }

        private static class Sections
        {
            public const string RecordTypes = "Record Types";
            public const string OptionSetSelection = "Select Details Of The Option Set Field(s) To Generate Constants For";
            public const string IncludeConstantsForTheseItems = "Include Constants For These Items";
            public const string Type = "Select The Code Generation Type";
            public const string FileDetails = "File Details";
        }
    }
}