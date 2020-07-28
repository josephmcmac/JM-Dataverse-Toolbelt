using JosephM.Application.Application;
using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Module.Dialog;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Test
{
    public class TestApplication : ApplicationBase
    {
        public static TestApplication CreateTestApplication(ApplicationControllerBase applicationController = null, ISettingsManager settingsManager = null)
        {
            if(applicationController == null)
                applicationController = new FakeApplicationController();
            if (settingsManager == null)
                settingsManager = new DesktopSettingsManager(applicationController);
            return new TestApplication(applicationController, settingsManager);
        }

        private TestApplication(ApplicationControllerBase applicationController, ISettingsManager settingsManager)
            : base(applicationController, new ApplicationOptionsViewModel(applicationController), settingsManager)
        {
            Controller.RegisterType<IDialogController, FakeDialogController>();
        }

        public T GetNavigatedDialog<T>(int index = 0)
            where T : DialogViewModel
        {
            var objects = Controller
                            .GetObjects()
                            .Where(o => o.GetType().IsTypeOf(typeof(T)))
                            .ToList();
            if (index == 0)
                Assert.AreEqual(1, objects.Count(), "Ambiguous which dialog to get");
            var item = objects[index] as T;
            Assert.IsNotNull(item);
            if(!item.Controller.IsStarted)
                item.Controller.BeginDialog();
            return item;
        }

        public void EnterObject(object objectToEnter, RecordEntryViewModelBase viewModel)
        {
            foreach (var property in objectToEnter.GetType().GetReadWriteProperties())
            {
                var proprtyValue = objectToEnter.GetPropertyValue(property.Name);
                if (proprtyValue != null)
                {
                    if (property.PropertyType.Name == "IEnumerable`1")
                    {
                        if (viewModel is RecordEntryFormViewModel)
                        {
                            var subGrid = ((RecordEntryFormViewModel)viewModel).GetEnumerableFieldViewModel(property.Name);
                            subGrid.ClearRows();
                            if (proprtyValue != null)
                            {
                                foreach (var item in (IEnumerable)proprtyValue)
                                {
                                    subGrid.AddRow();
                                    if(viewModel.ChildForms.Any())
                                    {
                                        var childForm = viewModel.ChildForms.First() as ObjectEntryViewModel;
                                        if (childForm == null)
                                            throw new NullReferenceException();
                                        childForm.LoadFormSections();
                                        EnterAndSaveObject(item, childForm);
                                    }
                                    else
                                    {
                                        var newRow = subGrid.GridRecords.First();
                                        EnterObject(item, newRow);
                                    }
                                }
                            }
                        }
                        else if (viewModel is GridRowViewModel)
                        {
                            var gridRow = (GridRowViewModel)viewModel;
                            if (gridRow.CanEdit)
                            {
                                gridRow.EditRow();
                            }
                            else
                            {
                                var gridEnumField = gridRow.GetEnumerableFieldViewModel(property.Name);
                                if (gridEnumField.EditAction != null)
                                {
                                    gridEnumField.EditButton.Invoke();
                                }
                                else
                                    throw new Exception("Error determining how to enter values into enumerable field");
                            }
                            var parentForm = gridRow.GridViewModel.ParentForm as RecordEntryFormViewModel;
                            Assert.IsNotNull(parentForm);
                            Assert.AreEqual(1, parentForm.ChildForms.Count);
                            var childForm = parentForm.ChildForms.First();
                            if (childForm is RecordEntryFormViewModel)
                            {
                                var tChildForm = childForm as RecordEntryFormViewModel;
                                tChildForm.LoadFormSections();
                                EnterAndSaveObject(objectToEnter, tChildForm);
                            }
                            else
                                throw new NotImplementedException("Havent implemented for type " + childForm.GetType().Name);
                        }
                        else
                            throw new NotImplementedException("Unexpected type " + viewModel.GetType().Name);
                    }
                    else
                    {
                        if (viewModel.FieldViewModels.Any(f => f.FieldName == property.Name))
                        {
                            var fieldViewModel = viewModel.GetFieldViewModel(property.Name);
                            if (proprtyValue != null && fieldViewModel is PicklistFieldViewModel)
                            {
                                Assert.IsTrue(((PicklistFieldViewModel)fieldViewModel).ItemsSource.Any());
                                fieldViewModel.ValueObject = proprtyValue;
                            }
                            else if(proprtyValue != null && fieldViewModel is RecordFieldFieldViewModel)
                            {
                                Assert.IsTrue(((RecordFieldFieldViewModel)fieldViewModel).ItemsSource.Any());
                                fieldViewModel.ValueObject = proprtyValue;
                            }
                            else if(proprtyValue != null && fieldViewModel is RecordTypeFieldViewModel)
                            {
                                Assert.IsTrue(((RecordTypeFieldViewModel)fieldViewModel).ItemsSource.Any());
                                fieldViewModel.ValueObject = proprtyValue;
                            }
                            else
                                fieldViewModel.ValueObject = proprtyValue;
                        }
                        else
                            viewModel.GetRecord()[property.Name] = proprtyValue;
                    }
                }
            }
        }

        public TResponse NavigateAndProcessDialog<TDialogModule, TDialog, TResponse>(object instanceEntered)
            where TDialogModule : DialogModule<TDialog>, new()
            where TDialog : DialogViewModel
            where TResponse : class
        {
            TDialog dialog = NavigateAndProcessDialog<TDialogModule, TDialog>(instanceEntered);

            return dialog.CompletionItem as TResponse;
        }

        public ObjectEntryViewModel NavigateAndProcessDialogGetResponseViewModel<TDialogModule, TDialog>(object instanceEntered)
            where TDialogModule : DialogModule<TDialog>, new()
            where TDialog : DialogViewModel
        {
            TDialog dialog = NavigateAndProcessDialog<TDialogModule, TDialog>(instanceEntered);

            return GetCompletionViewModel(dialog);
        }

        public ObjectEntryViewModel GetCompletionViewModel(DialogViewModel dialog)
        {
            var uiItem = dialog.Controller.UiItems.First();
            if(dialog.FatalException != null)
            {
                throw dialog.FatalException;
            }
            if (uiItem is CompletionScreenViewModel completionScreen)
            {
                Assert.IsNotNull(completionScreen.CompletionDetails);
                LoadForm(completionScreen.CompletionDetails);
                return completionScreen.CompletionDetails;
            }
            if (uiItem is ObjectEntryViewModel entryViewModel)
            {
                if(entryViewModel.ValidationPrompt != null)
                {
                    Assert.Fail("Validation Message: " + entryViewModel.ValidationPrompt);
                }
            }
            Assert.Fail("Could not get completion screen for dialog");
            throw new Exception("Huh?");
        }

        private TDialog NavigateAndProcessDialog<TDialogModule, TDialog>(object instanceEntered)
            where TDialogModule : DialogModule<TDialog>, new()
            where TDialog : DialogViewModel
        {
            var dialog = NavigateToDialog<TDialogModule, TDialog>();
            var entryForm = GetSubObjectEntryViewModel(dialog);

            if (entryForm is ObjectEntryViewModel)
            {
                LoadForm(entryForm);
            }

            EnterAndSaveObject(instanceEntered, entryForm);
            var uiItem = dialog.Controller.UiItems.Any()
                ? dialog.Controller.UiItems.First()
                : null;
            //this is for dialogs which display any validation details
            //after the initial entry
            //in this case we proceed at that screen as well
            if(uiItem is ObjectDisplayViewModel)
            {
                var odvbm = uiItem as ObjectDisplayViewModel;
                odvbm.SaveButtonViewModel.Invoke();
            }
            return dialog;
        }

        private static void LoadForm(ObjectEntryViewModel entryForm)
        {
            entryForm.LoadFormSections();

            foreach (var grid in entryForm.SubGrids)
                Assert.IsNotNull(grid.DynamicGridViewModel.GridRecords);
        }

        public void NavigateAndProcessDialog<TDialogModule, TDialog>(IEnumerable<object> instancesEntered)
            where TDialogModule : DialogModule<TDialog>, new()
            where TDialog : DialogViewModel
        {
            var dialog = NavigateToDialog<TDialogModule, TDialog>();
            var instanceList = instancesEntered.ToList();
            for (var i = 0; i < instanceList.Count(); i++)
            {
                var entryForm = GetSubObjectEntryViewModel(dialog, i);
                EnterAndSaveObject(instanceList[i], entryForm);
            }
        }

        public RecordEntryFormViewModel NavigateToDialogModuleEntryForm<TDialogModule, TDialog>()
            where TDialogModule : DialogModule<TDialog>, new()
            where TDialog : DialogViewModel
        {
            var dialog = NavigateToDialog<TDialogModule, TDialog>();
            return GetSubObjectEntryViewModel(dialog);
        }

        public TDialog NavigateToDialog<TDialogModule, TDialog>() where TDialogModule : DialogModule<TDialog>, new()
            where TDialog : DialogViewModel
        {
            Controller.ClearTabs();
            var module = GetModule<TDialogModule>();
            module.DialogCommand();
            //autodialog should process the dialog when get it
            var dialog = GetNavigatedDialog<TDialog>();
            return dialog;
        }

        public void EnterAndSaveObject(object objectToEnter, RecordEntryFormViewModel viewModel)
        {
            EnterObject(objectToEnter, viewModel);
            if(!viewModel.Validate())
                throw new Exception(viewModel.GetValidationSummary());
            viewModel.SaveButtonViewModel.Invoke();
        }

        public ObjectEntryViewModel GetSubObjectEntryViewModel(DialogViewModel addDialog, int index = 0)
        {
            var typeSelection = addDialog.SubDialogs.ElementAt(index) as ObjectEntryDialogBase;
            Assert.IsNotNull(typeSelection);
            if (typeSelection.ViewModel == null)
                addDialog.LoadDialog();
            var viewModel = typeSelection.ViewModel;
            Assert.IsNotNull(viewModel);
            viewModel.LoadFormSections();
            return viewModel;
        }

        public ObjectEntryViewModel GetSubObjectEntryViewModel(RecordEntryFormViewModel entryForm)
        {
            var subEntry = entryForm.ChildForms.First() as ObjectEntryViewModel;
            Assert.IsNotNull(subEntry);
            subEntry.LoadFormSections();
            return subEntry;
        }


        public DialogViewModel GetSubDialog(DialogViewModel addDialog, int index = 0)
        {
            return addDialog.SubDialogs.ElementAt(index);
        }

        public void ClearTabs()
        {
            Controller.ClearTabs();
        }
    }
}