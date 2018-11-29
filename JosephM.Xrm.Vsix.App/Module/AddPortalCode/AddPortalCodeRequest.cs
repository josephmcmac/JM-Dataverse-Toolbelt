using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.AddPortalCode
{
    [Group(Sections.Main, true, order: 10)]
    [Group(Sections.Options, true, order: 20)]
    [Group(Sections.TypesToInclude, true, order: 25, selectAll: true)]
    [Group(Sections.RecordsToInclude, true, order: 30, selectAll: true)]
    [Instruction("This process will export html, javascript and css configured in portal records into files in the visual studio project\n\nOnce exported the code may be deployed into the dynamics instance by right clicking the file, then selecting the 'Deploy Into Record' option")]
    public class AddPortalCodeRequest : ServiceRequestBase, IValidatableObject
    {
        public AddPortalCodeRequest()
        {
            IncludeHtml = true;
            IncludeCss = true;
            IncludeJavaScript = true;

            var exportConfigs = AddPortalCodeConfiguration.GetExportConfigs();

            var typesToInclude = new List<PortalRecordsToExport>();
            foreach(var item in exportConfigs.OrderBy(c => c.RecordType))
            {
                typesToInclude.Add(new PortalRecordsToExport
                {
                    RecordType = new RecordType(item.RecordType, item.RecordType)
                });
            }
            RecordsToExport = typesToInclude;
        }

        [Hidden]
        [ReadOnlyWhenSet]
        [DisplayOrder(10)]
        [Group(Sections.Main)]
        public string ProjectName { get; set; }

        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [DoNotAllowAdd]
        [RequiredProperty]
        [ReferencedType(Entities.adx_website)]
        [UsePicklist]
        [InitialiseIfOneOption]
        public Lookup WebSite { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.Options)]
        [DisplayName("Create Folder For Website Name (required to deploy code when multiple sites)")]
        public bool CreateFolderForWebsiteName { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.Options)]
        public bool ExportWhereFieldEmpty { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.TypesToInclude)]
        [DisplayName("HTML")]
        public bool IncludeHtml { get; set; }

        [DisplayOrder(220)]
        [Group(Sections.TypesToInclude)]
        [DisplayName("CSS")]
        public bool IncludeCss { get; set; }

        [DisplayOrder(230)]
        [Group(Sections.TypesToInclude)]
        [DisplayName("JavaScript")]
        public bool IncludeJavaScript { get; set; }

        [DisplayOrder(240)]
        [Group(Sections.TypesToInclude)]
        [DisplayName("Other (web file types)")]
        public bool IncludeOtherTypes { get; set; }

        [DoNotAllowAdd]
        [DoNotAllowDelete]
        [DoNotAllowGridOpen]
        [Group(Sections.RecordsToInclude)]
        [DisplayOrder(320)]
        public IEnumerable<PortalRecordsToExport> RecordsToExport { get; set; }

        public bool IncludeType(string type)
        {
            return RecordsToExport != null  && RecordsToExport.Any(r => r.RecordType?.Key == type && r.Selected);
        }

        public IsValidResponse Validate()
        {
            var response = new IsValidResponse();
            if(RecordsToExport == null || !RecordsToExport.Any(r => r.Selected))
            {
                response.AddInvalidReason("At Least One Record Type Must Be Included For Export");
            }
            return response;
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Options = "Options";
            public const string TypesToInclude = "Types To Include";
            public const string RecordsToInclude = "Records To Include";
        }

        public class PortalRecordsToExport : ISelectable
        {
            [GridWidth(75)]
            [DisplayOrder(20)]
            [DisplayName("Include")]
            public bool Selected { get; set; }

            [ReadOnlyWhenSet]
            [DisplayOrder(10)]
            public RecordType RecordType { get; set; }
            //[DisplayOrder(30)]
            //public bool AllRecords { get; set; }
            //[DisplayOrder(40)]
            //public bool RecordsToInclude { get; set; }
        }
    }
}