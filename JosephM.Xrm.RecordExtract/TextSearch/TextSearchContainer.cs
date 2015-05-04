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
        public List<IRecord> NameMatches { get; private set; }
        public Section Section { get; private set; }

        public TextSearchContainer(TextSearchRequest request, TextSearchResponse response, LogController controller,
            Section section)
        {
            Request = request;
            Response = response;
            Controller = controller;
            NameMatches = new List<IRecord>();
            Section = section;
        }

        public void AddNameMatch(IRecord record)
        {
            NameMatches.Add(record);
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

        public IEnumerable<string> GetRecordTypesWithNameMatch()
        {
            return NameMatches != null
                ? NameMatches.Select(r => r.Type).Distinct()
                : new string[] {};
        }
    }
}