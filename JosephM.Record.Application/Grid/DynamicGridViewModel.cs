#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;

#endregion

namespace JosephM.Application.ViewModel.Grid
{
    /// <summary>
    ///     Container For Properties Required By A IDynamicGridViewModel Without Having To Implement Them For Each Concrete
    ///     Type
    /// </summary>
    public class DynamicGridViewModel : INotifyPropertyChanged
    {
        public DynamicGridViewModel(IApplicationController applicationController)
        {
            ApplicationController = applicationController;
            OnDoubleClick = () => { };
            OnKeyDown = () => { };
            PreviousPageButton = new XrmButtonViewModel("Prev", () =>
            {
                if (PreviousPageButton.Enabled)
                    --CurrentPage;
            }, ApplicationController)
            {
                Enabled = false
            };
            NextPageButton = new XrmButtonViewModel("Next", () =>
            {
                if (NextPageButton.Enabled)
                    ++CurrentPage;
            }, ApplicationController)
            {
                Enabled = false
            };
        }

        public void LoadGridButtons(IEnumerable<CustomGridFunction> functions)
        {
            ApplicationController.DoOnMainThread(() =>
            {
                if (functions == null)
                    functions = new CustomGridFunction[0];
                _customFunctions =
                    new ObservableCollection<XrmButtonViewModel>(functions.Select(cf =>
                        new XrmButtonViewModel(cf.Label, () => cf.Function(),
                            ApplicationController)));
                OnPropertyChanged("CustomFunctions");
            });
        }

        private ObservableCollection<XrmButtonViewModel> _customFunctions;

        public ObservableCollection<XrmButtonViewModel> CustomFunctions
        {
            get { return _customFunctions; }
        }

        public bool HasPaging
        {
            get { return PageSize > 0; }
        }

        public string PageDescription
        {
            get
            {
                if (HasPaging)
                {
                    var description = new StringBuilder();
                    description.Append(string.Format("Page {0}: ", CurrentPage));
                    if (GridRecords == null || !GridRecords.Any())
                        description.Append("No Records");
                    else
                        description.Append(string.Format("Displaying Records {0} to {1}", CurrentPageFloor + 1,
                            CurrentPageFloor + GridRecords.Count));
                    return description.ToString();
                }
                else
                    return null;
            }
        }

        private bool _hasMoreRows;
        public bool HasMoreRows
        {
            get
            {
                return _hasMoreRows;
            }
            set
            {
                _hasMoreRows = value;
                NextPageButton.Enabled = _hasMoreRows;
            }
        }

