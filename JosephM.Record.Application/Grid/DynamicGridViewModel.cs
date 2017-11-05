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
using System.Threading;
using JosephM.Core.Utility;
using System.IO;
using JosephM.Record.Extentions;
using JosephM.Application.ViewModel.Dialog;

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
            LoadingViewModel = new LoadingViewModel(applicationController);
            OnDoubleClick = () => { };
            OnClick = () => { };
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
            MaxHeight = 600;
            LoadDialog = (d) => { ApplicationController.UserMessage(string.Format("Error The {0} Method Has Not Been Set In This Context", nameof(LoadDialog))); };
        }

        public int MaxHeight { get; set; }

        public XrmButtonViewModel GetButton(string id)
        {
            if (CustomFunctions.Any(b => b.Id == id))
                return CustomFunctions.First(b => b.Id == id);
            if (CustomFunctions.Where(b => b.HasChildOptions).SelectMany(b => b.ChildButtons).Any(b => b.Id == id))
                return CustomFunctions.Where(b => b.HasChildOptions).SelectMany(b => b.ChildButtons).First(b => b.Id == id);
            throw new ArgumentOutOfRangeException("id", "No Button Found With Id Of " + id);
        }

        public LoadingViewModel LoadingViewModel { get; set; }

        public void RefreshGridButtons()
        {
            ApplicationController.DoOnMainThread(() =>
            {
                _customFunctions =
                    new ObservableCollection<XrmButtonViewModel>(GridsFunctionsToXrmButtons(_loadedGridButtons.ToArray()));
                
                OnPropertyChanged(nameof(CustomFunctions));
            });
        }

        private IEnumerable<XrmButtonViewModel> GridsFunctionsToXrmButtons(IEnumerable<CustomGridFunction> functions)
        {
            var buttons = new List<XrmButtonViewModel>();
            foreach(var cf in functions.ToArray())
            {
                var isVisible = cf.VisibleFunction(this);
                if (isVisible)
                {
                    if (cf.ChildGridFunctions != null && cf.ChildGridFunctions.Any())
                    {
                        var childButtons = GridsFunctionsToXrmButtons(cf.ChildGridFunctions);
                        buttons.Add(new XrmButtonViewModel(cf.Id, cf.Label, childButtons, ApplicationController));
                    }
                    else
                    {
                        buttons.Add(new XrmButtonViewModel(cf.Id, cf.Label, () => cf.Function(this), ApplicationController));
                    }
                }
            }
            return buttons;
        }

        public void AddGridButtons(IEnumerable<CustomGridFunction> gridButtons)
        {
            _loadedGridButtons.AddRange(gridButtons);
            RefreshGridButtons();
        }

        private List<CustomGridFunction> _loadedGridButtons = new List<CustomGridFunction>();

        public void OnSelectionsChanged()
        {
            OnClick();
            RefreshGridButtons();

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

        public bool MultiSelect { get; set; }

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
                        description.Append(string.Format("Records {0} to {1}", CurrentPageFloor + 1,
                            CurrentPageFloor + GridRecords.Count));
                    return description.ToString();
                }
                else
                    return null;
            }
        }

        public bool IsNotFirstPage
        {
            get
            {
                return CurrentPage != 1;
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
                OnPropertyChanged(nameof(HasMoreRows));
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
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(IsNotFirstPage));
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
        public Action OnClick { get; set; }
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
                OnPropertyChanged(nameof(CanAddRow));
                OnPropertyChanged(nameof(AddRowButton));
            }
        }
        public XrmButtonViewModel AddRowButton { get; set; }

        public bool CanAddMultipleRow { get { return AddMultipleRow != null; } }
        private Action _addMultipleRow;

        public Action AddMultipleRow
        {
            get { return _addMultipleRow; }
            set
            {
                _addMultipleRow = value;
                
                AddMultipleRowButton = _addMultipleRow == null
                    ? null
                    : new XrmButtonViewModel("Add Multiple", _addMultipleRow, ApplicationController);
            }
        }
        public XrmButtonViewModel AddMultipleRowButton { get; set; }

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
            ApplicationController.DoOnAsyncThread(() =>
            {
                try
                {
                    GridLoadError = false;
                    GridLoaded = false;
                    LoadingViewModel.IsLoading = true;
                    Thread.Sleep(100);
                    if (OnReloading != null)
                        OnReloading();
                    var getRecordsResponse = GetGridRecords(false);
                    var records = getRecordsResponse.Records;

                    ApplicationController.DoOnMainThread(() =>
                    {
                        try
                        {
                            GridRecords = GridRowViewModel.LoadRows(records, this);
                            OnPropertyChanged(nameof(PageDescription));
                            HasMoreRows = getRecordsResponse.HasMoreRecords;
                        }
                        catch (Exception ex)
                        {
                            GridLoadError = true;
                            ErrorMessage = string.Format("There was an error loading data into the grid: {0}", ex.DisplayString());
                        }
                        LoadingViewModel.IsLoading = false;
                        if (!GridLoadError)
                            GridLoaded = true;
                        if (LoadedCallback != null)
                            LoadedCallback();
                    });
                }
                catch (Exception ex)
                {
                    GridLoadError = true;
                    ErrorMessage = string.Format("There was an error loading data into the grid: {0}", ex.DisplayString());
                    if (LoadedCallback != null)
                        LoadedCallback();
                    LoadingViewModel.IsLoading = false;
                }
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
                OnPropertyChanged(nameof(GridLoadError));
            }
        }

        private bool _gridLoaded = true;
        public bool GridLoaded
        {
            get { return _gridLoaded; }
            set
            {
                _gridLoaded = value;
                OnPropertyChanged(nameof(GridLoaded));
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
                OnPropertyChanged(nameof(GridRecords));
                RefreshGridButtons();
            }
        }

        public bool IsReadOnly { get; set; }

        private IEnumerable<GridFieldMetadata> _fieldMetadata;
        private readonly object _lockthis = new object();
        public IEnumerable<GridFieldMetadata> FieldMetadata
        {
            get
            {
                lock (_lockthis)
                {
                    if (_fieldMetadata == null && RecordService != null)
                    {
                        _fieldMetadata =
                            RecordType == null
                            ? new GridFieldMetadata[0]
                            : RecordService.GetGridFields(RecordType, ViewType);
                    }
                }
                return _fieldMetadata;
            }
            set { _fieldMetadata = value; }
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

        public IEnumerable<GridRowViewModel> SelectedRows
        {
            get { return GridRecords == null ? new GridRowViewModel[0] :  GridRecords.Where(r => r.IsSelected).ToArray(); }
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
        public Action<DialogViewModel> LoadDialog { get; set; }

        public void DownloadCsv()
        {
            var newFileName = ApplicationController.GetSaveFileName(string.Format("{0}", RecordService.GetCollectionName(RecordType)), "csv");
            ApplicationController.DoOnAsyncThread(() =>
            {
                try
                {
                    LoadingViewModel.IsLoading = true;
                    if (!string.IsNullOrWhiteSpace(newFileName))
                    {

                        var folder = Path.GetDirectoryName(newFileName);
                        var fileName = Path.GetFileName(newFileName);
                        var fields = FieldMetadata.Select(rf => rf.FieldName);
                        CsvUtility.CreateCsv(folder, fileName, GetGridRecords(true).Records, fields, (f) => RecordService.GetFieldLabel(f, RecordType), (r, f) => { return RecordService.GetFieldAsDisplayString((IRecord)r, f); });
                        ApplicationController.StartProcess(folder);
                    }
                }
                catch (Exception ex)
                {
                    ApplicationController.UserMessage("Error Downloading CSV: " + ex.DisplayString());
                }
                finally
                {
                    LoadingViewModel.IsLoading = false;
                }
            });
        }
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
