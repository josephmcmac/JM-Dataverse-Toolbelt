using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Record.IService;
using JosephM.Xrm.RecordExtract.DocumentWriter;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    internal class RecordExtractContainer
    {
        public RecordExtractToDocumentRequest Request { get; private set; }
        public RecordExtractToDocumentResponse Response { get; private set; }

        public LogController Controller
        {
            get { return Request.Controller; }
        }

        public Section Section
        {
            get { return Request.Section; }
        }

        public IRecord RecordToExtract { get; set; }

        public string RecordToExtractId
        {
            get { return Lookup.Id; }
        }

        public string RecordToExtractType
        {
            get { return Lookup.RecordType; }
        }

        public Lookup Lookup
        {
            get { return Request.RecordLookup; }
        }

        public RecordExtractContainer(RecordExtractToDocumentRequest request)
        {
            Request = request;
            Response = new RecordExtractToDocumentResponse();
        }

        public ContentBookmark AddBookmark(string displayLabel)
        {
            var bookmark = Section.AddBookmark(displayLabel);
            Response.AddBookmark(bookmark);
            return bookmark;
        }

        public void AddBookmark(ContentBookmark bookmark)
        {
            Response.AddBookmark(bookmark);
        }
    }
}