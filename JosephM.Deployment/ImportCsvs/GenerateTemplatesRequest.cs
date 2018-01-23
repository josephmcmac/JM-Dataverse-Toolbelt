using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportCsvs
{
    [Group(Sections.Main, true)]
    public class GenerateTemplatesRequest
    {
        [Group(Sections.Main)]
        [DisplayOrder(10)]
        [RequiredProperty]
        public Folder FolderToSaveInto { get; set; }

        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [RequiredProperty]
        public bool UseSchemaNames { get; set; }

        [RequiredProperty]
        public IEnumerable<GenerateTemplateConfiguration>  CsvsToGenerate { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
        }
    }
}
