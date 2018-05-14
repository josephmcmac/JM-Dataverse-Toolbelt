using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Xrm.RecordExtract.DocumentWriter;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [TestClass]
    public class TextSearchTests : FakeRecordExtractTests
    {
        [TestMethod]
        public void TextSearchFakeTextSearchTest()
        {
            var request = new TextSearchRequest()
            {
                DocumentFormat = DocumentType.Rtf,
                SaveToFolder = new Folder(TestingFolder),
                SearchTerms = new[] { new TextSearchRequest.SearchTerm() { Text = TestingString } }
            };
            var response = new TextSearchResponse();
            TestTextSearchService.ExecuteExtention(request, response, Controller);
            if (!response.Success)
                throw new AssertFailedException("Response Contained Error", response.Exception);
            if (response.ResponseItems.Any(r => r.Exception != null))
            {
                var ex = response.ResponseItems.First(r => r.Exception != null).Exception;
                throw new AssertFailedException(ex.DisplayString(), ex);
            }
        }
    }
}