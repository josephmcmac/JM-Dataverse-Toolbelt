#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Extentions;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Section
{
    public class GridSectionViewModel : SectionViewModelBase, IValidatable
    {
        private ObservableCollection<GridRowViewModel> _records;

        public GridSectionViewModel(SubGridSection subGridSection,
            RecordEntryFormViewModel recordForm)
            : base(subGridSection, recordForm)
        {
            DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
            {
                RecordFields = SubGridSection.Fields,
                PageSize = recordForm.GridPageSize,
                DeleteRow = recordForm.IsReadOnly ? (Action<GridRowViewModel>)null : RemoveRow,
                EditRow = EditRow,
                AddRow = recordForm.IsReadOnly ? (Action)null : AddRow,
                IsReadOnly = recordForm.IsReadOnly,
                ParentForm = recordForm,
                ReferenceName = ReferenceName,
                RecordType = subGridSection.LinkedRecordType,
                RecordService = RecordService,
                GetGridRecords = GetGridRecords,
                LoadRecordsAsync = true,
                FormController = RecordForm.FormController,
                LoadedCallback = () =>
                {
                    IsLoaded = true;
                    RecordForm.OnSectionLoaded();
                }
            };
            var customFunctions = FormService.GetCustomFunctionsFor(ReferenceName, GetRecordForm()).ToList();
            customFunctions.Add(new CustomGridFunction("Download CSV", DownloadCsv));
            DynamicGridViewModel.LoadGridButtons(customFunctions);
        }

        public void AddRow()
        {
            try
            {
                var viewModel = FormService.GetLoadRowViewModel(SectionIdentifier, RecordForm, (record) =>
                {
                    InsertRecord(record, 0);
                    RecordForm.ClearChildForm();
                }, () => RecordForm.ClearChildForm());
                if (viewModel == null)
                    InsertRecord(RecordService.NewRecord(RecordType), 0);
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
                    //todo this wouldn't match in test script couldn't figure out why
                    InsertRecord(record, index == -1 ? 0 : index);
                });
            }, () => RecordForm.ClearChildForm(), row);
            return viewModel;
        }

        private SubGridSection SubGridSection
        {
            get { return FormSection as SubGridSection; }
        }

        public string ReferenceName
        {
            get { return SubGridSection.LinkedRecordLookup; }
        }

        public IEnumerable<GridFieldMetadata> RecordFields
        {
            get { return SubGridSection.Fields; }
        }

        public GetGridRecordsResponse GetGridRecords(bool ignorePages)
        {
            if (DynamicGridViewModel.HasPaging && !ignorePages)
            {
                var conditions = new[]
                {new Record.Query.Condition(SubGridSection.LinkedRecordLookup, ConditionType.Equal, RecordForm.RecordId)};
                return DynamicGridViewModel.GetGridRecordPage(conditions, null);
            }
            else
                return new GetGridRecordsResponse(RecordService.GetLinkedRecords(SubGridSection.LinkedRecordType, RecordForm.RecordType,
                    SubGridSection.LinkedRecordLookup, RecordForm.RecordId));
        }

        private void InsertRecord(IRecord record, int index)
        {
            var rowItem = new GridRowViewModel(record, DynamicGridViewModel);
            DynamicGridViewModel.GridRecords.Insert(index, rowItem);
            rowItem.OnLoad();
        }

        public override string RecordType
        {
            get { return SubGridSection.LinkedRecordType; }
        }


        public DynamicGridViewModel DynamicGridViewModel { get; set; }

        internal override bool Validate()
        {
            ErrorMessage = null;
            var isValid = true;
            if (IsVisible)
            {
                foreach (var gridRowViewModel in DynamicGridViewModel.GridRecords)
                {
                    if (!gridRowViewModel.Validate())
                        isValid = false;
                }
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

        public override string SectionIdentifier
        {
            get { return ReferenceName; }
        }

        public RecordEntryViewModelBase GetRecordForm()
        {
            return RecordForm;
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
                                , RecordService.GetFields(RecordType),
                                (f) => RecordService.GetFieldLabel(f, RecordType),
                                (r, f) => RecordService.GetFieldAsDisplayString((IRecord) r, f));
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
    }
}