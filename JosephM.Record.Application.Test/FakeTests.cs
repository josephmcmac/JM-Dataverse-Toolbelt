using JosephM.Application.ViewModel.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Record.Application.Test
{
    [TestClass]
    public class FakeTests
    {
        [TestMethod]
        public void RecordApplicationTestFakeRecordEntryViewModelTests()
        {
            var viewModel = new FakeRecordEntryViewModel();
            var record = viewModel.FormSectionsAsync;
            Assert.IsNotNull(record);
        }

        [TestMethod]
        public void RecordApplicationTestFakeSubgridViewModelTests()
        {
            var viewModel = new FakeSubGridViewModel();
            var records = viewModel.GridRecords;
            Assert.IsNotNull(records);
        }

        [TestMethod]
        public void RecordApplicationTestFakeLookupViewModelTests()
        {
            var viewModel = new FakeLookupViewModel();
            var fields = viewModel.ValueObject;
            Assert.IsNotNull(fields);
        }
    }
}