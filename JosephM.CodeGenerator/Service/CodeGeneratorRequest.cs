using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.Service
{
    public class CodeGeneratorRequest : ServiceRequestBase
    {
        public CodeGeneratorRequest()
        {
            IncludeEntities = true;
            IncludeFields= true;
            IncludeOptions = true;
            IncludeRelationships = true;

        }

        [RequiredProperty]
        public CodeGeneratorType? Type { get; set; }

        [RequiredProperty]
        public Folder Folder { get; set; }

        [RequiredProperty]
        [InitialiseFor("Type", CodeGeneratorType.CSharpMetadata, "Schema")]
        [PropertyInContextByPropertyNotNull("Type")]
        public string FileName { get; set; }

        [RequiredProperty]
        [InitialiseFor("Type", CodeGeneratorType.CSharpMetadata, "Schema")]
        [PropertyInContextByPropertyNotNull("Type")]
        public string Namespace { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool IncludeEntities { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool IncludeFields { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool IncludeRelationships { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool IncludeOptions { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", CodeGeneratorType.CSharpMetadata)]
        public bool AllRecordTypes { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("AllRecordTypes", false)]
        [PropertyInContextByPropertyNotNull("Type")]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }
    }
}