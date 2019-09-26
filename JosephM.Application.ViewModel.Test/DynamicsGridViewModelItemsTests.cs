using JosephM.Record.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;


namespace JosephM.Application.ViewModel.Test
{
    [TestClass]
    public class DynamicsGridViewModelItemsTests : RecordApplicationTests
    {
        [TestMethod]
        public void DynamicsGridViewModelItemsSortTest()
        {
            var testObject = new TestClass();

            testObject.Bool = true;
            testObject.Int = 1;
            testObject.String = "12345";
            testObject.EnumerableObjects = new List<TestClass.NestedClass>
            {
                new TestClass.NestedClass() {},
                new TestClass.NestedClass() {Bool = true, Int = 2, String = "1"},
                new TestClass.NestedClass() {Bool = false, Int = 11, String = "11"},
                new TestClass.NestedClass() {Bool = true, Int = 100, String = "100"},
                new TestClass.NestedClass() {}
            };

            var viewModel = LoadToObjectEntryViewModel(testObject);

            var gridObjects = viewModel.GetEnumerableFieldViewModel(nameof(TestClass.EnumerableObjects));
            gridObjects.DynamicGridViewModel.SortIt(nameof(TestClass.NestedClass.Bool));
            Assert.IsFalse(gridObjects.GridRecords.First().GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(2).GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(3).GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(4).GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            gridObjects.DynamicGridViewModel.SortIt(nameof(TestClass.NestedClass.Bool));
            Assert.IsTrue(gridObjects.GridRecords.First().GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(2).GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(3).GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(4).GetBooleanFieldFieldViewModel(nameof(TestClass.NestedClass.Bool)).Value ?? false);
            gridObjects.DynamicGridViewModel.SortIt(nameof(TestClass.NestedClass.Int));
            Assert.IsTrue(gridObjects.GridRecords.First().GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 0);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(1).GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 0);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(2).GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 2);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(3).GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 11);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(4).GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 100);
            gridObjects.DynamicGridViewModel.SortIt(nameof(TestClass.NestedClass.Int));
            Assert.IsTrue(gridObjects.GridRecords.First().GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 100);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(1).GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 11);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(2).GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 2);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(3).GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 0);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(4).GetIntegerFieldFieldViewModel(nameof(TestClass.NestedClass.Int)).Value == 0);
            gridObjects.DynamicGridViewModel.SortIt(nameof(TestClass.NestedClass.String));
            Assert.IsNull(gridObjects.GridRecords.First().GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value);
            Assert.IsNull(gridObjects.GridRecords.ElementAt(1).GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(2).GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value == "1");
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(3).GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value == "100");
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(4).GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value == "11");
            gridObjects.DynamicGridViewModel.SortIt(nameof(TestClass.NestedClass.String));
            Assert.IsTrue(gridObjects.GridRecords.First().GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value == "11");
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(1).GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value == "100");
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(2).GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value == "1");
            Assert.IsNull(gridObjects.GridRecords.ElementAt(3).GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value);
            Assert.IsNull(gridObjects.GridRecords.ElementAt(4).GetStringFieldFieldViewModel(nameof(TestClass.NestedClass.String)).Value);
        }
    }
}