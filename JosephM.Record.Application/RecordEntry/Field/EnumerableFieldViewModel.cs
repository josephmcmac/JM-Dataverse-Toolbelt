using System.Collections;
using System.Collections.Generic;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Grid;
using System;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Record.IService;
using JosephM.Record.Query;
using System.Collections.ObjectModel;
using System.Threading;
using JosephM.Core.Utility;
using System.IO;
using System.Diagnostics;
using JosephM.Record.Extentions;
using JosephM.Application.ViewModel.Extentions;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class EnumerableFieldViewModel : FieldViewModel<IEnumerable>
    {
        public EnumerableFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm, string linkedRecordType)
            : base(fieldName, label, recordForm)
        {
            if (recordForm is RecordEntryFormViewModel)
            {
                RecordForm = (RecordEntryFormViewModel)recordForm;
                RecordFields = recordForm.FormService.GetGridMetadata(linkedRecordType);
                LinkedRecordType = linkedRecordType;

                DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
                {
                    RecordFields = RecordFields,
                    PageSize = RecordForm.GridPageSize,
                    DeleteRow = recordForm.IsReadOnly ? (Action<GridRowViewModel>)null : RemoveRow,
                    EditRow = EditRow,
                    AddRow = !recordForm.IsReadOnly && FormService.AllowAddRow(ReferenceName) ? AddRow : (Action)null,
                    AddMultipleRow = FormService.GetBulkAddFunctionFor(ReferenceName, RecordEntryViewModel),
                    IsReadOnly = recordForm.IsReadOnly,
                    ParentForm = recordForm,
                    ReferenceName = ReferenceName,
                    RecordType = linkedRecordType,
                    RecordService = recordForm.RecordService,
                    GetGridRecords = GetGridRecords,
                    LoadRecordsAsync = true,
                    FormController = recordForm.FormController,
                    OnReloading = () =>
                    {
                        _isLoaded = false;
                    },
                    LoadedCallback = () =>
                    {
                        _isLoaded = true;
                        RecordForm.OnSectionLoaded();
                    },
                    OnlyValidate = recordForm.OnlyValidate
                };
                DynamicGridViewModel.AddMultipleRow = FormService.GetBulkAddFunctionFor(ReferenceName, RecordEntryViewModel);
                var customFunctions = FormService.GetCustomFunctionsFor(ReferenceName, GetRecordForm()).ToList();
                //customFunctions.Add(new CustomGridFunction("Download CSV", DownloadCsv));
                DynamicGridViewModel.LoadGridButtons(customFunctions);
            }
        }

        private bool _isLoaded;
        public override bool IsLoaded { get { return _isLoaded; } }

        private string LinkedRecordLookup {  get { return FieldName; } }
        private string LinkedRecordType { get; set; }

        private RecordEntryFormViewModel RecordForm { get; set; }

        public void AddRow()
        {
            try
            {
                var viewModel = FormService.GetLoadRowViewModel(SectionIdentifier, RecordForm, (record) =>
                DoOnMainThread(() =>
                {
                    InsertRecord(record, 0);
                    RecordForm.ClearChildForm();
                }), () => RecordForm.ClearChildForm());
                if (viewModel == null)
                {

                    InsertRecord(GetRecordService().NewRecord(RecordType), 0);
                }
                else
                {
                    RecordForm.LoadChildForm(viewModel);
                }
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(string.Format("Error Adding Row: {0}", ex.DisplayString()));
            }
        }

        private void RemoveRow(GridRowViewModel row)
        {
            DynamicGridViewModel.GridRecords.Remove(row);
        }

        private void EditRow(GridRowViewModel row)
        {
            try
            {
                var viewModel = GetEditRowViewModel(row);
                if (viewModel == null)
                {
                    throw new NotImplementedException("No Form For Type");
                }
                else
                    RecordForm.LoadChildForm(viewModel);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(string.Format("Error Adding Row: {0}", ex.DisplayString()));
            }
        }

        public RecordEntryFormViewModel GetEditRowViewModel(GridRowViewModel row)
        {
            var viewModel = FormService.GetEditRowViewModel(SectionIdentifier, RecordForm, (record) =>
            {
                RecordForm.ClearChildForm();
                var index = DynamicGridViewModel.GridRecords.IndexOf(row);
                DoOnMainThread(() =>
                {
                    DynamicGridViewModel.GridRecords.Remove(row);
                    InsertRecord(record, index == -1 ? 0 : index);
                });
            }, () => RecordForm.ClearChildForm(), row);
            return viewModel;
        }

        public IEnumerable<GridFieldMetadata> RecordFields
        {
            get; set;
        }

        public GetGridRecordsResponse GetGridRecords(bool ignorePages)
        {
            if (DynamicGridViewModel.HasPaging && !ignorePages)
            {
                var conditions = new[]
                {new Condition(LinkedRecordLookup, ConditionType.Equal, RecordForm.RecordId)};
                return DynamicGridViewModel.GetGridRecordPage(conditions, null);
            }
            else
                return new GetGridRecordsResponse(GetRecordService().GetLinkedRecords(LinkedRecordType, RecordForm.RecordType,
                    LinkedRecordLookup, RecordForm.RecordId));
        }



        public void InsertRecord(IRecord record, int index)
        {
            DoOnAsynchThread(() =>
            {
                RecordForm.LoadingViewModel.IsLoading = true;
                try
                {
                    //this part may take a while to load/create (e.g. get record types for record type field)
                    //so create on asynch thread while loading so doesn't freeze ui
                    //then add to observable collection on main thread
                    var rowItem = new GridRowViewModel(record, DynamicGridViewModel);
                    DoOnMainThread(() =>
                    {
                        try
                        {

                            DynamicGridViewModel.GridRecords.Insert(index, rowItem);
                            rowItem.OnLoad();
                            rowItem.RunOnChanges();
                        }
                        finally
                        {
                            RecordForm.LoadingViewModel.IsLoading = false;
                        }
                    });
                }
                catch (Exception)
                {
                    RecordForm.LoadingViewModel.IsLoading = false;
                    throw;
                }
            });
        }

        public string RecordType
        {
            get { return LinkedRecordType; }
        }


        public DynamicGridViewModel DynamicGridViewModel { get; set; }

        public override bool Validate()
        {
            ErrorMessage = null;
            var isValid = true;
            if (IsVisible && DynamicGridViewModel != null)
            {
                foreach (var gridRowViewModel in DynamicGridViewModel.GridRecords)
                {
                    if (!gridRowViewModel.Validate())
                        isValid = false;
                }
                var onlyValidate = GetRecordForm().OnlyValidate;
                if (onlyValidate == null ||
                    (onlyValidate.ContainsKey(RecordType) && onlyValidate[RecordType].Contains(FieldName)))
                {
                    var thisValidators = FormService.GetSectionValidationRules(SectionIdentifier);

                    foreach (var validator in thisValidators)
                    {
                        var response = validator.Validate(this);
                        if (!response.IsValid)
                        {
                            ErrorMessage = response.ErrorContent != null ? response.ErrorContent.ToString() : "No Error Content";
                            //need to somehow get the error message into the grid
                            isValid = false;
                        }
                    }
                }
            }

            return isValid;
        }

        private bool _hasError;
        public bool HasError
        {
            get { return _hasError; }
            set
            {
                _hasError = value;
                OnPropertyChanged("HasError");
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                HasError = !_errorMessage.IsNullOrWhiteSpace();
                OnPropertyChanged("ErrorMessage");
            }
        }

        public string SectionIdentifier
        {
            get { return ReferenceName; }
        }

        public ObservableCollection<GridRowViewModel> GridRecords
        {
            get { return DynamicGridViewModel.GridRecords; }
        }

        public void ClearRows()
        {
            ApplicationController.DoOnMainThread(() => DynamicGridViewModel.GridRecords.Clear());
        }

        public void DownloadCsv()
        {
            try
            {
                var fileName = ApplicationController.GetSaveFileName(RecordType, ".csv");
                if (!fileName.IsNullOrWhiteSpace())
                {
                    RecordForm.LoadingViewModel.IsLoading = true;
                    DoOnAsynchThread(() =>
                    {
                        try
                        {
                            Thread.Sleep(5000);
                            var records = GetGridRecords(true);
                            CsvUtility.CreateCsv(Path.GetDirectoryName(fileName), Path.GetFileName(fileName),
                                records.Records
                                , GetRecordService().GetFields(RecordType),
                                (f) => GetRecordService().GetFieldLabel(f, RecordType),
                                (r, f) => GetRecordService().GetFieldAsDisplayString((IRecord)r, f));
                            Process.Start(fileName);
                        }
                        catch (Exception ex)
                        {
                            ApplicationController.ThrowException(ex);
                        }
                        finally
                        {
                            RecordForm.LoadingViewModel.IsLoading = false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ApplicationController.ThrowException(ex);
            }
        }

        public string StringDisplay
        {
            get
            {
                var list = new List<string>();
                if (Enumerable != null)
                {
                    foreach (var item in Enumerable)
                        list.Add(item == null ? "" : item.ToString());
                }
                return string.Join(",", list.ToArray());
            }
        }

        public IEnumerable Enumerable
        {
            get { return ValueObject == null ? null : (IEnumerable) ValueObject; }
        }
    }
}
