using System.Linq;
using System.Reflection;
using JosephM.Application.ViewModel.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Attributes;
using JosephM.Record.Service;
using JosephM.Application.ViewModel.RecordEntry.Form;

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
            var notRequiredStringViewModel = viewModel.GetFieldViewModel(nameof(TestViewModelValidationObject.NotRequiredString));
            Assert.IsFalse(notRequiredStringViewModel.HasErrors);
            var requiredStringViewModel = viewModel.GetFieldViewModel(nameof(TestViewModelValidationObject.RequiredString));
            Assert.IsTrue(requiredStringViewModel.HasErrors);
            var propertyAttribute = objectToEnter.GetType().GetProperty(nameof(TestViewModelValidationObject.RequiredString)).GetCustomAttribute<RequiredProperty>();
            Assert.AreEqual(propertyAttribute.GetErrorMessage("Required String"), requiredStringViewModel.GetErrorsString());
            
            //check the subgrid validates as required
            var notRequiredSubGrid = viewModel.GetSubGridViewModel(nameof(TestViewModelValidationObject.NotRequiredIEnumerableProperty));
            Assert.IsFalse(notRequiredSubGrid.HasError);
            var requiredSubGrid = viewModel.GetSubGridViewModel(nameof(TestViewModelValidationObject.RequiredIEnumerableProperty));
            Assert.IsTrue(requiredSubGrid.HasError);
            var subGridAttribute = objectToEnter.GetType().GetProperty(nameof(TestViewModelValidationObject.RequiredIEnumerableProperty)).GetCustomAttribute<RequiredProperty>();
            Assert.AreEqual(subGridAttribute.GetErrorMessage("Required I Enumerable Property"), requiredSubGrid.ErrorMessage);

            requiredStringViewModel.ValueObject = "Populate";
            requiredSubGrid.AddRow();
            Assert.IsFalse(viewModel.Validate());
            Assert.IsFalse(requiredStringViewModel.HasErrors);
            Assert.IsFalse(requiredSubGrid.HasError);

            var gridRow1 = requiredSubGrid.GridRecords.First();
            var notRequiredGridFieldViewModel = gridRow1.GetFieldViewModel(nameof(TestViewModelValidationObject.NotRequiredString));
            Assert.IsFalse(notRequiredGridFieldViewModel.HasErrors);
            var requiredGridFieldViewModel = gridRow1.GetFieldViewModel(nameof(TestViewModelValidationObject.RequiredString));
            Assert.IsTrue(requiredGridFieldViewModel.HasErrors);
            var subGridRowObject = objectToEnter.RequiredIEnumerableProperty.First();
            var subGridFieldAttribute = subGridRowObject.GetType().GetProperty(nameof(TestViewModelValidationObject.RequiredString)).GetCustomAttribute<RequiredProperty>();
            Assert.AreEqual(subGridFieldAttribute.GetErrorMessage("Required String"), requiredGridFieldViewModel.GetErrorsString());

            requiredGridFieldViewModel.ValueObject = "Something";

            //okay well we also have an enumerable which may be require din the subgrid
            gridRow1.GetBooleanFieldFieldViewModel(nameof(TestViewModelValidationObject.TestEnumerablePropertyObject.RequireRecordsInTheGrid)).Value = true;
            Assert.IsFalse(viewModel.Validate());

            gridRow1.EditRow();

            var childForm = viewModel.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(childForm);
            childForm.LoadFormSections();
            childForm.GetStringFieldFieldViewModel(nameof(TestViewModelValidationObject.TestEnumerablePropertyObject.RequiredString)).Value = "Something";
            var subGrid = childForm.GetSubGridViewModel(nameof(TestViewModelValidationObject.TestEnumerablePropertyObject.RequiredEnumerableInTheGrid));
            subGrid.AddRow();
            subGrid.GridRecords.First().GetStringFieldFieldViewModel(nameof(TestViewModelValidationObject.TestEnumerablePropertyObject.RequiredString)).Value = "Something";
            Assert.IsTrue(childForm.Validate());
            childForm.SaveButtonViewModel.Invoke();

            Assert.IsFalse(viewModel.ChildForms.Any());

            Assert.IsTrue(viewModel.Validate(), viewModel.GetValidationSummary());
        }
    }
}