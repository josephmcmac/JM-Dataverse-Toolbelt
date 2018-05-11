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
    [Group(Sections.Types, true, 30)]
    [DisplayName("Text Search")]
    public class TextSearchRequest : ServiceRequestBase
    {
        public TextSearchRequest()
        {
            SearchAllTypes = true;
            ExcludeEmails = true;
            ExcludePosts = true;
        }

        [DisplayOrder(10)]
        [Group(Sections.Document)]
        [RequiredProperty]
        public DocumentType DocumentFormat { get; set; }

        [DisplayOrder(20)]
        [Group(Sections.Document)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.SearchTerm)]
        [RequiredProperty]
        public string SearchText { get; set; }

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

        private static class Sections
        {
            public const string Document = "Document";
            public const string SearchTerm = "Search Term";
            public const string Types = "Types to Search - note if All Types is selected some system object types are excluded in the searches";
        }
    }
}