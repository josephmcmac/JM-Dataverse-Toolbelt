using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.IService;
using JosephM.Record.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections;
using JosephM.Core.Extentions;

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
            //todo this does not work when it is a field in an enumerable field in a grid
            //and we have a connection for or other reference type
            var reference = string.Format("{0}{1}", (recordForm.ParentFormReference == null ? null : recordForm.ParentFormReference + "."), subGridReference);

            return recordForm.RecordService.GetLookupService(GetTargetProperty(recordForm, subGridReference).Name, GetEnumeratedType(recordForm, subGridReference).FullName, reference, recordForm.GetRecord());
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
            var theRecord = GetEntryViewModel(recordForm).GetRecord() as ObjectRecord;
            if (theRecord == null)
                throw new NullReferenceException(string.Format("Expected Object Of Type {0}. Actual Type Is {1}", typeof(ObjectRecord).Name, GetEntryViewModel(recordForm).GetRecord()?.GetType()?.Name ?? "(null)"));
            var theObject = theRecord.Instance;

            var type = theObject.GetType();
            var property = type.GetProperty(subGridReference);
            var enumeratedType = property.PropertyType.GetGenericArguments()[0];
            return enumeratedType;
        }

        public static RecordEntryViewModelBase GetEntryViewModel(RecordEntryViewModelBase recordForm)
        {
            return recordForm;
            //var objectFormService = recordForm as ObjectEntryViewModel;
            //if (objectFormService == null)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(recordForm), string.Format("Required To Be Type Of {0}. Actual Type Is {1}", typeof(ObjectEntryViewModel).Name, recordForm.GetType().Name));
            //}

            //return objectFormService;
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

                    var closeFunction = new CustomGridFunction("RETURN", "Return", () => mainFormInContext.ClearChildForm());
                    var targetType = GetTargetType(recordForm, subGridReference);

                    var selectedFunction = new CustomGridFunction("ADDSELECTED", "Add Selected", (g) => AddSelectedItems(g, recordForm, subGridReference)
                    , visibleFunction: (g) => g.SelectedRows.Any());

                    var childForm = new QueryViewModel(new[] { targetType }, GetQueryLookupService(recordForm, subGridReference), recordForm.ApplicationController, allowQuery: AllowQuery, loadInitially: !AllowQuery, closeFunction: closeFunction, customFunctions: new[] { selectedFunction });
                    childForm.TypeAhead = TypeAhead;
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

        public virtual bool TypeAhead { get { return false; } }

        public abstract string GetTargetType(RecordEntryViewModelBase recordForm, string subGridReference);

        public void AddSelectedItems(DynamicGridViewModel grid, RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var mainFormInContext = recordForm;
            if (recordForm is GridRowViewModel)
                mainFormInContext = recordForm.ParentForm;
            mainFormInContext.ApplicationController.DoOnAsyncThread(() =>
            {
                mainFormInContext.LoadingViewModel.IsLoading = true;
                try
                {
                    Thread.Sleep(100);
                    foreach (var selectedRow in grid.SelectedRows)
                    {
                        AddSelectedItem(selectedRow, recordForm, subGridReference);
                    }
                    mainFormInContext.ClearChildForm();
                }
                catch(Exception ex)
                {
                    mainFormInContext.ApplicationController.ThrowException(ex);
                }
                finally
                {
                    mainFormInContext.LoadingViewModel.IsLoading = false;
                }
            });
        }

        public abstract void AddSelectedItem(GridRowViewModel gridRow, RecordEntryViewModelBase recordForm, string subGridReference);

        public void InsertNewItem(RecordEntryViewModelBase recordForm, string subGridReference, IRecord recordToInsert)
        {
            //todo consider adding duplicates logic

            //if the grid field is a subgrid then we add to the grid
            //otherwise if it is within a subgrid, it is just a display string
            //in which case lets just add it to the object
            var enumerableField = GetEntryViewModel(recordForm).GetEnumerableFieldViewModel(subGridReference);

            if (enumerableField.DynamicGridViewModel == null)
            {
                var objectRecord = recordToInsert as ObjectRecord;
                if (objectRecord == null)
                    throw new ArgumentOutOfRangeException(nameof(recordToInsert), string.Format("Expected Object of Type {0}. Actual Type Was {1}", typeof(ObjectRecord).Name, recordToInsert?.GetType()?.Name ?? "(null)"));
                var items = new List<object>();
                if(enumerableField.ValueObject as IEnumerable != null)
                {
                    foreach (var item in enumerableField.ValueObject as IEnumerable)
                        items.Add(item);
                }
                items.Add(objectRecord.Instance);
                enumerableField.Value = (IEnumerable)GetEnumeratedType(recordForm, subGridReference).ToNewTypedEnumerable(items);
            }
            else
            {
                var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;
                enumerableField.InsertRecord(recordToInsert, 0);
            }
        }
    }
}