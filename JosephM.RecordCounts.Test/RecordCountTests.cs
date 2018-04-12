using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.XrmModule.Test;
using JosephM.Xrm.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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

            var request = new RecordCountsRequest();
            request.AllRecordTypes = true;
            request.GroupCountsByOwner = false;

            //verify dialog processes
            var response = testApplication.NavigateAndProcessDialog<RecordCountsModule, RecordCountsDialog, RecordCountsResponse>(request);
            //verify response counts

            Assert.IsTrue(response.RecordCounts.Count() > 10);
            Assert.AreEqual(1, response.RecordCounts.Count(r => r.RecordType == accountLabel));
            var accountCount = response.RecordCounts.First(r => r.RecordType == accountLabel);
            Assert.AreEqual(3, accountCount.Count);

            request = new RecordCountsRequest();
            request.AllRecordTypes = true;
            request.GroupCountsByOwner = true;
            //verify dialog processes
            response = testApplication.NavigateAndProcessDialog<RecordCountsModule, RecordCountsDialog, RecordCountsResponse>(request);

            //verify response counts
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
            response = testApplication.NavigateAndProcessDialog<RecordCountsModule, RecordCountsDialog, RecordCountsResponse>(request);
            //verify response counts
            Assert.IsTrue(response.RecordCounts.All(r => r.RecordType == accountLabel));
            accountCounts = response.RecordCounts.Where(r => r.RecordType == accountLabel);
            Assert.AreEqual(2, accountCounts.Count());
            Assert.IsTrue(accountCounts.All(r => r is RecordCountByOwner));
            Assert.AreEqual(2, accountCounts.Cast<RecordCountByOwner>().Select(r => r.Owner).Distinct().Count());
            Assert.AreEqual(1, accountCounts.Where(r => r.Count == 1).Count());
            Assert.AreEqual(1, accountCounts.Where(r => r.Count == 2).Count());

            request.AllRecordTypes = true;
            request.OnlyIncludeSelectedOwner = true;
            request.Owner = new Lookup(Entities.systemuser, CurrentUserId.ToString(), "Current User");
            //verify dialog processes
            response = testApplication.NavigateAndProcessDialog<RecordCountsModule, RecordCountsDialog, RecordCountsResponse>(request);
            //verify response counts
            var userName = (string)XrmService.LookupField(Entities.systemuser, CurrentUserId, Fields.systemuser_.fullname);
            Assert.IsTrue(response.RecordCounts.Any());
            Assert.IsTrue(response.RecordCounts.All(rc => rc is RecordCountByOwner));
            Assert.IsTrue(response.RecordCounts.Cast<RecordCountByOwner>().All(rc => rc.Owner == userName));
            Assert.AreEqual(1, response.RecordCounts.Count(r => r.RecordType == accountLabel));
            accountCount = response.RecordCounts.First(r => r.RecordType == accountLabel);
            Assert.IsTrue(accountCount is RecordCountByOwner);
            Assert.AreEqual(2, accountCount.Count);
        }
    }
}
