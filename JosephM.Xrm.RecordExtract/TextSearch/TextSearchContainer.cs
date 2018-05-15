using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Log;
using JosephM.Record.IService;
using JosephM.Xrm.RecordExtract.DocumentWriter;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    internal class TextSearchContainer
    {
        public TextSearchRequest Request { get; set; }
        public TextSearchResponse Response { get; set; }
        public LogController Controller { get; set; }
        public Section Section { get; private set; }

        private Dictionary<string, Dictionary<string, List<string>>> Matches { get; set; }

        public TextSearchContainer(TextSearchRequest request, TextSearchResponse response, LogController controller,
            Section section)
        {
            Request = request;
            Response = response;
            Controller = controller;
            Section = section;
            Matches = new Dictionary<string, Dictionary<string, List<string>>>();
            Response.SetMatchDictionary(Matches);
        }

        public void AddMatchedRecord(string matchedField, IRecord record)
        {
            if (!Matches.ContainsKey(record.Type))
                Matches.Add(record.Type, new Dictionary<string, List<string>>());
            if (!Matches[record.Type].ContainsKey(matchedField))
                Matches[record.Type].Add(matchedField, new List<string>());
            Matches[record.Type][matchedField].Add(record.Id);
        }


        private readonly List<ContentBookmark> _bookmarks = new List<ContentBookmark>();

        public IEnumerable<ContentBookmark> Bookmarks
        {
            get { return _bookmarks; }
        }

        public void AddBookmark(ContentBookmark bookmark)
        {
            _bookmarks.Add(bookmark);
        }

        public ContentBookmark AddHeadingWithBookmark(string heading)
        {
            var bookmark = Section.AddHeading1WithBookmark(heading);
            AddBookmark(bookmark);
            return bookmark;
        }
    }
}