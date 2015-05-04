#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.IService;
using JosephM.Record.Metadata;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class LookupGridViewModel : ViewModelBase, IDynamicGridViewModel
    {
        public LookupGridViewModel(IRecordService recordService, string recordType,
            RecordEntryViewModelBase recordEntryViewModel,
            Action<IRecord> onRecordSelected)
            : base(recordEntryViewModel.ApplicationController)
        {
            RecordType = recordType;
            RecordService = recordService;
            OnRecordSelected = onRecordSelected;
            RecordEntryViewModel = recordEntryViewModel;
            FormController = new FormController(RecordService, null, recordEntryViewModel.ApplicationController);
            DynamicGridViewModelItems = new DynamicGridViewModelItems()
            {
                CanDelete = false,
                OnDoubleClick = OnDoubleClick
            };
        }

        public FormController FormController { get; private set; }

        public bool IsReadOnly
        {
            get { return true; }
        }

        private IEnumerable<GridFieldMetadata> _recordFields;
        private RecordEntryViewModelBase RecordEntryViewModel { get; set; }

        public IEnumerable<GridFieldMetadata> RecordFields
        {
            get
            {
                if (_recordFields == null)
                {
                    if (RecordType.IsNullOrWhiteSpace())
                        _recordFields = new GridFieldMetadata[] {};
                    else
                    {
                        var savedViews = RecordService.GetViews(RecordType);
                        if (savedViews != null && savedViews.Any(v => v.ViewType == ViewType.LookupView))
                        {
                            var lookupView = savedViews.First(v => v.ViewType == ViewType.LookupView);
                            _recordFields = lookupView
                                .Fields
                                .Select(f => new GridFieldMetadata(f.FieldName) {WidthPart = f.Width})
                                .ToArray();
                        }
                        else
                        {
                            _recordFields = new[]
                            {new GridFieldMetadata(RecordService.GetPrimaryField(RecordType)) {WidthPart = 200}};
                        }
                    }
                }
                return _recordFields;
            }
        }

        public IRecordService RecordService { get; private set; }
        public string RecordType { get; set; }
        private Action<IRecord> OnRecordSelected { get; set; }

        private ObservableCollection<GridRowViewModel> _records;

        public ObservableCollection<GridRowViewModel> GridRecords
        {
            get { return _records; }
            set
            {
                _records = value;
                OnPropertyChanged("GridRecords");
            }
        }

        private GridRowViewModel _selectedRow;

        public GridRowViewModel SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                _selectedRow = value;
                OnPropertyChanged("SelectedRow");
            }
        }

        public void OnKeyDown()
        {
        }

        public void OnDoubleClick()
        {
            SetLookupToSelectedRow();
        }

        public void SetLookupToSelectedRow()
        {
            if (SelectedRow != null)
                OnRecordSelected(SelectedRow.Record);
        }

        private bool _isFocused;

        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                OnPropertyChanged("IsFocused");
            }
        }

        public void MoveDown()
        {
            try
            {
                if (GridRecords != null && GridRecords.Any())
                {
                    var index = -1;
                    if (SelectedRow != null)
                        index = GridRecords.IndexOf(SelectedRow);
                    index++;
                    if (index > GridRecords.Count - 1)
                        index = 0;
                    SelectedRow = GridRecords[index];
                }
            }
            catch
            {
            }
        }

        public void MoveUp()
        {
            try
            {
                if (GridRecords != null && GridRecords.Any())
                {
                    var index = GridRecords.Count;
                    if (SelectedRow != null)
                        index = GridRecords.IndexOf(SelectedRow);
                    index--;
                    if (index < 0)
                        index = GridRecords.Count - 1;
                    SelectedRow = GridRecords[index];
                }
            }
            catch
            {
            }
        }

        public void DoWhileLoading(string message, Action action)
        {
            RecordEntryViewModel.DoWhileLoading(message, action);
        }


        public DynamicGridViewModelItems DynamicGridViewModelItems { get; set; }
    }
}