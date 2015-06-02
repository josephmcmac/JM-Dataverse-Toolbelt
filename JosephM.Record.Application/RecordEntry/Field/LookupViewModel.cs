#region

using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.Shared;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class LookupFieldViewModel : FieldViewModel<Lookup>
    {
        public LookupFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm,
            string referencedRecordType)
            : this(fieldName, fieldLabel, recordForm, referencedRecordType, null)
        {
        }

        public LookupFieldViewModel(string fieldName, string fieldLabel, RecordEntryViewModelBase recordForm,
            string referencedRecordType, IRecordService lookupService)
            : base(fieldName, fieldLabel, recordForm)
        {
            RecordTypeToLookup = referencedRecordType;
            if (lookupService != null)
            {
                LookupService = lookupService;
                LookupGridViewModel = new LookupGridViewModel(LookupService, RecordTypeToLookup, RecordEntryViewModel,
                    OnRecordSelected);
                XrmButton = new XrmButtonViewModel("Search", Search, ApplicationController);
            }
            if(Value != null)
                SetEnteredTestWithoutClearingValue(Value.Name);
        }

        public void Search()
        {
            ApplicationController.DoOnAsyncThread(LoadRowsAsync);
        }

        public void OnRecordSelected(IRecord selectedRecord)
        {
            if (selectedRecord != null)
            {
                var recordName = selectedRecord.GetStringField(LookupService.GetPrimaryField(selectedRecord.Type));
                Value = new Lookup(RecordTypeToLookup, selectedRecord.Id, recordName);
                LookupGridVisible = false;
                SetEnteredTestWithoutClearingValue(recordName);
            }
        }

        private void SetEnteredTestWithoutClearingValue(string recordName)
        {
            _enteredText = recordName;
            OnPropertyChanged("EnteredText");
        }

        private IRecordService LookupService { get; set; }

        private string _referencedRecordType;

        public string RecordTypeToLookup
        {
            get { return _referencedRecordType; }
            set
            {
                _referencedRecordType = value;
                LookupGridViewModel = new LookupGridViewModel(LookupService, RecordTypeToLookup, RecordEntryViewModel,
                    OnRecordSelected);
            }
        }

        private string _enteredText;

        public string EnteredText
        {
            get { return _enteredText; }
            set
            {
                _enteredText = value;
                Value = null;
                OnPropertyChanged("EnteredText");
            }
        }

        public XrmButtonViewModel XrmButton { get; set; }

        private bool _lookupGridVisible;

        public bool LookupGridVisible
        {
            get { return _lookupGridVisible; }
            set
            {
                _lookupGridVisible = value;
                OnPropertyChanged("LookupGridVisible");
            }
        }

        private LookupGridViewModel _lookupGridViewModel;

        public LookupGridViewModel LookupGridViewModel
        {
            get { return _lookupGridViewModel; }
            set
            {
                _lookupGridViewModel = value;
                OnPropertyChanged("LookupGridViewModel");
            }
        }

        private int MaxRecordsForLookup
        {
            get { return 15; }
        }


        private bool _searching;

        public bool Searching
        {
            get { return _searching; }
            set
            {
                _searching = value;
                OnPropertyChanged("Searching");
            }
        }

        private object _searchLock = new object();
        private void LoadRowsAsync()
        {
            lock (_searchLock)
            {
                LookupGridVisible = false;
                Searching = true;
                var records =
                    EnteredText.IsNullOrWhiteSpace()
                        ? LookupService.GetFirstX(RecordTypeToLookup, MaxRecordsForLookup)
                        : LookupService.RetrieveMultiple(
                            RecordTypeToLookup,
                            EnteredText,
                            MaxRecordsForLookup
                            );

                SendToDispatcher(() =>
                {
                    LookupGridViewModel.GridRecords = GridRowViewModel.LoadRows(records, LookupGridViewModel);
                    OnPropertyChanged("LookupGridViewModel");
                    Searching = false;
                    LookupGridVisible = LookupGridViewModel.GridRecords.Any();
                });
            }
        }

        public void SelectLookupGrid()
        {
            if (LookupGridVisible)
            {
                //move logical and key foucs to the grid view
                //have to add in the false focus first to ensire the ui properly processes the cvhange to focus on it
                LookupGridViewModel.IsFocused = false;
                LookupGridViewModel.MoveDown();
                LookupGridViewModel.IsFocused = true;
            }
        }
    }
}