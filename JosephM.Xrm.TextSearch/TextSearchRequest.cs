using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.Xrm.TextSearch
{
    [DisplayName("Text Search")]
    [Instruction("String Fields Will Be Searched For Matches To The Search Text, Then A Report Output Detailing Matches. Matching Records May Then Be Edited Either Individually, Or Using A Bulk Replace Function")]
    [Group(Sections.Document, true, 10)]
    [Group(Sections.SearchTerm, true, 20)]
    [Group(Sections.SearchOptions, true, 30)]
    [Group(Sections.Types, true, 40)]
    [AllowSaveAndLoad]
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

        [GridWidth(75)]
        [DisplayOrder(105)]
        [Group(Sections.SearchTerm)]
        [RequiredProperty]
        public SearchTermOperator Operator { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.SearchTerm)]
        [RequiredProperty]
        public IEnumerable<SearchTerm> SearchTerms { get; set; }

        [GridWidth(100)]
        [DisplayOrder(160)]
        [Group(Sections.SearchOptions)]
        public bool StripHtmlTagsPriorToSearch { get; set; }

        [DisplayOrder(170)]
        [Group(Sections.SearchOptions)]
        [PropertyInContextByPropertyValue(nameof(StripHtmlTagsPriorToSearch), true)]
        public IEnumerable<RecordFieldSetting> CustomHtmlFields { get; set; }

        [GridWidth(100)]
        [DisplayOrder(210)]
        [Group(Sections.Types)]
        public bool SearchAllTypes { get; set; }

        [DisplayOrder(220)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), false)]

        public IEnumerable<TypeToSearch> TypesToSearch { get; set; }

        [GridWidth(100)]
        [DisplayOrder(230)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), true)]
        public bool ExcludeEmails { get; set; }

        [GridWidth(100)]
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

        [DoNotAllowGridOpen]
        [BulkAddRecordTypeFunction]
        public class TypeToSearch
        {
            [Hidden]
            public string Type { get { return RecordType == null ? null : RecordType.Key; } }

            public RecordType RecordType { get; set; }

            public override string ToString()
            {
                return RecordType != null ? RecordType.Value : base.ToString();
            }
        }

        private static class Sections
        {
            public const string Document = "Document";
            public const string SearchTerm = "Search Term";
            public const string SearchOptions = "Search Options";
            public const string Types = "Types To Search - Note If 'All Types' Is Selected Some System Object Types Are Excluded In The Searches";
        }

        [DoNotAllowGridOpen]
        public class SearchTerm
        {
            [GridWidth(400)]
            [RequiredProperty]
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        public enum SearchTermOperator
        {
            Or,
            And
        }
    }
}