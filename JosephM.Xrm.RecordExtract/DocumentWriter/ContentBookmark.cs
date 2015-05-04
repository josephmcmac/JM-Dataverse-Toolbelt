using System;
using System.Collections.Generic;

namespace JosephM.Xrm.RecordExtract.DocumentWriter
{
    public class ContentBookmark
    {
        public ContentBookmark(string displayLabel)
        {
            DisplayLabel = displayLabel;
            Key = Guid.NewGuid().ToString();
        }

        public string Key { get; private set; }
        public string DisplayLabel { get; private set; }

        private readonly List<ContentBookmark> _childBookmarks = new List<ContentBookmark>();

        public IEnumerable<ContentBookmark> ChildBookmarks
        {
            get { return _childBookmarks; }
        }

        public void AddChildBookmark(ContentBookmark bookmark)
        {
            _childBookmarks.Add(bookmark);
        }

        public void AddChildBookmarks(IEnumerable<ContentBookmark> bookmarks)
        {
            _childBookmarks.AddRange(bookmarks);
        }
    }
}