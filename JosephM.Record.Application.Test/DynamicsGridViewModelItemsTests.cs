using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JosephM.Record.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace JosephM.Record.Application.Test
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

            var gridObjects = viewModel.GetSubGridViewModel("EnumerableObjects");
            gridObjects.DynamicGridViewModel.SortIt("Bool");
            Assert.IsFalse(gridObjects.GridRecords.First().GetBooleanFieldFieldViewModel("Bool").Value);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel("Bool").Value);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(2).GetBooleanFieldFieldViewModel("Bool").Value);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(3).GetBooleanFieldFieldViewModel("Bool").Value);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(4).GetBooleanFieldFieldViewModel("Bool").Value);
            gridObjects.DynamicGridViewModel.SortIt("Bool");
            Assert.IsTrue(gridObjects.GridRecords.First().GetBooleanFieldFieldViewModel("Bool").Value);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel("Bool").Value);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(2).GetBooleanFieldFieldViewModel("Bool").Value);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(3).GetBooleanFieldFieldViewModel("Bool").Value);
            Assert.IsFalse(gridObjects.GridRecords.ElementAt(4).GetBooleanFieldFieldViewModel("Bool").Value);
            gridObjects.DynamicGridViewModel.SortIt("Int");
            Assert.IsTrue(gridObjects.GridRecords.First().GetIntegerFieldFieldViewModel("Int").Value == 0);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(1).GetIntegerFieldFieldViewModel("Int").Value == 0);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(2).GetIntegerFieldFieldViewModel("Int").Value == 2);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(3).GetIntegerFieldFieldViewModel("Int").Value == 11);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(4).GetIntegerFieldFieldViewModel("Int").Value == 100);
            gridObjects.DynamicGridViewModel.SortIt("Int");
            Assert.IsTrue(gridObjects.GridRecords.First().GetIntegerFieldFieldViewModel("Int").Value == 100);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(1).GetIntegerFieldFieldViewModel("Int").Value == 11);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(2).GetIntegerFieldFieldViewModel("Int").Value == 2);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(3).GetIntegerFieldFieldViewModel("Int").Value == 0);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(4).GetIntegerFieldFieldViewModel("Int").Value == 0);
            gridObjects.DynamicGridViewModel.SortIt("String");
            Assert.IsNull(gridObjects.GridRecords.First().GetStringFieldFieldViewModel("String").Value);
            Assert.IsNull(gridObjects.GridRecords.ElementAt(1).GetStringFieldFieldViewModel("String").Value);
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(2).GetStringFieldFieldViewModel("String").Value == "1");
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(3).GetStringFieldFieldViewModel("String").Value == "100");
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(4).GetStringFieldFieldViewModel("String").Value == "11");
            gridObjects.DynamicGridViewModel.SortIt("String");
            Assert.IsTrue(gridObjects.GridRecords.First().GetStringFieldFieldViewModel("String").Value == "11");
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(1).GetStringFieldFieldViewModel("String").Value == "100");
            Assert.IsTrue(gridObjects.GridRecords.ElementAt(2).GetStringFieldFieldViewModel("String").Value == "1");
            Assert.IsNull(gridObjects.GridRecords.ElementAt(3).GetStringFieldFieldViewModel("String").Value);
            Assert.IsNull(gridObjects.GridRecords.ElementAt(4).GetStringFieldFieldViewModel("String").Value);
        }
    }
}