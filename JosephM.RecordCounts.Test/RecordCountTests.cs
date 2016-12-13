using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Prism.XrmModule.Test;
using JosephM.Core.Utility;
using System.Linq;
using JosephM.RecordCounts.Exporter;
using JosephM.Core.FieldType;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Xrm.Schema;

namespace JosephM.RecordCounts.Test
{
    [TestClass]
    public class RecordCountTests : XrmModuleTest
    {
        [TestMethod]
        public void RecordCountTest()
        {
            var accountLabel = XrmService.GetEntityDisplayName(Entities.account);
            DeleteAll(Entities.account);
            var account1 = CreateAccount();
            var account2 = CreateAccount();
            var account3 = CreateAccount();
            XrmService.Assign(account3, NotCurrentUserId);

            //okay script through generation of various requests

            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<RecordCountsModule>();

            FileUtility.DeleteFiles(TestingFolder);
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            var request = new RecordCountsRequest();
            request.AllRecordTypes = true;
            request.GroupCountsByOwner = false;
            request.SaveToFolder = new Folder(TestingFolder);

            //verify dialog processes
            testApplication.NavigateAndProcessDialog<RecordCountsModule, RecordCountsDialog>(request);
            Assert.AreEqual(1, FileUtility.GetFiles(TestingFolder).Count());
            //verify response counts
            var service = new RecordCountsService(XrmRecordService);
            var response = service.Execute(request, Controller);

            Assert.IsTrue(response.RecordCounts.Count() > 10);
            Assert.AreEqual(1, response.RecordCounts.Count(r => r.RecordType == accountLabel));
            var accountCount = response.RecordCounts.First(r => r.RecordType == accountLabel);
            Assert.AreEqual(3, accountCount.Count);

            FileUtility.DeleteFiles(TestingFolder);
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            request = new RecordCountsRequest();
            request.AllRecordTypes = true;
            request.GroupCountsByOwner = true;
            request.SaveToFolder = new Folder(TestingFolder);
            //verify dialog processes
            testApplication.NavigateAndProcessDialog<RecordCountsModule, RecordCountsDialog>(request);
            Assert.AreEqual(1, FileUtility.GetFiles(TestingFolder).Count());

            //verify response counts
            service = new RecordCountsService(XrmRecordService);
            response = service.Execute(request, Controller);

            Assert.IsTrue(response.RecordCounts.Count() > 10);
            var accountCounts = response.RecordCounts.Where(r => r.RecordType == accountLabel);
            Assert.AreEqual(2, accountCounts.Count());
            Assert.IsTrue(accountCounts.All(r => r is RecordCountByOwner));
            Assert.AreEqual(2, accountCounts.Cast<RecordCountByOwner>().Select(r => r.Owner).Distinct().Count());
            Assert.AreEqual(1, accountCounts.Where(r => r.Count == 1).Count());
            Assert.AreEqual(1, accountCounts.Where(r => r.Count == 2).Count());

            FileUtility.DeleteFiles(TestingFolder);
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            request.AllRecordTypes = false;
            request.RecordTypes = new[] { new RecordTypeSetting(Entities.account, Entities.account) };
            //verify dialog processes
            testApplication.NavigateAndProcessDialog<RecordCountsModule, RecordCountsDialog>(request);
            Assert.AreEqual(1, FileUtility.GetFiles(TestingFolder).Count());
            //verify response counts
            service = new RecordCountsService(XrmRecordService);
            response = service.Execute(request, Controller);

            Assert.IsTrue(response.RecordCounts.All(r => r.RecordType == accountLabel));
            accountCounts = response.RecordCounts.Where(r => r.RecordType == accountLabel);
            Assert.AreEqual(2, accountCounts.Count());
            Assert.IsTrue(accountCounts.All(r => r is RecordCountByOwner));
            Assert.AreEqual(2, accountCounts.Cast<RecordCountByOwner>().Select(r => r.Owner).Distinct().Count());
            Assert.AreEqual(1, accountCounts.Where(r => r.Count == 1).Count());
            Assert.AreEqual(1, accountCounts.Where(r => r.Count == 2).Count());

            FileUtility.DeleteFiles(TestingFolder);
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            request.AllRecordTypes = true;
            request.OnlyIncludeSelectedOwner = true;
            request.Owner = new Lookup(Entities.systemuser, CurrentUserId.ToString(), "Current User");
            //verify dialog processes
            testApplication.NavigateAndProcessDialog<RecordCountsModule, RecordCountsDialog>(request);
            Assert.AreEqual(1, FileUtility.GetFiles(TestingFolder).Count());
            //verify response counts
            service = new RecordCountsService(XrmRecordService);
            response = service.Execute(request, Controller);

            Assert.IsTrue(response.RecordCounts.Count() > 10);
            Assert.AreEqual(1, response.RecordCounts.Count(r => r.RecordType == accountLabel));
            accountCount = response.RecordCounts.First(r => r.RecordType == accountLabel);
            Assert.IsTrue(accountCount is RecordCountByOwner);
            Assert.AreEqual(2, accountCount.Count);
        }
    }
}
