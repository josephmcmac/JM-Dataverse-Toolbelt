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

namespace JosephM.Prism.Infrastructure.Test
{
    public class TestApplication : ApplicationBase
    {
        public TestApplication()
            : base(new FakeApplicationController())
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
                                var newRow = subGrid.GridRecords.First();
                                EnterObject(item, newRow);
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

        public void NavigateAndProcessDialog<TDialogModule, TDialog>(object instanceEntered)
            where TDialogModule : DialogModule<TDialog>, new()
            where TDialog : DialogViewModel
        {
            var entryForm = NavigateToDialogModuleEntryForm<TDialogModule, TDialog>();

            if (entryForm is ObjectEntryViewModel)
            {
                entryForm.LoadFormSections();
                    var oevm = (ObjectEntryViewModel)entryForm;

                foreach (var grid in oevm.SubGrids)
                    if (grid.DynamicGridViewModel.LoadedCallback != null)
                        grid.DynamicGridViewModel.LoadedCallback();

                if (oevm.SaveRequestButtonViewModel.IsVisible)
                {
                    oevm.SaveRequestButtonViewModel.Invoke();
                    Assert.AreEqual(1, oevm.ChildForms.Count());
                    var childForm = oevm.ChildForms.First();
                    if (childForm is RecordEntryFormViewModel)
                    {
                        var tChildForm = childForm as ObjectEntryViewModel;
                        tChildForm.LoadFormSections();
                        var childObject = tChildForm.GetObject();
                        CoreTest.PopulateObject(childObject);
                        EnterAndSaveObject(childObject, tChildForm);
                    }
                    else
                        throw new NotImplementedException("Havent implemented for type " + childForm.GetType().Name);
                }
            }

            EnterAndSaveObject(instanceEntered, entryForm);
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

        private TDialog NavigateToDialog<TDialogModule, TDialog>() where TDialogModule : DialogModule<TDialog>, new()
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
            Assert.IsTrue(viewModel.Validate(), viewModel.GetValidationSummary());
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

        public void ClearTabs()
        {
            Controller.ClearTabs();
        }
    }
}