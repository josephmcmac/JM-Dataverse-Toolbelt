using JosephM.Application.Application;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Section;
using JosephM.Core.FieldType;
using JosephM.Core.Test;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace JosephM.Application.Desktop.Test
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
                if (section is FieldSectionViewModel fieldSection)
                {
                    foreach (var field in fieldSection.Fields)
                    {
                        PopulateViewModel(field);
                    }
                }
            }
            if (!viewModel.Validate())
            {
                throw new ValidationException($"The Autopopulated Form Did Not Validate:\n{viewModel.GetValidationSummary()}");
            }
            if (viewModel.OnSave != null)
            {
                viewModel.OnSave();
            }
        }

        private static void PopulateViewModel(FieldViewModelBase field)
        {
            if (field.IsEditable)
            {
                if (field is StringFieldViewModel)
                {
                    field.ValueObject = TestConstants.TestingString;
                }
                else if (field is BooleanFieldViewModel)
                {
                    field.ValueObject = true;
                }
                else if (field is IntegerFieldViewModel)
                {
                    field.ValueObject = 1;
                }
                else if (field is FolderFieldViewModel)
                {
                    field.ValueObject = new Folder(TestConstants.TestFolder);
                }
                else if (field is PicklistFieldViewModel picklistFieldViewModel)
                {
                    if (!picklistFieldViewModel.ItemsSource.Any())
                    {
                        throw new NullReferenceException($"No Items In {typeof(PicklistFieldViewModel).Name} To Populate The Value");
                    }
                    field.ValueObject = picklistFieldViewModel.ItemsSource.First();
                }
                else if (field is RecordTypeFieldViewModel recordTypeFieldViewModel)
                {
                    if (!recordTypeFieldViewModel.ItemsSource.Any())
                    {
                        throw new NullReferenceException($"No Items In {typeof(RecordTypeFieldViewModel).Name} To Populate The Value");
                    }
                    if (recordTypeFieldViewModel.ItemsSource.Any(it => it.Key == "contact"))
                    {
                        field.ValueObject = recordTypeFieldViewModel.ItemsSource.First(it => it.Key == "contact");
                    }
                    else
                    {
                        field.ValueObject = recordTypeFieldViewModel.ItemsSource.First();
                    }
                }
                else if (field is RecordFieldFieldViewModel recordFieldFieldViewModel)
                {
                    if (!recordFieldFieldViewModel.ItemsSource.Any())
                    {
                        throw new NullReferenceException($"No Items In {typeof(RecordFieldFieldViewModel).Name} To Populate The Value");
                    }
                    field.ValueObject = recordFieldFieldViewModel.ItemsSource.First();
                }
                else if (field is LookupFieldViewModel lookupFieldViewModel)
                {
                    lookupFieldViewModel.EnteredText = TestConstants.TestingString;
                    lookupFieldViewModel.Search();
                    if (!lookupFieldViewModel.LookupGridViewModel.DynamicGridViewModel.GridRecords.Any())
                    {
                        throw new NullReferenceException($"No Items In {typeof(LookupGridViewModel).Name} To Populate The {typeof(LookupFieldViewModel).Name} Value For Search String {TestConstants.TestingString}");
                    }
                    lookupFieldViewModel.OnRecordSelected(lookupFieldViewModel.LookupGridViewModel.DynamicGridViewModel.GridRecords.First().Record);
                }
                else if (field is EnumerableFieldViewModel enumerableFieldViewModel)
                {
                    enumerableFieldViewModel.AddRow();
                    var rowViewModel = enumerableFieldViewModel.DynamicGridViewModel.GridRecords.First();
                    foreach (var gridField in rowViewModel.FieldViewModels)
                    {
                        PopulateViewModel(gridField);
                    }
                }
                else
                {
                    throw new NotImplementedException($"No Logic Implemented To AutoPopulate The Form For Type {field.GetType().Name}");
                }
            }
        }
    }
}