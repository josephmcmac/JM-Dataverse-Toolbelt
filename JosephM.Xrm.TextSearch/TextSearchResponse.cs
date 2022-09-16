using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.TextSearch
{
    public class TextSearchResponse : ServiceResponseBase<TextSearchResponseItem>
    {
        [Hidden]
        public string Folder { get; set; }
        [Hidden]
        public string FileName { get; set; }
        [Hidden]
        public string FileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, FileName); }
        }

        private Dictionary<string, Dictionary<string, List<string>>> Matches { get; set; }

        private List<SummaryItem> _summaryItems = new List<SummaryItem>();

        [Hidden]
        public bool HasSummaryItems { get { return Summary != null && Summary.Any(); } }

        [PropertyInContextByPropertyValue(nameof(HasSummaryItems), true)]
        [DoNotAllowGridOpen]
        [AllowDownload]
        [DisplayName("Search Results")]
        public IEnumerable<SummaryItem> Summary
        {
            get
            {
                return _summaryItems;
            }
        }

        internal void SetMatchDictionary(Dictionary<string, Dictionary<string, List<string>>> matches)
        {
            Matches = matches;
        }

        internal void GenerateSummaryItems(IRecordService service)
        {
            _summaryItems.Clear();
            foreach (var item in Matches.OrderBy(m => service.GetDisplayName(m.Key)))
            {
                var typeSchemaName = item.Key;
                var typeLabel = service.GetDisplayName(typeSchemaName);
                _summaryItems.Add(new SummaryItem(typeSchemaName, typeLabel, "any", "Any Match", item.Value.SelectMany(kv => kv.Value).Distinct().ToArray(), service));
                foreach(var fieldMatch in item.Value.OrderBy(kv => service.GetFieldLabel(kv.Key, typeSchemaName)))
                {
                    _summaryItems.Add(new SummaryItem(typeSchemaName, typeLabel, fieldMatch.Key, service.GetFieldLabel(fieldMatch.Key, typeSchemaName), fieldMatch.Value, service));
                }
            }
        }

        public class SummaryItem
        {
            public SummaryItem(string typeSchemaName, string typeLabel, string matchedFieldSchemaName, string matchedField, IEnumerable<string> ids, IRecordService recordService)
            {
                RecordTypeSchemaName = typeSchemaName;
                RecordType = typeLabel;
                MatchedFieldSchemaName = matchedFieldSchemaName;
                MatchedField = matchedField;
                Ids = ids;
                RecordService = recordService;
            }

            private IRecordService RecordService { get; set; }

            public IRecordService GetRecordService()
            {
                return RecordService;
            }

            public IEnumerable<string> GetIds()
            {
                return Ids;
            }

            [Hidden]
            public string RecordTypeSchemaName { get; set; }

            public string RecordType { get; set; }

            [Hidden]
            public string MatchedFieldSchemaName { get; set; }

            public string MatchedField { get; set; }

            private IEnumerable<string> Ids { get; set; }

            public int NumberOfMatches { get { return Ids.Count(); } }

        }
    }
}