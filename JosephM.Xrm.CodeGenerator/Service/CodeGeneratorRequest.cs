using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Application;
using JosephM.Record.Application.SettingTypes;

namespace JosephM.Xrm.CodeGenerator.Service
{
    public class CodeGeneratorRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public CodeGeneratorType? Type { get; set; }

        [RequiredProperty]
        public Folder Folder { get; set; }

        [RequiredProperty]
        [InitialiseFor("Type", CodeGeneratorType.CSharpEntitiesAndFields, "Schema")]
        [InitialiseFor("Type", CodeGeneratorType.CSharpOptionSets, "OptionSets")]
        [PropertyInContextByPropertyValues("Type",
            new object[]
            {
                CodeGeneratorType.CSharpOptionSets, CodeGeneratorType.JavaScriptOptionSets,
                CodeGeneratorType.CSharpEntitiesAndFields
            })]
        public string FileName { get; set; }

        [RequiredProperty]
        [InitialiseFor("Type", CodeGeneratorType.CSharpEntitiesAndFields, "Schema")]
        [InitialiseFor("Type", CodeGeneratorType.CSharpOptionSets, "OptionSets")]
        [PropertyInContextByPropertyValues("Type",
            new object[]
            {
                CodeGeneratorType.CSharpOptionSets, CodeGeneratorType.JavaScriptOptionSets,
                CodeGeneratorType.CSharpEntitiesAndFields
            })]
        public string Namespace { get; set; }

        [RequiredProperty]
        [InitialiseFor("Type", CodeGeneratorType.CSharpEntitiesAndFields, "Schema")]
        [InitialiseFor("Type", CodeGeneratorType.CSharpOptionSets, "OptionSets")]
        [PropertyInContextByPropertyValues("Type",
            new object[] {CodeGeneratorType.CSharpOptionSets, CodeGeneratorType.JavaScriptOptionSets})]
        public string ClassName { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValues("Type",
            new object[]
            {
                CodeGeneratorType.CSharpOptionSets, CodeGeneratorType.JavaScriptOptionSets,
                CodeGeneratorType.CSharpEntitiesAndFields
            })]
        public bool AllRecordTypes { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("AllRecordTypes", false)]
        [PropertyInContextByPropertyValues("Type",
    new object[] { CodeGeneratorType.CSharpOptionSets, CodeGeneratorType.JavaScriptOptionSets,
                CodeGeneratorType.CSharpEntitiesAndFields
            })]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }
    }
}