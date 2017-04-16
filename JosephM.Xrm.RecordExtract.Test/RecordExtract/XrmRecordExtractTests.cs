using JosephM.Application.ViewModel.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.Test;
using JosephM.Xrm.RecordExtract.DocumentWriter;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    [TestClass]
    public class XrmRecordExtractTests : XrmRecordTest
    {
        [TestMethod]
        public void RecordExtractXrmCreateDocumentTest()
        {
            PrepareTests();

            var testReportRecord = XrmRecordService.GetFirst(Entities.contact);
            //script out a document from fake data
            var recordService = FakeRecordService.Get();

            const string type = FakeConstants.RecordType;
            var request = new RecordExtractRequest();
            request.SaveToFolder = new Folder(TestingFolder);
            request.RecordType = new RecordType(type, recordService.GetDisplayName(type));
            request.RecordLookup = new Lookup(testReportRecord.Type, testReportRecord.Id, "Test Report");

            var response = new RecordExtractResponse();
            XrmRecordExtractService.ExecuteExtention(request, response, new LogController());
        }

        private XrmRecordExtractService XrmRecordExtractService
        {
            get
            {
                return new XrmRecordExtractService(XrmRecordService, new DocumentWriter.DocumentWriter());
            }
        }
    }
}