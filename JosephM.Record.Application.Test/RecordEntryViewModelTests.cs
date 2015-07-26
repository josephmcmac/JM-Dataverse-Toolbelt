using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Attributes;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Service;

namespace JosephM.Record.Application.Test
{
    [TestClass]
    public class RecordEntryViewModelTests : RecordApplicationTests
    {
        [TestMethod]
        public void FakeRecordEntryViewModelTest()
        {
            var blah = new FakeRecordEntryViewModel();
        }

        [TestMethod]
        public void RecordEntryViewModelRequiredPropertiesTests()
        {
            var objectToEnter = new TestViewModelValidationObject();
            var viewModel = LoadToObjectEntryViewModel(objectToEnter);
            //check the form not yet valid
            Assert.IsFalse(viewModel.Validate());
            //check the string property validates as required
            var notRequiredStringViewModel = viewModel.GetFieldViewModel("NotRequiredString");
            Assert.IsFalse(notRequiredStringViewModel.HasErrors);
            var requiredStringViewModel = viewModel.GetFieldViewModel("RequiredString");
            Assert.IsTrue(requiredStringViewModel.HasErrors);
            var propertyAttribute = objectToEnter.GetType().GetProperty("RequiredString").GetCustomAttribute<RequiredProperty>();
            Assert.AreEqual(propertyAttribute.GetErrorMessage("RequiredString", objectToEnter), requiredStringViewModel.GetErrorsString());
            //check the subgrid validates as required
            var notRequiredSubGrid = viewModel.GetSubGridViewModel("NotRequiredIEnumerableProperty");
            Assert.IsFalse(notRequiredSubGrid.HasError);
            var requiredSubGrid = viewModel.GetSubGridViewModel("RequiredIEnumerableProperty");
            Assert.IsTrue(requiredSubGrid.HasError);
            var subGridAttribute = objectToEnter.GetType().GetProperty("RequiredIEnumerableProperty").GetCustomAttribute<RequiredProperty>();
            Assert.AreEqual(subGridAttribute.GetErrorMessage("RequiredIEnumerableProperty", objectToEnter), requiredSubGrid.ErrorMessage);

            requiredStringViewModel.ValueObject = "Populate";
            requiredSubGrid.AddRow();
            Assert.IsFalse(viewModel.Validate());
            Assert.IsFalse(requiredStringViewModel.HasErrors);
            Assert.IsFalse(requiredSubGrid.HasError);

            var gridRow1 = requiredSubGrid.GridRecords.First();
            var notRequiredGridFieldViewModel = gridRow1.GetFieldViewModel("NotRequiredString");
            Assert.IsFalse(notRequiredGridFieldViewModel.HasErrors);
            var requiredGridFieldViewModel = gridRow1.GetFieldViewModel("RequiredString");
            Assert.IsTrue(requiredGridFieldViewModel.HasErrors);
            var subGridRowObject = objectToEnter.RequiredIEnumerableProperty.First();
            var subGridFieldAttribute = subGridRowObject.GetType().GetProperty("RequiredString").GetCustomAttribute<RequiredProperty>();
            Assert.AreEqual(subGridFieldAttribute.GetErrorMessage("RequiredString", subGridRowObject), requiredGridFieldViewModel.GetErrorsString());

            requiredGridFieldViewModel.ValueObject = "Something";
            Assert.IsTrue(viewModel.Validate());
        }
    }
}