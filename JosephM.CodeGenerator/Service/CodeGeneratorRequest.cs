using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System.Collections.Generic;

namespace JosephM.CodeGenerator.Service
{
    [Group(Sections.Type, true, 10)]
    [Group(Sections.FileDetails, true, 20)]
    [Group(Sections.IncludeConstantsForTheseItems, true, 30)]
    [Group(Sections.RecordTypes, true, 40)]
    [Group(Sections.OptionSetSelection, true, 50)]
    [Group(Sections.FetchXml, false, 60)]
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

        [DisplayOrder(10)]
        [Group(Sections.Type)]
        [RequiredProperty]
        public CodeGeneratorType? Type { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Folder To Save The File Into")]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public Folder Folder { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Name Of The File To Create")]
        [InitialiseFor("Type", CodeGeneratorType.CSharpMetadata, "Schema")]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public string FileName { get; set; }

        [DisplayOrder(120)]
        [Group(Sections.FileDetails)]
        [RequiredProperty]
        [DisplayName("Namespace Of The Code Object")]
        [InitialiseFor("Type", CodeGeneratorType.CSharpMetadata, "Schema")]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public string Namespace { get; set; }

        [DisplayOrder(200)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Entities { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Fields { get; set; }

        [DisplayOrder(220)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Relationships { get; set; }

        [DisplayOrder(230)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool FieldOptions { get; set; }

        [DisplayOrder(240)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool SharedOptions { get; set; }

        [DisplayOrder(250)]
        [Group(Sections.IncludeConstantsForTheseItems)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Actions { get; set; }

        [DisplayOrder(300)]
        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata })]
        public bool IncludeAllRecordTypes { get; set; }

        [DisplayOrder(310)]
        [Group(Sections.RecordTypes)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("IncludeAllRecordTypes", false)]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata })]
        [DisplayName("Include These Specific Record Types")]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }

        [DisplayOrder(400)]
        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.JavaScriptOptionSets })]
        [RecordTypeFor("SpecificOptionSetField")]
        public RecordType RecordType { get; set; }

        [DisplayOrder(410)]
        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.JavaScriptOptionSets)]
        [PropertyInContextByPropertyNotNull("RecordType")]
        public bool AllOptionSetFields { get; set; }

        [DisplayOrder(420)]
        [Group(Sections.OptionSetSelection)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.JavaScriptOptionSets)]
        [PropertyInContextByPropertyValue("AllOptionSetFields", false)]
        [PropertyInContextByPropertyNotNull("RecordType")]
        [LookupCondition("FieldType", ConditionType.In, new[] { RecordFieldType.Picklist, RecordFieldType.State, RecordFieldType.Status })]
        public RecordField SpecificOptionSetField { get; set; }

        [DisplayOrder(500)]
        [Group(Sections.FetchXml)]
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
            public const string FetchXml = "FetchXml";
            public const string FileDetails = "File Details";
        }
    }
}