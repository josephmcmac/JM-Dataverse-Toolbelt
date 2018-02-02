using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using System;
using System.Collections.Generic;
using System.Threading;

namespace JosephM.Application.ViewModel.Attributes
{
    public abstract class BulkAddMultiSelectFunction : BulkAddFunction
    {
        public abstract IEnumerable<PicklistOption> GetSelectionOptions(RecordEntryViewModelBase recordForm, string subGridReference);

        public override Action GetCustomFunction(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            return () =>
            {
                recordForm.LoadingViewModel.IsLoading = true;
                recordForm.DoOnAsynchThread(() =>
                {
                    try
                    {
                        Thread.Sleep(100);
                        Load(recordForm, subGridReference);
                    }
                    catch (Exception ex)
                    {
                        recordForm.ApplicationController.ThrowException(ex);
                        recordForm.LoadingViewModel.IsLoading = false;
                    }
                });
            };
        }

        public void Load(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            recordForm.DoOnMainThread(() =>
            {
                try
                {
                    var mainFormInContext = recordForm;
                    if (recordForm is GridRowViewModel)
                        mainFormInContext = recordForm.ParentForm;

                    //okay i need to load a dialog
                    //displaying a grid of the selectable options with a checkbox
                    Action<IEnumerable<PicklistOption>> onSave = (selectedOptions) =>
                    {
                        //copy into the
                        mainFormInContext.LoadingViewModel.IsLoading = true;
                        try
                        {
                            AddSelectedItems(selectedOptions, recordForm, subGridReference);
                            mainFormInContext.ClearChildForm();
                        }
                        catch (Exception ex)
                        {
                            mainFormInContext.ApplicationController.ThrowException(ex);
                        }
                        finally
                        {
                            mainFormInContext.LoadingViewModel.IsLoading = false;
                        }
                    };
                    var picklistOptions = GetSelectionOptions(recordForm, subGridReference);
                    var childForm = new MultiSelectDialogViewModel<PicklistOption>(picklistOptions, null, onSave, () => mainFormInContext.ClearChildForm(), mainFormInContext.ApplicationController);
                    mainFormInContext.LoadChildForm(childForm);
                }
                catch (Exception ex)
                {
                    recordForm.ApplicationController.ThrowException(ex);
                }
                finally
                {
                    recordForm.LoadingViewModel.IsLoading = false;
                }
            });
        }

        public void AddSelectedItems(IEnumerable<PicklistOption> selectedItems, RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var gridField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);
            var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;

            foreach (var item in selectedItems)
            {
                var newRecord = recordForm.RecordService.NewRecord(GetEnumeratedType(recordForm, subGridReference).AssemblyQualifiedName);
                var newPicklistOption = TargetPropertyType.CreateFromParameterlessConstructor();
                newPicklistOption.SetPropertyValue(nameof(PicklistOption.Key), item.Key);
                newPicklistOption.SetPropertyValue(nameof(PicklistOption.Value), item.Value);
                newRecord.SetField(targetPropertyname, newPicklistOption, recordForm.RecordService);

                InsertNewItem(recordForm, subGridReference, newRecord);
            }
        }
    }
}