        public int PageSize { get; set; }
        private int _currentPage = 1;
        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                //okay need to refresh the set of rows
                ReloadGrid();
                OnPropertyChanged("CurrentPage");
                OnPropertyChanged("PageDescription");
                PreviousPageButton.Enabled = _currentPage > 1;
            }
        }

        public int CurrentPageCeiling
        {
            get
            {
                return CurrentPage * PageSize;
            }
        }
        public int CurrentPageFloor
        {
            get
            {
                return CurrentPageCeiling - PageSize;
            }
        }

        public XrmButtonViewModel PreviousPageButton { get; set; }
        public XrmButtonViewModel NextPageButton { get; set; }
        
        public bool CanDelete { get { return DeleteRow != null; } }
        public bool CanEdit { get { return EditRow != null; } }
        public Action<GridRowViewModel> DeleteRow { get; set; }
        public Action<GridRowViewModel> EditRow { get; set; }
        public Action OnDoubleClick { get; set; }
        public Action OnKeyDown { get; set; }

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

        public bool CanAddRow { get { return AddRow != null; } }
        private Action _addRow;

        public Action AddRow
        {
            get { return _addRow; }
            set
            {
                _addRow = value;
                AddRowButton = _addRow == null ? null : new XrmButtonViewModel("Add", _addRow, ApplicationController);
            }
        }
        public XrmButtonViewModel AddRowButton { get; set; }

        public string LastSortField { get; set; }
        public bool LastSortAscending { get; set; }

        public SortExpression GetLastSortExpression()
        {
            return LastSortField.IsNullOrWhiteSpace()
                ? null
                : new SortExpression(LastSortField,
                    LastSortAscending ? SortType.Ascending : SortType.Descending);
        }

        public IApplicationController ApplicationController { get; set; }

        /// <summary>
        /// sorts the grid
        /// </summary>
        /// <param name="sortField"></param>
        public void SortIt(string sortField)
        {
            //just copy all into a new list (sorting as go), clear the collection then add each sorted item
            if (GridRecords.Any())
            {
                var newList = new List<GridRowViewModel>();
                foreach (var item in GridRecords)
                {
                    var value1 = item.GetFieldViewModel(sortField).ValueObject;
                    if (value1 == null)
                    {
                        newList.Insert(0, item);
                        continue;
                    }
                    foreach (var item2 in newList)
                    {
                        var value2 = item2.GetFieldViewModel(sortField).ValueObject;
                        if (value2 == null)
                        {
                            continue;
                        }
                        else if (!(value1 is Enum) && value1 is IComparable)
                        {
                            if (((IComparable)value1).CompareTo(value2) < 0)
                            {
                                newList.Insert(newList.IndexOf(item2), item);
                                break;
                            }
                            else
                                continue;
                        }
                        var sortString1 = value1.ToString();
                        var sortString2 = value2.ToString();
                        if (value1 is Enum)
                            sortString1 = ((Enum) value1).GetDisplayString();
                        if (value2 is Enum)
                            sortString2 = ((Enum)value2).GetDisplayString();
                        if (String.Compare(sortString1, sortString2, StringComparison.Ordinal) < 0)
                        {
                            newList.Insert(newList.IndexOf(item2), item);
                            break;
                        }
                    }
                    if(!newList.Contains(item))
                        newList.Add(item);
                }
                //just a check for already sorted ascending the sort descending
                if (LastSortField != sortField)
                    LastSortAscending = true;
                else
                {
                    if (LastSortAscending)
                        newList.Reverse();
                    LastSortAscending = !LastSortAscending;
                }
                LastSortField = sortField;

                if (HasPaging)
                {
                    CurrentPage = 1;
                }
                else
                {
                    GridRecords.Clear();
                    foreach (var item in newList)
                        GridRecords.Add(item);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ReloadGrid()
        {
            if (OnReloading != null)
                OnReloading();

            var getRecordsResponse = GetGridRecords(false);
            var records = getRecordsResponse.Records;
            
            ApplicationController.DoOnMainThread(() =>
            {
                try
                {
                    GridRecords = GridRowViewModel.LoadRows(records, this);
                    OnPropertyChanged("PageDescription");
                    HasMoreRows = getRecordsResponse.HasMoreRecords;
                }
                catch (Exception ex)
                {
                    GridLoadError = true;
                    ErrorMessage = string.Format("There was an error loading data into the grid: {0}", ex.DisplayString());
                }
                if (LoadedCallback != null)
                    LoadedCallback();
            });
        }


        public Action LoadedCallback { get; set; }
        public Action OnReloading { get; set; }

        private bool _gridLoadError;
        public bool GridLoadError
        {
            get { return _gridLoadError; }
            set
            {
                _gridLoadError = value;
                OnPropertyChanged("GridLoadError");
            }
        }

        public Func<bool, GetGridRecordsResponse> GetGridRecords { get; set; }

        public IRecordService RecordService { get; set; }
        public string RecordType { get; set; }
        public ViewType ViewType { get; set; }

        private ObservableCollection<GridRowViewModel> _records;
        public ObservableCollection<GridRowViewModel> GridRecords
        {
            get
            {
                if (_records == null && LoadRecordsAsync)
                {
                    ApplicationController.DoOnAsyncThread(ReloadGrid);
                }
                return _records;
            }
            set
            {
                _records = value;
                OnPropertyChanged("GridRecords");
            }
        }

        public bool IsReadOnly { get; set; }

        private IEnumerable<GridFieldMetadata> _recordFields;
        private readonly object _lockthis = new object();
        public IEnumerable<GridFieldMetadata> RecordFields
        {
            get
            {
                lock (_lockthis)
                {
                    if (_recordFields == null && RecordService != null)
                    {
                        _recordFields =
                            RecordType == null
                            ? new GridFieldMetadata[0]
                            : RecordService.GetGridFields(RecordType, ViewType);
                    }
                }
                return _recordFields;
            }
            set { _recordFields = value; }
        }

        public FormController FormController { get; set; }

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

        public RecordEntry.Form.RecordEntryViewModelBase ParentForm { get; set; }

        public string ReferenceName { get; set; }

        public bool LoadRecordsAsync { get; set; }

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

        public IDictionary<string, IEnumerable<string>> OnlyValidate { get; internal set; }
    }

    public class GetGridRecordsResponse
    {
        public bool HasMoreRecords { get; set; }
        public IEnumerable<IRecord> Records { get; set; }

        public GetGridRecordsResponse(IEnumerable<IRecord> records, bool hasMoreRecords)
        {
            HasMoreRecords = hasMoreRecords;
            Records = records;
        }

        public GetGridRecordsResponse(IEnumerable<IRecord> records)
        {
            Records = records;
        }
    }
}
