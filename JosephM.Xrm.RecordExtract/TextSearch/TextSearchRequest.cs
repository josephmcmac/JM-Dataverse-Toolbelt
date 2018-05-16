using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.RecordExtract.DocumentWriter;
using System.Collections.Generic;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [Group(Sections.Document, true, 10)]
    [Group(Sections.SearchTerm, true, 20)]
    [Group(Sections.SearchOptions, true, 30)]
    [Group(Sections.Types, true, 40)]
    [DisplayName("Text Search")]
    public class TextSearchRequest : ServiceRequestBase
    {
        public TextSearchRequest()
        {
            SearchAllTypes = true;
            ExcludeEmails = true;
            ExcludePosts = true;
            SearchTerms = new[]
            {
                new SearchTerm()
            };
        }

        [DisplayOrder(5)]
        [Group(Sections.Document)]
        [RequiredProperty]
        public bool GenerateDocument { get; set; }

        [DisplayOrder(10)]
        [Group(Sections.Document)]
        [PropertyInContextByPropertyValue(nameof(GenerateDocument), true)]
        [RequiredProperty]
        public DocumentType DocumentFormat { get; set; }

        [DisplayOrder(20)]
        [Group(Sections.Document)]
        [PropertyInContextByPropertyValue(nameof(GenerateDocument), true)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [DisplayOrder(105)]
        [Group(Sections.SearchTerm)]
        [RequiredProperty]
        public SearchTermOperator Operator { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.SearchTerm)]
        [RequiredProperty]
        public IEnumerable<SearchTerm> SearchTerms { get; set; }

        [DisplayOrder(160)]
        [Group(Sections.SearchOptions)]
        public bool StripHtmlTagsPriorToSearch { get; set; }

        [DisplayOrder(170)]
        [Group(Sections.SearchOptions)]
        [PropertyInContextByPropertyValue(nameof(StripHtmlTagsPriorToSearch), true)]
        public IEnumerable<RecordFieldSetting> CustomHtmlFields { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.Types)]
        public bool SearchAllTypes { get; set; }

        [DisplayOrder(220)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), false)]

        public IEnumerable<RecordTypeSetting> TypesToSearch { get; set; }

        [DisplayOrder(230)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), true)]
        public bool ExcludeEmails { get; set; }

        [DisplayOrder(240)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), true)]
        public bool ExcludePosts { get; set; }

        [DisplayOrder(250)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), true)]
        public IEnumerable<RecordTypeSetting> OtherExclusions { get; set; }

        [DisplayOrder(260)]
        [Group(Sections.Types)]
        public IEnumerable<RecordFieldSetting> FieldExclusions { get; set; }

        private static class Sections
        {
            public const string Document = "Document";
            public const string SearchTerm = "Search Term";
            public const string SearchOptions = "Search Options";
            public const string Types = "Types to Search - note if All Types is selected some system object types are excluded in the searches";
        }

        public class SearchTerm
        {
            [RequiredProperty]
            public string Text { get; set; }
        }

        public enum SearchTermOperator
        {
            Or,
            And
        }
    }
}