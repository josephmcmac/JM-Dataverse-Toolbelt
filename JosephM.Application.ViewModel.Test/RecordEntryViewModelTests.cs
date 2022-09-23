using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace JosephM.Application.ViewModel.Test
{
    [TestClass]
    public class RecordEntryViewModelTests : RecordApplicationTests
    {
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
            var notRequiredSubGrid = viewModel.GetEnumerableFieldViewModel(nameof(TestViewModelValidationObject.NotRequiredIEnumerableProperty));
            Assert.IsFalse(notRequiredSubGrid.HasError);
            var requiredSubGrid = viewModel.GetEnumerableFieldViewModel(nameof(TestViewModelValidationObject.RequiredIEnumerableProperty));
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

            //okay well we also have an enumerable which may be required in the subgrid
            gridRow1.GetBooleanFieldFieldViewModel(nameof(TestViewModelValidationObject.TestEnumerablePropertyObject.RequireRecordsInTheGrid)).Value = true;
            Assert.IsFalse(viewModel.Validate());

            gridRow1.EditRow();

            var childForm = viewModel.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(childForm);
            childForm.LoadFormSections();
            childForm.GetStringFieldFieldViewModel(nameof(TestViewModelValidationObject.TestEnumerablePropertyObject.RequiredString)).Value = "Something";
            var subGrid = childForm.GetEnumerableFieldViewModel(nameof(TestViewModelValidationObject.TestEnumerablePropertyObject.RequiredEnumerableInTheGrid));
            subGrid.AddRow();
            subGrid.GridRecords.First().GetStringFieldFieldViewModel(nameof(TestViewModelValidationObject.TestEnumerablePropertyObject.RequiredString)).Value = "Something";
            Assert.IsTrue(childForm.Validate());
            childForm.SaveButtonViewModel.Invoke();

            Assert.IsFalse(viewModel.ChildForms.Any());

            Assert.IsTrue(viewModel.Validate(), viewModel.GetValidationSummary());
        }

        /// <summary>
        /// this script verifies view model part for a date field view model
        /// </summary>
        [TestMethod]
        public void RecordEntryViewModelTestDateParts()
        {
            var testObject = new TestDatePropertyViewModel();
            var viewModel = LoadToObjectEntryViewModel(testObject);
            var dateViewmodel = viewModel.GetFieldViewModel<DateFieldViewModel>(nameof(TestDatePropertyViewModel.DateProperty));
            Assert.IsNull(dateViewmodel.ValueObject);
            Assert.IsFalse(dateViewmodel.Value.HasValue);
            Assert.IsFalse(dateViewmodel.SelectedDate.HasValue);
            Assert.IsNull(dateViewmodel.SelectedHour);
            Assert.IsNull(dateViewmodel.SelectedMinute);
            Assert.IsNull(dateViewmodel.SelectedAmPm);

            var dayZero = new DateTime(1980, 11, 15, 0, 0, 0, DateTimeKind.Unspecified);
            dateViewmodel.SelectedDate = dayZero;
            Assert.IsTrue(dateViewmodel.ValueObject is DateTime);
            Assert.AreEqual(dayZero, (DateTime)dateViewmodel.ValueObject);
            Assert.IsTrue(dateViewmodel.SelectedDate.HasValue);
            Assert.AreEqual(dayZero, (DateTime)dateViewmodel.SelectedDate);
            Assert.AreEqual("12", dateViewmodel.SelectedHour?.Key);
            Assert.AreEqual("0", dateViewmodel.SelectedMinute?.Key);
            Assert.AreEqual("AM", dateViewmodel.SelectedAmPm?.Key);

            dateViewmodel.SelectedAmPm = dateViewmodel.AmPmOptions.First(p => p.Key == "PM");
            Assert.AreEqual(new DateTime(1980, 11, 15, 12, 0, 0, DateTimeKind.Unspecified), (DateTime)dateViewmodel.ValueObject);

            dateViewmodel.SelectedMinute = dateViewmodel.MinuteOptions.First(p => p.Key == "30");
            Assert.AreEqual(new DateTime(1980, 11, 15, 12, 30, 0, DateTimeKind.Unspecified), (DateTime)dateViewmodel.ValueObject);

            dateViewmodel.SelectedHour = dateViewmodel.HourOptions.First(p => p.Key == "1");
            Assert.AreEqual(new DateTime(1980, 11, 15, 13, 30, 0, DateTimeKind.Unspecified), (DateTime)dateViewmodel.ValueObject);

            dateViewmodel.SelectedAmPm = dateViewmodel.AmPmOptions.First(p => p.Key == "AM");
            Assert.AreEqual(new DateTime(1980, 11, 15, 1, 30, 0, DateTimeKind.Unspecified), (DateTime)dateViewmodel.ValueObject);

            dateViewmodel.SelectedDate = null;
            Assert.IsNull(dateViewmodel.ValueObject);
            Assert.IsFalse(dateViewmodel.Value.HasValue);
            Assert.IsFalse(dateViewmodel.SelectedDate.HasValue);
            Assert.IsNull(dateViewmodel.SelectedHour);
            Assert.IsNull(dateViewmodel.SelectedMinute);
            Assert.IsNull(dateViewmodel.SelectedAmPm);

            testObject = new TestDatePropertyViewModel();
            var utcTime = DateTime.UtcNow;
            var localTime = utcTime.ToLocalTime();
            testObject.DateProperty = utcTime;
            viewModel = LoadToObjectEntryViewModel(testObject);
            dateViewmodel = viewModel.GetFieldViewModel<DateFieldViewModel>(nameof(TestDatePropertyViewModel.DateProperty));
            Assert.IsTrue(dateViewmodel.Value.HasValue);
            Assert.AreNotEqual(utcTime, dateViewmodel.Value.Value);
            Assert.AreEqual(localTime, dateViewmodel.Value.Value);
            Assert.AreEqual(localTime, (DateTime)dateViewmodel.SelectedDate);
        }

        /// <summary>
        /// this script creates a form for an object cointaining all the field typrs and verifies they load, populate and save
        /// </summary>
        [TestMethod]
        public void RecordEntryViewModelTestAllFieldTypes()
        {
            var applicationController = new FakeApplicationController();
            var settingsObject = new SettingsTestAllFieldTypes
            {
                SavedInstances = new[]
                 {
                     new TestAllFieldTypes()
                     {
                         StringField = "Foo"
                     }
                 }
            };
            applicationController.RegisterInstance(typeof(SettingsTestAllFieldTypes), settingsObject);

            //create the form
            var testObject = new TestAllFieldTypes();

            var lookupService = FakeRecordService.Get();
            var formController = FormController.CreateForObject(testObject, applicationController, lookupService);
            var entryViewModel = new ObjectEntryViewModel(() => { }, () => { }, testObject, formController);

            //populate all the fields
            entryViewModel.LoadFormSections();
            PopulateRecordEntry(entryViewModel, populateSubgrids: true);

            //save the record
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.SaveButtonViewModel.Invoke();
        }

        private void PopulateRecordEntry(RecordEntryViewModelBase entryViewModel, bool populateSubgrids = true)
        {
            var mainForminContent = entryViewModel.ParentForm ?? entryViewModel;

            entryViewModel.GetFieldViewModel<BigIntFieldViewModel>(nameof(TestAllFieldTypes.BigIntField)).Value = 100;
            entryViewModel.GetBooleanFieldFieldViewModel(nameof(TestAllFieldTypes.BooleanField)).Value = true;
            entryViewModel.GetFieldViewModel<DateFieldViewModel>(nameof(TestAllFieldTypes.DateField)).Value = new DateTime(1990, 11, 15);
            entryViewModel.GetFieldViewModel<DecimalFieldViewModel>(nameof(TestAllFieldTypes.DecimalField)).Value = 200;
            entryViewModel.GetFieldViewModel<DoubleFieldViewModel>(nameof(TestAllFieldTypes.DoubleField)).Value = 300;
            entryViewModel.GetFieldViewModel<FileRefFieldViewModel>(nameof(TestAllFieldTypes.FileField)).Value = new FileReference(TestConstants.TestFolder);
            entryViewModel.GetFieldViewModel<FolderFieldViewModel>(nameof(TestAllFieldTypes.FolderField)).Value = new Folder(TestConstants.TestFolder);
            entryViewModel.GetIntegerFieldFieldViewModel(nameof(TestAllFieldTypes.Integerield)).Value = 400;
            entryViewModel.GetFieldViewModel<MoneyFieldViewModel>(nameof(TestAllFieldTypes.MoneyField)).Value = 500;
            entryViewModel.GetFieldViewModel<PasswordFieldViewModel>(nameof(TestAllFieldTypes.PasswordField)).Value = new Password("Password");
            entryViewModel.GetPicklistFieldFieldViewModel(nameof(TestAllFieldTypes.PicklistField)).Value = PicklistOption.EnumToPicklistOption(TestEnum.Option2);
            entryViewModel.GetStringFieldFieldViewModel(nameof(TestAllFieldTypes.StringField)).Value = "Something";
            entryViewModel.GetFieldViewModel<UrlFieldViewModel>(nameof(TestAllFieldTypes.UrlField)).Value = new Url("http://google.com", "Google");

            entryViewModel.GetRecordTypeFieldViewModel(nameof(TestAllFieldTypes.RecordTypeField)).Value = entryViewModel.GetRecordTypeFieldViewModel(nameof(TestAllFieldTypes.RecordTypeField)).ItemsSource.First();
            entryViewModel.GetRecordFieldFieldViewModel(nameof(TestAllFieldTypes.RecordFieldField)).Value = entryViewModel.GetRecordFieldFieldViewModel(nameof(TestAllFieldTypes.RecordFieldField)).ItemsSource.First();

            var recordFieldMultiSelectField = entryViewModel.GetFieldViewModel<RecordFieldMultiSelectFieldViewModel>(nameof(TestAllFieldTypes.RecordFieldMultiSelectField));
            recordFieldMultiSelectField.EditAction();
            //multiselection is done in a child dialog so select several and invoke save
            Assert.IsTrue(mainForminContent.ChildForms.Any());
            var multiSelectFieldEntry = mainForminContent.ChildForms.First() as MultiSelectDialogViewModel<RecordField>;
            multiSelectFieldEntry.ItemsSource.ElementAt(1).Select = true;
            multiSelectFieldEntry.ItemsSource.ElementAt(2).Select = true;
            multiSelectFieldEntry.ApplyButtonViewModel.Invoke();

            Assert.IsFalse(mainForminContent.ChildForms.Any());
            //verify values selected have been applied to the multiselect field
            Assert.IsNotNull(recordFieldMultiSelectField.DisplayLabel);
            Assert.AreEqual(2, recordFieldMultiSelectField.Value.Count());
            Assert.IsTrue(recordFieldMultiSelectField.Value.Any(p => p.Value == multiSelectFieldEntry.ItemsSource.ElementAt(1).Item));
            Assert.IsTrue(recordFieldMultiSelectField.Value.Any(p => p.Value == multiSelectFieldEntry.ItemsSource.ElementAt(1).Item));

            var lookupField = entryViewModel.GetLookupFieldFieldViewModel(nameof(TestAllFieldTypes.LookupField));
            lookupField.Search();
            lookupField.OnRecordSelected(lookupField.LookupGridViewModel.DynamicGridViewModel.GridRecords.First().Record);
            Assert.IsNotNull(lookupField.Value);
            Assert.AreEqual(lookupField.Value.Name, lookupField.EnteredText);

            var multiSelectField = entryViewModel.GetFieldViewModel<PicklistMultiSelectFieldViewModel>(nameof(TestAllFieldTypes.PicklistMultiSelectField));
            multiSelectField.EditAction();
            //multiselection is done in a child form so select several and invoke save
            Assert.IsTrue(mainForminContent.ChildForms.Any());
            var multiSelectOptionEntry = mainForminContent.ChildForms.First() as MultiSelectDialogViewModel<PicklistOption>;
            multiSelectOptionEntry.ItemsSource.ElementAt(1).Select = true;
            multiSelectOptionEntry.ItemsSource.ElementAt(2).Select = true;
            multiSelectOptionEntry.ApplyButtonViewModel.Invoke();
            Assert.IsFalse(mainForminContent.ChildForms.Any());
            //verify values selected have been applied to the multiselect field
            Assert.IsNotNull(multiSelectField.DisplayLabel);
            Assert.AreEqual(2, multiSelectField.Value.Count());
            Assert.IsTrue(multiSelectField.Value.Any(p => p == PicklistOption.EnumToPicklistOption(TestEnum.Option2)));
            Assert.IsTrue(multiSelectField.Value.Any(p => p == PicklistOption.EnumToPicklistOption(TestEnum.Option3)));

            if (entryViewModel is RecordEntryFormViewModel && populateSubgrids)
            {
                var gridField = ((RecordEntryFormViewModel)entryViewModel).GetEnumerableFieldViewModel(nameof(TestAllFieldTypes.EnumerableField));
                gridField.AddRow();
                var row = gridField.GridRecords.First();
                PopulateRecordEntry(row, populateSubgrids: false);
                gridField.AddRow();
                row = gridField.GridRecords.First();
                PopulateRecordEntry(row, populateSubgrids: false);
            }

            var objectFieldViewModel = entryViewModel.GetObjectFieldFieldViewModel(nameof(TestAllFieldTypes.ObjectField));
            objectFieldViewModel.SelectedItem = objectFieldViewModel.ItemsSource.First(r => r.Record != null);
            Assert.IsNotNull(objectFieldViewModel.Value);
        }
    }
}