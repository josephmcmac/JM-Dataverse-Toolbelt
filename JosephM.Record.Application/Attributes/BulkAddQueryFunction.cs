using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Query;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.IService;
using System;
using System.Linq;
using System.Threading;

namespace JosephM.Application.ViewModel.Attributes
{
    public abstract class BulkAddQueryFunction : BulkAddFunction
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
                    }
                    finally
                    {
                        recordForm.LoadingViewModel.IsLoading = false;
                    }
                });
            };
        }

        public virtual bool AllowQuery { get { return true; } }

        public void Load(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var parentForm = recordForm.ParentForm;
            if (parentForm != null)
                parentForm.LoadingViewModel.IsLoading = true;
            recordForm.DoOnAsynchThread(() =>
            {
                try
                {
                    var mainFormInContext = recordForm;
                    if (recordForm is GridRowViewModel)
                        mainFormInContext = recordForm.ParentForm;

                    var closeFunction = new CustomGridFunction("RETURN", "Return", () => mainFormInContext.ClearChildForm());
                    var targetType = GetTargetType(recordForm, subGridReference);

                    var selectedFunction = new CustomGridFunction("ADDSELECTED", "Add Selected", (g) => AddSelectedItems(g, recordForm, subGridReference)
                    , visibleFunction: (g) => g.GridRecords != null && g.GridRecords.Any());

                    var childForm = new QueryViewModel(new[] { targetType }, GetQueryLookupService(recordForm, subGridReference), recordForm.ApplicationController, allowQuery: AllowQuery, loadInitially: !AllowQuery, closeFunction: closeFunction, customFunctions: new[] { selectedFunction }, allowCrud: false);
                    childForm.TypeAhead = TypeAhead;
                    mainFormInContext.LoadChildForm(childForm);
                }
                catch (Exception ex)
                {
                    recordForm.ApplicationController.ThrowException(ex);
                }
                finally
                {
                    if (parentForm != null)
                        parentForm.LoadingViewModel.IsLoading = false;
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
    }
}