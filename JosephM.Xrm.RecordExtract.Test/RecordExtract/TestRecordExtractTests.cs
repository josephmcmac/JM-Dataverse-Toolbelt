using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    [TestClass]
    public class TestRecordExtractTests : FakeRecordExtractTests
    {
        [TestMethod]
        public void RecordExtractTestCreateDocumentTest()
        {
            //script out a document from fake data
            var recordService = FakeRecordService.Get();

            const string type = FakeConstants.RecordType;
            var request = new RecordExtractRequest();
            request.SaveToFolder = new Folder(TestingFolder);
            var record = recordService.GetMainRecord();
            request.RecordType = new RecordType(type, recordService.GetDisplayName(type));
            request.RecordLookup = new Lookup(FakeConstants.RecordType, record.Id, "Fake Record");

            var response = new RecordExtractResponse();
            TestRecordExtractService.ExecuteExtention(request, response, new LogController());
        }

        [TestMethod]
        public void RecordExtractTestActivityPartyTest()
        {
            //script out a document from fake data
            var recordService = FakeRecordService.Get();
            var contact = recordService.GetFirst(FakeConstants.FakeContactType, FakeConstants.FakeContactPrimaryField,
                TestingString + " Contact");

            const string type = FakeConstants.FakeContactType;
            var request = new RecordExtractRequest();
            request.SaveToFolder = new Folder(TestingFolder);
            var record = contact;
            request.RecordType = new RecordType(type, recordService.GetDisplayName(type));
            request.RecordLookup = new Lookup(record.Type, record.Id, "Fake Record");

            var response = new RecordExtractResponse();
            TestRecordExtractService.ExecuteExtention(request, response, new LogController());
        }
    }
}