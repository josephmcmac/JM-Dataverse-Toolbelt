using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Extentions;
using System.Reflection;
using System.Threading;

namespace JosephM.Application.ViewModel.Attributes
{
    public abstract class BulkAddFunction : Attribute
    {
        public string GetFunctionLabel()
        {
            return "Add Multiple";
        }

        public abstract IRecordService GetQueryLookupService(RecordEntryViewModelBase recordForm, string subGridReference);

        public IRecordService GetLookupservice(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            return recordForm.RecordService.GetLookupService(GetTargetProperty(recordForm, subGridReference).Name, GetEnumeratedType(recordForm, subGridReference).FullName, subGridReference, recordForm.GetRecord());
        }

        public Action GetCustomFunction(RecordEntryViewModelBase recordForm, string subGridReference)
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

        public virtual bool AllowQuery { get { return true; } }

        public abstract Type TargetPropertyType { get; }

        public IRecordService GetLookupService(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            return recordForm.RecordService.GetLookupService(GetTargetProperty(recordForm, subGridReference).Name, GetEnumeratedType(recordForm, subGridReference).FullName, subGridReference, recordForm.GetRecord());
        }

        public PropertyInfo GetTargetProperty(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            PropertyInfo targetPropertyInfo = null;
            foreach (var enumeratedTypeProperty in GetEnumeratedType(recordForm, subGridReference).GetProperties())
            {
                if (enumeratedTypeProperty.PropertyType == TargetPropertyType)
                {
                    targetPropertyInfo = enumeratedTypeProperty;
                }
            }

            return targetPropertyInfo;
        }

        public static Type GetEnumeratedType(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var theObject = GetObjectFormService(recordForm).GetObject();

            var type = theObject.GetType();
            var property = type.GetProperty(subGridReference);
            var enumeratedType = property.PropertyType.GetGenericArguments()[0];
            return enumeratedType;
        }

        public static ObjectEntryViewModel GetObjectFormService(RecordEntryViewModelBase recordForm)
        {
            var objectFormService = recordForm as ObjectEntryViewModel;
            if (objectFormService == null)
            {
                throw new ArgumentOutOfRangeException(nameof(recordForm), string.Format("Required To Be Type Of {0}. Actual Type Is {1}", typeof(ObjectEntryViewModel).Name, recordForm.GetType().Name));
            }

            return objectFormService;
        }

        public void Load(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            recordForm.DoOnMainThread(() =>
            {
                try
                {
                    var closeFunction = new CustomGridFunction("Return", () => recordForm.ClearChildForm());
                    var targetType = GetTargetType(recordForm, subGridReference);

                    var selectedFunction = new CustomGridFunction("Add Selected", (g) => AddSelectedItems(g, recordForm, subGridReference)
                    , visibleFunction: (g) => g.SelectedRows.Any());

                    var childForm = new QueryViewModel(new[] { targetType }, GetQueryLookupService(recordForm, subGridReference), recordForm.ApplicationController, allowQuery: AllowQuery, loadInitially: !AllowQuery, closeFunction: closeFunction, processSelectedFunction: selectedFunction);

                    recordForm.LoadChildForm(childForm);
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

        public abstract string GetTargetType(RecordEntryViewModelBase recordForm, string subGridReference);

        public void AddSelectedItems(DynamicGridViewModel grid, RecordEntryViewModelBase recordForm, string subGridReference)
        {
            recordForm.ApplicationController.DoOnAsyncThread(() =>
            {
                recordForm.LoadingViewModel.IsLoading = true;
                try
                {
                    Thread.Sleep(100);
                    foreach (var selectedRow in grid.SelectedRows)
                    {
                        AddSelectedItem(selectedRow, recordForm, subGridReference);
                    }
                    recordForm.ClearChildForm();
                }
                catch(Exception ex)
                {
                    recordForm.ApplicationController.ThrowException(ex);
                }
                finally
                {
                    recordForm.LoadingViewModel.IsLoading = false;
                }
            });
        }

        public abstract void AddSelectedItem(GridRowViewModel gridRow, RecordEntryViewModelBase recordForm, string subGridReference);
    }
}