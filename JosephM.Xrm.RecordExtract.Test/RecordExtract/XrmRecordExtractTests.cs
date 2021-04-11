using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.Xrm.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            if(testReportRecord == null)
            {
                CreateContact();
                testReportRecord = XrmRecordService.GetFirst(Entities.contact);
            }

            var request = new RecordExtractRequest();
            request.SaveToFolder = new Folder(TestingFolder);
            request.RecordType = new RecordType(testReportRecord.Type, testReportRecord.Type);
            request.RecordLookup = new Lookup(testReportRecord.Type, testReportRecord.Id, "Test Report");

            var response = new RecordExtractResponse();
            XrmRecordExtractService.ExecuteExtention(request, response, CreateServiceRequestController());
            Assert.IsFalse(response.HasError);
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