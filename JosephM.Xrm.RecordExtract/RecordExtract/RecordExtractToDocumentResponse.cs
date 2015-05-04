using System.Collections.Generic;
using JosephM.Core.Service;
using JosephM.Record.IService;
using JosephM.Xrm.RecordExtract.DocumentWriter;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class RecordExtractToDocumentResponse : ServiceResponseBase<RecordExtractResponseItem>
    {
        public IRecord Record { get; set; }

        private readonly List<ContentBookmark> _bookmarks = new List<ContentBookmark>();

        public IEnumerable<ContentBookmark> Bookmarks
        {
            get { return _bookmarks; }
        }

        public void AddBookmark(ContentBookmark bookmark)
        {
            _bookmarks.Add(bookmark);
        }
    }
}