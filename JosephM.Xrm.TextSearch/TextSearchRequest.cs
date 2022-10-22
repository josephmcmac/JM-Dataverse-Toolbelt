using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.Xrm.TextSearch
{
    [DisplayName("Text Search")]
    [Instruction("String fields will be searched for matches to search text, then a report will detail matches. Matching records may be edited individually, or using a bulk replace function")]
    [Group(Sections.SearchTerms, Group.DisplayLayoutEnum.VerticalCentered, order: 10, displayLabel: false)]
    [Group(Sections.SearchOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    [Group(Sections.TypeOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30, displayLabel: false)]
    [Group(Sections.Types, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40, displayLabel: false)]
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

        [DisplayOrder(10)]
        [Group(Sections.SearchTerms)]
        [RequiredProperty]
        public IEnumerable<SearchTerm> SearchTerms { get; set; }

        [GridWidth(75)]
        [DisplayOrder(20)]
        [Group(Sections.SearchOptions)]
        [RequiredProperty]
        public SearchTermOperator Operator { get; set; }

        [GridWidth(30)]
        [DisplayOrder(30)]
        [Group(Sections.SearchOptions)]
        public bool StripHtmlTagsPriorToSearch { get; set; }

        [DisplayOrder(40)]
        [Group(Sections.SearchOptions)]
        [PropertyInContextByPropertyValue(nameof(StripHtmlTagsPriorToSearch), true)]
        public IEnumerable<RecordFieldSetting> CustomHtmlFields { get; set; }

        [GridWidth(100)]
        [DisplayOrder(50)]
        [Group(Sections.TypeOptions)]
        public bool SearchAllTypes { get; set; }

        [GridWidth(100)]
        [DisplayOrder(60)]
        [Group(Sections.TypeOptions)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), true)]
        public bool ExcludeEmails { get; set; }

        [GridWidth(100)]
        [DisplayOrder(70)]
        [Group(Sections.TypeOptions)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), true)]
        public bool ExcludePosts { get; set; }

        [DisplayOrder(80)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), false)]

        public IEnumerable<TypeToSearch> TypesToSearch { get; set; }

        [DisplayOrder(90)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(SearchAllTypes), true)]
        public IEnumerable<RecordTypeSetting> OtherExclusions { get; set; }

        [DisplayOrder(100)]
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
            public const string SearchTerms = "Search Terms";
            public const string SearchOptions = "Search Options";
            public const string TypeOptions = "Type Options";
            public const string Types = "Types to Search";
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