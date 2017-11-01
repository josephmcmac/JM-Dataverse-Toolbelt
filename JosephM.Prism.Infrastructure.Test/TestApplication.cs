using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Test;
using JosephM.Application.Options;
using JosephM.Core.AppConfig;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Prism.Infrastructure.Dialog;

namespace JosephM.Prism.Infrastructure.Test
{
    public class TestApplication : ApplicationBase
    {
        public static TestApplication CreateTestApplication(ApplicationControllerBase applicationController = null, ISettingsManager settingsManager = null)
        {
            if(applicationController == null)
                applicationController = new FakeApplicationController();
            if (settingsManager == null)
                settingsManager = new PrismSettingsManager(applicationController);
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
                            .GetObjects(RegionNames.MainTabRegion)
                            .Where(o => o.GetType().IsTypeOf(typeof(T)))
                            .ToList();
            if (index == 0)
                Assert.AreEqual(1, objects.Count(), "Ambiguous which dialog to get");
            var item = objects[index] as T;
            Assert.IsNotNull(item);
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
                            var subGrid = ((RecordEntryFormViewModel)viewModel).GetSubGridViewModel(property.Name);
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
                            gridRow.EditRow();
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
                            else
                                fieldViewModel.ValueObject = proprtyValue;
                        }
                        else
                            viewModel.GetRecord()[property.Name] = proprtyValue;
                    }
                }
            }
        }

        public void NavigateAndProcessDialog<TDialogModule, TDialog>(object instanceEntered)
            where TDialogModule : DialogModule<TDialog>, new()
            where TDialog : DialogViewModel
        {
            var entryForm = NavigateToDialogModuleEntryForm<TDialogModule, TDialog>();

            var saveRequest = false;
            Type savedRequestType = null;

            if (entryForm is ObjectEntryViewModel)
            {
                entryForm.LoadFormSections();
                    var oevm = (ObjectEntryViewModel)entryForm;

                foreach (var grid in oevm.SubGrids)
                    if (grid.DynamicGridViewModel.LoadedCallback != null)
                        grid.DynamicGridViewModel.LoadedCallback();

            }

            EnterAndSaveObject(instanceEntered, entryForm);


            if(saveRequest)
            {
                //okay lets delete the request we saved earlier (and any others)
                ObjectEntryViewModel oevm = LoadSavedRequestsEntryForm(savedRequestType);
                foreach (var grid in oevm.SubGrids)
                {
                    while (grid.GridRecords.Any())
                        grid.GridRecords.First().DeleteRow();
                }
                oevm.SaveButtonViewModel.Invoke();
            }
        }

        public ObjectEntryViewModel LoadSavedRequestsEntryForm(Type savedRequestType)
        {
            var applicationOptions = (ApplicationOptionsViewModel)Controller.Container.ResolveType<IApplicationOptions>();

            //get the setting which has the label - hope this doesn't break
            var savedSettingsOption = applicationOptions.Settings.First(o => o.Label.EndsWith(savedRequestType.GetDisplayName()));
            savedSettingsOption.DelegateCommand.Execute();

            var items = Controller.GetObjects(RegionNames.MainTabRegion);
            var dialog = items.First();
            Assert.IsTrue(dialog is SavedRequestDialog);
            var srd = (SavedRequestDialog)dialog;
            srd.Controller.BeginDialog();
            var oevm = GetSubObjectEntryViewModel(srd);
            return oevm;
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
            var viewModel = typeSelection.ViewModel;
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