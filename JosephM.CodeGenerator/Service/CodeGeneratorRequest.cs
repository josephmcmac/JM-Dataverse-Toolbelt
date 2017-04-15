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
    [Group("Option Set Selection", true)]
    [Group("Include Constants For These Items", true)]
    [Group("Record Types", true)]
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

        [RequiredProperty]
        public CodeGeneratorType? Type { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public Folder Folder { get; set; }

        [RequiredProperty]
        [InitialiseFor("Type", CodeGeneratorType.CSharpMetadata, "Schema")]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public string FileName { get; set; }

        [RequiredProperty]
        [InitialiseFor("Type", CodeGeneratorType.CSharpMetadata, "Schema")]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata, CodeGeneratorType.JavaScriptOptionSets })]
        public string Namespace { get; set; }

        [Group("Include Constants For These Items", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Entities { get; set; }

        [Group("Include Constants For These Items", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Fields { get; set; }

        [Group("Include Constants For These Items", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Relationships { get; set; }

        [Group("Include Constants For These Items", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool FieldOptions { get; set; }

        [Group("Include Constants For These Items", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool SharedOptions { get; set; }

        [Group("Include Constants For These Items", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool Actions { get; set; }

        [Group("Record Types", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata })]
        public bool IncludeAllRecordTypes { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("IncludeAllRecordTypes", false)]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.CSharpMetadata })]
        [DisplayName("Include These Specific Record Types")]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }

        [Group("Option Set Selection", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("Type", new object[] { CodeGeneratorType.JavaScriptOptionSets })]
        [RecordTypeFor("SpecificOptionSetField")]
        public RecordType RecordType { get; set; }

        [Group("Option Set Selection", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.JavaScriptOptionSets)]
        [PropertyInContextByPropertyNotNull("RecordType")]
        public bool AllOptionSetFields { get; set; }

        [Group("Option Set Selection", true)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.JavaScriptOptionSets)]
        [PropertyInContextByPropertyValue("AllOptionSetFields", false)]
        [PropertyInContextByPropertyNotNull("RecordType")]
        [LookupCondition("FieldType", ConditionType.In, new[] { RecordFieldType.Picklist, RecordFieldType.State, RecordFieldType.Status })]
        public RecordField SpecificOptionSetField { get; set; }

        [Multiline]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.FetchToJavascript)]
        public string Fetch { get; set; }
    }
}