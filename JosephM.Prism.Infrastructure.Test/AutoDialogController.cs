using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Section;
using JosephM.Core.FieldType;
using JosephM.Core.Test;

namespace JosephM.Record.Application.Fakes
{
    public class AutoDialogController : FakeDialogController
    {
        public AutoDialogController(IApplicationController applicationController)
            : base(applicationController)
        {

        }

        protected override void ProcessRecordEntryForm(RecordEntryFormViewModel viewModel)
        {
            base.ProcessRecordEntryForm(viewModel);
            foreach (var section in viewModel.FormSectionsAsync)
            {
                if (section is FieldSectionViewModel)
                {
                    var fieldSection = (FieldSectionViewModel)section;
                    foreach (var field in fieldSection.Fields)
                    {
                        PopulateViewModel(field);
                    }
                }
                if (section is GridSectionViewModel)
                {
                    var gridSection = (GridSectionViewModel)section;
                    gridSection.AddRow();
                    var rowViewModel = gridSection.DynamicGridViewModel.GridRecords.First();
                    foreach (var field in rowViewModel.FieldViewModels)
                    {
                        PopulateViewModel(field);
                    }
                }
            }
            if (!viewModel.Validate())
            {
                throw new ValidationException(string.Format("The Autopopulated Form Did Not Validate:\n{0}", viewModel.GetValidationSummary()));
            }
            if (viewModel.OnSave != null)
                viewModel.OnSave();
        }

        private static void PopulateViewModel(FieldViewModelBase field)
        {
            if (field.IsEditable)
            {
                if (field is StringFieldViewModel)
                    field.ValueObject = TestConstants.TestingString;
                else if (field is BooleanFieldViewModel)
                    field.ValueObject = true;
                else if (field is IntegerFieldViewModel)
                    field.ValueObject = 1;
                else if (field is FolderFieldViewModel)
                    field.ValueObject = new Folder(TestConstants.TestFolder);
                else if (field is PicklistFieldViewModel)
                {
                    var typed = (PicklistFieldViewModel)field;
                    if (!typed.ItemsSource.Any())
                        throw new NullReferenceException(string.Format("No Items In {0} To Populate The Value",
                            typeof(PicklistFieldViewModel).Name));
                    field.ValueObject = typed.ItemsSource.First();
                }
                else if (field is RecordTypeFieldViewModel)
                {
                    var typed = (RecordTypeFieldViewModel)field;
                    if (!typed.ItemsSource.Any())
                        throw new NullReferenceException(string.Format("No Items In {0} To Populate The Value",
                            typeof(RecordTypeFieldViewModel).Name));
                    if (typed.ItemsSource.Any(it => it.Key == "contact"))
                        field.ValueObject = typed.ItemsSource.First(it => it.Key == "contact");
                    else
                        field.ValueObject = typed.ItemsSource.First();
                }
                else if (field is RecordFieldFieldViewModel)
                {
                    var typed = (RecordFieldFieldViewModel)field;
                    if (!typed.ItemsSource.Any())
                        throw new NullReferenceException(string.Format("No Items In {0} To Populate The Value",
                            typeof(RecordFieldFieldViewModel).Name));
                    field.ValueObject = typed.ItemsSource.First();
                }
                else if (field is LookupFieldViewModel)
                {
                    var typed = (LookupFieldViewModel)field;
                    typed.EnteredText = TestConstants.TestingString;
                    typed.Search();
                    if (!typed.LookupGridViewModel.DynamicGridViewModel.GridRecords.Any())
                        throw new NullReferenceException(
                            string.Format("No Items In {0} To Populate The {1} Value For Search String {2}",
                                typeof(LookupGridViewModel).Name, typeof(LookupFieldViewModel).Name,
                                TestConstants.TestingString));
                    typed.OnRecordSelected(typed.LookupGridViewModel.DynamicGridViewModel.GridRecords.First().Record);
                }
                else
                    throw new NotImplementedException(
                        string.Format("No Logic Implemented To AutoPopulate The Form For Type {0}",
                            field.GetType().Name));
            }
        }
    }
}