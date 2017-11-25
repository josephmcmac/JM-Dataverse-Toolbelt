using System.Linq;
using System.Reflection;
using JosephM.Application.ViewModel.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Attributes;
using JosephM.Record.Service;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Metadata;
using System;
using System.Collections.Generic;
using JosephM.Core.FieldType;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Core.Test;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.Application;

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

            var prismSettingsManager = new PrismSettingsManager(applicationController);

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
            recordFieldMultiSelectField.MultiSelectsVisible = true;
            recordFieldMultiSelectField.DynamicGridViewModel.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
            recordFieldMultiSelectField.DynamicGridViewModel.GridRecords.ElementAt(2).GetBooleanFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
            Assert.IsNotNull(recordFieldMultiSelectField.DisplayLabel);
            Assert.AreEqual(2, recordFieldMultiSelectField.Value.Count());
            Assert.IsTrue(recordFieldMultiSelectField.Value.Any(p => p.Value == recordFieldMultiSelectField.DynamicGridViewModel.GridRecords.ElementAt(1).GetStringFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Item)).Value));
            Assert.IsTrue(recordFieldMultiSelectField.Value.Any(p => p.Value == recordFieldMultiSelectField.DynamicGridViewModel.GridRecords.ElementAt(2).GetStringFieldFieldViewModel(nameof(RecordFieldMultiSelectFieldViewModel.SelectablePicklistOption.Item)).Value));

            var lookupField = entryViewModel.GetLookupFieldFieldViewModel(nameof(TestAllFieldTypes.LookupField));
            lookupField.Search();
            lookupField.OnRecordSelected(lookupField.LookupGridViewModel.DynamicGridViewModel.GridRecords.First().Record);
            Assert.IsNotNull(lookupField.Value);
            Assert.AreEqual(lookupField.Value.Name, lookupField.EnteredText);

            var multiSelectField = entryViewModel.GetFieldViewModel<PicklistMultiSelectFieldViewModel>(nameof(TestAllFieldTypes.PicklistMultiSelectField));
            multiSelectField.MultiSelectsVisible = true;
            multiSelectField.DynamicGridViewModel.GridRecords.ElementAt(1).GetBooleanFieldFieldViewModel(nameof(PicklistMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
            multiSelectField.DynamicGridViewModel.GridRecords.ElementAt(2).GetBooleanFieldFieldViewModel(nameof(PicklistMultiSelectFieldViewModel.SelectablePicklistOption.Select)).Value = true;
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
            objectFieldViewModel.SelectedItem = objectFieldViewModel.ItemsSource.First();
            Assert.IsNotNull(objectFieldViewModel.Value);
        }
    }
}