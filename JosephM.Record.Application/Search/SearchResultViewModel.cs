using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.IService;

namespace JosephM.Record.Application.Search
{
    public class SearchResultViewModel : ViewModelBase, IDynamicGridViewModel
    {
        private IEnumerable<IRecord> Records { get; set; }

        public SearchResultViewModel(IEnumerable<IRecord> records, IRecordService recordService, IApplicationController controller, int maxColumns)
            : base(controller)
        {
            if (maxColumns > 1)
                _maxColumns = maxColumns;
            Records = records;
            RecordService = recordService;
            FormController = new FormController(RecordService, null, controller);
            GridRecords = GridRowViewModel.LoadRows(records, this);
            DynamicGridViewModelItems = new DynamicGridViewModelItems();
        }
        public IRecordService RecordService { get; private set; }

        private string _heading = "Search Results";
        public string Heading { get { return _heading; }}

        private int _maxColumns = 250;
        private IEnumerable<GridFieldMetadata> _recordFields;
        private object _lockthis = new object();
        public IEnumerable<GridFieldMetadata> RecordFields
        {
            get
            {
                lock (_lockthis)
                {
                    if (_recordFields == null)
                    {
                        var recordFields =
                            RecordService.GetFields(RecordType).Select(f => new GridFieldMetadata(f)).ToArray();
                        if (recordFields.Count() > _maxColumns)
                        {
                            _heading = _heading + string.Format(" (A Maximum Of {0} Fields Are Displayed", _maxColumns);
                            OnPropertyChanged("Heading");
                        }
                        if (recordFields.Count() > _maxColumns)
                            _recordFields = recordFields.Take(_maxColumns).ToArray();
                        else
                            _recordFields = recordFields;
                    }
                }
                return _recordFields;
            }
        }



        public string RecordType
        {
            get { return Records.Any() ? Records.First().Type : "UnknownNoRecords"; }
        }
        public ObservableCollection<GridRowViewModel> GridRecords { get; private set; }
        public GridRowViewModel SelectedRow { get; set; }
        public void DoOnMainThread(Action action)
        {
            ApplicationController.DoOnMainThread(action);
        }

        public void DoOnAsynchThread(Action action)
        {
            ApplicationController.DoOnAsyncThread(action);
        }

        public void DoWhileLoading(string message, Action action)
        {
            action();
        }

        public FormController FormController { get; private set; }
        public bool IsReadOnly { get { return true; } }
        public DynamicGridViewModelItems DynamicGridViewModelItems { get; private set; }
    }
}
