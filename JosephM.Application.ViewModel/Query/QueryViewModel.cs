using JosephM.Application.Application;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace JosephM.Application.ViewModel.Query
{
    public class QueryViewModel : TabAreaViewModelBase, INotifyPropertyChanged
    {
        public QueryViewModel(IEnumerable<string> recordTypes, IRecordService recordService, IApplicationController controller, bool allowQuery = false, bool loadInitially = false, CustomGridFunction closeFunction = null
            , IEnumerable<CustomGridFunction> customFunctions = null, bool allowCrud = true)
            : base(controller)
        {
            AllowCrud = allowCrud;
            CustomFunctions = customFunctions;
            LoadInitially = loadInitially;
            AllowQuery = allowQuery;
            RecordService = recordService;
            if (closeFunction != null)
                ReturnButton = new XrmButtonViewModel(closeFunction.LabelFunc(null), () => { closeFunction.Function(DynamicGridViewModel); }, controller);
            QueryTypeButton = new XrmButtonViewModel("Change Query Type", ChangeQueryType, ApplicationController);

            DeleteSelectedConditionsButton = new XrmButtonViewModel("Delete Selected", () => DeleteSelected(), ApplicationController);
            GroupSelectedConditionsOr = new XrmButtonViewModel("Group Selected Or", () => GroupSelected(FilterOperator.Or), ApplicationController);
            GroupSelectedConditionsAnd = new XrmButtonViewModel("Group Selected And", () => GroupSelected(FilterOperator.And), ApplicationController);
            UngroupSelectedConditions = new XrmButtonViewModel("Ungroup Selected", () => UnGroupSelected(), ApplicationController);

            NotInDeleteSelectedConditionsButton = new XrmButtonViewModel("Delete Selected", () => DeleteSelected(isNotIn: true), ApplicationController);
            NotInGroupSelectedConditionsOr = new XrmButtonViewModel("Group Selected Or", () => GroupSelected(FilterOperator.Or, isNotIn: true), ApplicationController);
            NotInGroupSelectedConditionsAnd = new XrmButtonViewModel("Group Selected And", () => GroupSelected(FilterOperator.And, isNotIn: true), ApplicationController);
            NotInUngroupSelectedConditions = new XrmButtonViewModel("Ungroup Selected", () => UnGroupSelected(isNotIn: true), ApplicationController);

            RunQueryButton = new XrmButtonViewModel("Run Query", QuickFind, ApplicationController);
            IncludeNotInButton = new XrmButtonViewModel("Add Not In Query", NotInSwitch, ApplicationController);
            ChangeQueryType();

            QueryTypeButton.IsVisible = AllowQuery;

            _recordTypes = recordTypes;
            if (_recordTypes.Count() == 1)
                RecordType = _recordTypes.First();
        }

        private void NotInSwitch()
        {
            IncludeNotIn = !IncludeNotIn;
            IncludeNotInButton.Label = IncludeNotIn ? "Remove Not In Query" : "Add Not In Query";
        }

        public bool AllowCrud { get; set; }

        private IEnumerable<CustomGridFunction> CustomFunctions { get; set; }
        private bool LoadInitially { get; set; }

        public bool TypeAhead { get; set; }

        private bool _queryRun;
        public bool QueryRun
        {
            get
            {
                return _queryRun;
            }
            set
            {
                _queryRun = value;
                OnPropertyChanged(nameof(QueryRun));
                OnPropertyChanged(nameof(QuickFindOrNotQueryRun));
                OnPropertyChanged(nameof(QueryAndNotRun));
            }
        }

        private bool _includeNotIn;
        public bool IncludeNotIn
        {
            get
            {
                return _includeNotIn;
            }
            set
            {
                _includeNotIn = value;
                OnPropertyChanged(nameof(IncludeNotIn));
            }
        }

        public bool QueryAndNotRun
        {
            get
            {
                return !IsQuickFind && !QueryRun;
            }
        }

        public bool QuickFindOrNotQueryRun
        {
            get
            {
                return IsQuickFind || !QueryRun;
            }
        }

        public void RecreateGrid(bool resetQueryRun = true)
        {
            if (RecordType != null)
            {
                DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
                {
                    PageSize = StandardPageSize,
                    ViewType = ViewType.MainApplicationView,
                    RecordService = RecordService,
                    RecordType = RecordType,
                    IsReadOnly = true,
                    FormController = new FormController(RecordService, null, ApplicationController),
                    GetGridRecords = (b) => GetGridRecords(b),
                    DisplayTotalCount = false,
                    GetTotalCount = GetGridTotalCount,
                    MultiSelect = true,
                    GridLoaded = false,
                    FieldMetadata = ExplicitlySelectedColumns
                };
                var customFunctionList = new List<CustomGridFunction>()
                {
                    new CustomGridFunction("QUERY", "Run Query", QuickFind),
                    new CustomGridFunction("BACKTOQUERY", "Back To Query", (g) => { ResetToQueryEntry(); }, (g) => !IsQuickFind && QueryRun),
                    new CustomGridFunction("EDITCOLUMNS", "Edit Columns", (g) => LoadColumnEdit(), (g) => DynamicGridViewModel != null),
                    new CustomGridFunction("DISPLAYTOTALS", (g) => g.DisplayTotalCount ? "Hide Totals" : "Display Totals", (g) => { g.DisplayTotalCount = !g.DisplayTotalCount; g.ReloadGrid(); }, (g) => DynamicGridViewModel != null)
                };
                if (FormService != null && AllowCrud)
                {
                    DynamicGridViewModel.EditRow = (g) =>
                    {
                        var formMetadata = FormService.GetFormMetadata(RecordType, RecordService);
                        var formController = new FormController(RecordService, FormService, ApplicationController);
                        var selectedRow = g;
                        if (selectedRow != null)
                        {
                            Action onSave = () =>
                            {
                                ClearChildForm();
                                ClearNotInIds();
                                DynamicGridViewModel.ReloadGrid();
                            };
                            var record = selectedRow.Record;
                            var createOrUpdateRecord = RecordService.Get(record.Type, record.Id);
                            new[] { createOrUpdateRecord }.PopulateEmptyLookups(RecordService, null);
                            var newForm = new CreateOrUpdateViewModel(createOrUpdateRecord, formController, onSave, ClearChildForm);
                            ClearNotInIds();
                            LoadChildForm(newForm);
                        }
                    };
                    DynamicGridViewModel.EditRowNew = (g) =>
                    {
                        var formMetadata = FormService.GetFormMetadata(RecordType, RecordService);
                        var formController = new FormController(RecordService, FormService, ApplicationController);
                        var selectedRow = g;
                        if (selectedRow != null)
                        {
                            var record = selectedRow.Record;
                            CreateOrUpdateViewModel vmRef = null;
                            var createOrUpdateRecord = RecordService.Get(record.Type, record.Id);
                            new[] { createOrUpdateRecord }.PopulateEmptyLookups(RecordService, null);
                            vmRef = new CreateOrUpdateViewModel(createOrUpdateRecord, formController, () => {
                                vmRef.ValidationPrompt = "Changes Have Been Saved";
                                ClearNotInIds();
                                DynamicGridViewModel.ReloadGrid();
                            }, () => ApplicationController.Remove(vmRef), cancelButtonLabel: "Close");
                            ApplicationController.NavigateTo(vmRef);
                            //LoadChildForm(newForm);
                        }
                    };
                    DynamicGridViewModel.AddRow = () =>
                    {
                        var formMetadata = FormService.GetFormMetadata(RecordType, RecordService);
                        var formController = new FormController(RecordService, FormService, ApplicationController);
                        Action onSave = () =>
                        {
                            ClearChildForm();
                            ClearNotInIds();
                            DynamicGridViewModel.ReloadGrid();
                        };
                        var newForm = new CreateOrUpdateViewModel(RecordService.NewRecord(RecordType), formController, onSave, ClearChildForm, explicitIsCreate: true);
                        LoadChildForm(newForm);
                    };
                }
                customFunctionList.Add(new CustomGridFunction("CSV", "Download CSV", (g) => g.DownloadCsv(), (g) => g.GridRecords != null && g.GridRecords.Any()));

                if (CustomFunctions != null)
                {
                    foreach (var item in CustomFunctions)
                        customFunctionList.Add(item);
                }

                DynamicGridViewModel.AddGridButtons(customFunctionList);
                if (LoadInitially)
                    DynamicGridViewModel.ReloadGrid();

                RefreshConditionButtons();
                RefreshConditionButtons(isNotIn: true);

                if (resetQueryRun)
                    QueryRun = false;
            }
        }

        private void LoadColumnEdit()
        {
            LoadingViewModel.IsLoading = true;
            DoOnMainThread(() =>
            {
                try
                {
                    //okay lets spawn a dialog for editing the columns in the grid
                    var currentColumns = DynamicGridViewModel.FieldMetadata
                        .ToArray();

                    Action<IEnumerable<GridFieldMetadata>> letsLoadTheColumns = (newColumnSet) =>
                    {
                        LoadingViewModel.IsLoading = true;
                        DoOnMainThread(() =>
                        {
                            try
                            {
                                ExplicitlySelectedColumns = newColumnSet.ToList();
                                ClearChildForm();
                                RecreateGrid();
                                QuickFind();
                            }
                            catch (Exception ex)
                            {
                                ApplicationController.ThrowException(ex);
                            }
                            finally
                            {
                                LoadingViewModel.IsLoading = false;
                            }
                        });
                    };

                    var columnEditDialog = new ColumnEditDialogViewModel(RecordType, currentColumns, RecordService, letsLoadTheColumns, ClearChildForm, ApplicationController, allowLinkedFields: true);
                    LoadChildForm(columnEditDialog);
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
                finally
                {
                    ApplicationController.DoOnAsyncThread(() => LoadingViewModel.IsLoading = false);
                }
            });
        }


        private List<GridFieldMetadata> ExplicitlySelectedColumns
        {
            get; set;
        }

        public void ResetToQueryEntry()
        {
            QueryRun = false;
            ClearNotInIds();
            RecreateGrid();
        }

        private bool AllowQuery { get; set; }

        private FilterConditionsViewModel CreateFilterCondition(bool isNotIn = false)
        {
            return new FilterConditionsViewModel(RecordType, RecordService, ApplicationController, () => RefreshConditionButtons(isNotIn: isNotIn));
        }

        private JoinsViewModel CreateJoins(bool isNotIn = false)
        {
            return new JoinsViewModel(RecordType, RecordService, ApplicationController, () => RefreshConditionButtons(isNotIn: isNotIn));
        }

        private void GroupSelected(FilterOperator filterOperator, bool isNotIn = false)
        {
            var filterConditions = isNotIn ? NotInFilterConditions : FilterConditions;
            filterConditions?.GroupSelected(filterOperator);
            var joins = isNotIn ? NotInJoins : Joins;
            if (joins != null && joins.Joins != null)
            {
                foreach (var join in joins.Joins)
                {
                    join.GroupSelected(filterOperator);
                }
            }
            RefreshConditionButtons(isNotIn: isNotIn);
        }

        public void RefreshConditionButtons(FilterConditionsViewModel filter = null, bool isRootFilter = true, bool processJoins = true, JoinsViewModel joins = null, bool isNotIn = false)
        {
            joins = joins ?? (isNotIn ? NotInJoins : Joins);
            var deleteButton = isNotIn ? NotInDeleteSelectedConditionsButton : DeleteSelectedConditionsButton;
            var groupAndButton = isNotIn ? NotInGroupSelectedConditionsAnd : GroupSelectedConditionsAnd;
            var groupOrButton = isNotIn ? NotInGroupSelectedConditionsOr : GroupSelectedConditionsOr;
            var ungroupButton = isNotIn ? NotInUngroupSelectedConditions : UngroupSelectedConditions;

            if (filter == null)
            {
                deleteButton.IsVisible = false;
                groupAndButton.IsVisible = false;
                groupOrButton.IsVisible = false;
                ungroupButton.IsVisible = false;
                filter = (isNotIn ? NotInFilterConditions : FilterConditions);
                if (!AllowQuery)
                    return;
            }
            if (filter != null)
            {
                var selectedCount = filter.SelectedConditions.Count();
                if (selectedCount > 0)
                    deleteButton.IsVisible = true;
                if (selectedCount > 0 && !isRootFilter)
                    ungroupButton.IsVisible = true;
                if (selectedCount > 1 && filter.FilterOperator == FilterOperator.And)
                    groupOrButton.IsVisible = true;
                if (selectedCount > 1 && filter.FilterOperator == FilterOperator.Or)
                    groupAndButton.IsVisible = true;
                foreach (var item in filter.FilterConditions)
                {
                    RefreshConditionButtons(item, isRootFilter: false, processJoins: false, isNotIn: isNotIn);
                }
                if (processJoins && joins != null && joins.Joins != null)
                {
                    foreach (var join in joins.Joins)
                    {
                        if (join.FilterConditions != null)
                        {
                            RefreshConditionButtons(join.FilterConditions, isRootFilter: true, processJoins: join.Joins != null, joins: join.Joins, isNotIn: isNotIn);
                        }
                    }
                }
            }
        }

        private HashSet<string> _cachedNotinIds;

        private DateTime? _notInQueryStartTime;

        private object _lockObject = new object();
        private HashSet<string> GetNotInIds(out DateTime notInQueryStartTime)
        {
            lock (_lockObject)
            {
                if (_cachedNotinIds == null)
                {
                    try
                    {
                        _notInQueryStartTime = DateTime.UtcNow;
                        _cachedNotinIds = new HashSet<string>();
                        var notInQuery = GenerateNotInQuery();
                        var loadingVm = DynamicGridViewModel.LoadingViewModel;
                        DynamicGridViewModel.RecordService.ProcessResults(notInQuery, (r) =>
                        {
                            foreach (var item in r)
                                _cachedNotinIds.Add(item.Id);
                            if (loadingVm != null)
                                loadingVm.LoadingMessage = "Loading Not In Ids - " + _cachedNotinIds.Count;
                        });
                        if (loadingVm != null)
                            loadingVm.LoadingMessage = "Please Wait While Loading";
                    }
                    catch(Exception)
                    {
                        ClearNotInIds();
                        throw;
                    }
                }
                notInQueryStartTime = _notInQueryStartTime ?? DateTime.UtcNow;
                return _cachedNotinIds;
            }
        }

        public void ClearNotInIds()
        {
            _notInQueryStartTime = null;
            _cachedNotinIds = null;
        }


        private void UnGroupSelected(bool isNotIn = false)
        {
            var filterConditions = isNotIn ? NotInFilterConditions : FilterConditions;
            filterConditions?.UnGroupSelected(null);
            var joins = isNotIn ? NotInJoins : Joins;
            if (joins != null && joins.Joins != null)
            {
                foreach (var join in joins.Joins)
                {
                    join.UngroupSelectedConditions();
                }
            }
            RefreshConditionButtons(isNotIn: isNotIn);
        }

        private void DeleteSelected(bool isNotIn = false)
        {
            var filterConditions = isNotIn ? NotInFilterConditions : FilterConditions;
            filterConditions?.DeleteSelected(null);
            var joins = isNotIn ? NotInJoins : Joins;
            if (joins != null && joins.Joins != null)
            {
                foreach(var join in joins.Joins)
                {
                    join.DeleteSelectedConditions();
                }
            }
            RefreshConditionButtons(isNotIn: isNotIn);
        }

        private bool _isQuickFind;
        public bool IsQuickFind
        {
            get
            {
                return _isQuickFind;
            }
            set
            {
                _isQuickFind = value;
                OnPropertyChanged(nameof(IsQuickFind));
                if (!IsQuickFind)
                    ResetToQueryEntry();
                OnPropertyChanged(nameof(QuickFindOrNotQueryRun));
                OnPropertyChanged(nameof(QueryAndNotRun));
            }
        }

        private void ChangeQueryType()
        {
            IsQuickFind = !IsQuickFind;
            QueryTypeButton.Label = IsQuickFind ? "Use Query" : "Use Quick Find";
            IncludeNotInButton.IsVisible = !IsQuickFind;
            if (!IsQuickFind)
            {
                CreateFilterCondition();
                CreateJoins();
            }
        }

        public void QuickFind()
        {
            var isValid = ValidateCurrentSearch();
            if (!isValid)
                return;
            DynamicGridViewModel.ReloadGrid();
            QueryRun = true;
        }

        private bool ValidateCurrentSearch()
        {
            var result = true;
            if (!IsQuickFind)
            {
                result = FilterConditions.Validate();
                var joinValidate = Joins.Validate();
                if (!joinValidate)
                    result = result && joinValidate;

                if (IncludeNotIn)
                {
                    var notInFilterValidate = NotInFilterConditions.Validate();
                    result = result && notInFilterValidate;
                    var notInJoinsValidate = NotInJoins.Validate();
                    result = result && notInJoinsValidate;
                }
            }

            return result;
        }

        private string _heading = "Query";
        public string Heading { get { return _heading; }}

        public void DoWhileLoading(string message, Action action)
        {
            action();
        }

        public IRecordService RecordService { get; private set; }

        private DynamicGridViewModel _dynamicGridViewModel;
        public DynamicGridViewModel DynamicGridViewModel
        {
            get
            {
                return _dynamicGridViewModel;
            }
            set
            {
                _dynamicGridViewModel = value;
                OnPropertyChanged(nameof(DynamicGridViewModel));
            }
        }

        private ObservableCollection<ViewModelBase> _childForms = new ObservableCollection<ViewModelBase>();

        public ObservableCollection<GridRowViewModel> GridRecords
        {
            get { return DynamicGridViewModel?.GridRecords; }
        }

        public int GetGridTotalCount()
        {
            var isValid = ValidateCurrentSearch();
            if (!isValid)
                return 0;
            var query = GenerateQuery();
            query.Fields = new string[0];
            var totalCount = 0;

            var notInList = new HashSet<string>();
            if(IncludeNotIn)
            {
                DateTime notInThreshold;
                notInList = GetNotInIds(out notInThreshold);
                AdjustQueryForCreatedThreshold(query, notInThreshold);
            }

            var loadingVm = DynamicGridViewModel.LoadingViewModel;
            DynamicGridViewModel.RecordService.ProcessResults(query, (r) =>
            {
                totalCount += r.Count(t => !notInList.Contains(t.Id));
                if (loadingVm != null)
                    loadingVm.LoadingMessage = "Getting Total Record Count - " + totalCount;
            });
            if (loadingVm != null)
                loadingVm.LoadingMessage = "Please Wait While Loading";
            return totalCount;
        }

        private static void AdjustQueryForCreatedThreshold(QueryDefinition query, DateTime notInThreshold)
        {
            //if we have a not in query then we want to ensure any records created after the not in set was generated
            //arent included in the main query as they wont have been considered for exlcusion when running the not in query
            var rootFilter = query.RootFilter;
            var newFilter = new Filter();
            newFilter.SubFilters.Add(rootFilter);
            newFilter.AddCondition("createdon", ConditionType.LessThan, notInThreshold);
            query.RootFilter = newFilter;
        }

        public GetGridRecordsResponse GetGridRecords(bool ignorePages, IEnumerable<string> fields = null)
        {
            var isValid = ValidateCurrentSearch();
            if (!isValid)
                return new GetGridRecordsResponse(new IRecord[0]);
            var query = GenerateQuery();
            query.Fields = fields;

            var notInList = new HashSet<string>();
            if (IncludeNotIn)
            {
                DateTime notInThreshold;
                notInList = GetNotInIds(out notInThreshold);
                AdjustQueryForCreatedThreshold(query, notInThreshold);
            }

            if (!DynamicGridViewModel.HasPaging || ignorePages)
            {
                var loadingVm = DynamicGridViewModel.LoadingViewModel;
                var records = new List<IRecord>();
                DynamicGridViewModel.RecordService.ProcessResults(query, (r) =>
                {
                    records.AddRange(r.Where(t => !notInList.Contains(t.Id)));
                    if (loadingVm != null)
                        loadingVm.LoadingMessage = "Running Main Query - " + records.Count;
                });
                if (loadingVm != null)
                    loadingVm.LoadingMessage = "Populating Empty Lookups";
                records.PopulateEmptyLookups(RecordService, null);
                if (loadingVm != null)
                    loadingVm.LoadingMessage = "Please Wait While Loading";
                return new GetGridRecordsResponse(records);
            }
            else
            {
                return DynamicGridViewModel.GetGridRecordPage(query, notInList);
            }
        }

        public QueryDefinition GenerateNotInQuery()
        {
            var query = new QueryDefinition(RecordType);
            query.Distinct = true;
            query.RootFilter = NotInFilterConditions.GetAsFilter();
            query.Joins = NotInJoins.GetAsJoins().ToList();
            query.Fields = new string[0];
            return query;
        }

        public QueryDefinition GenerateQuery()
        {
            var query = new QueryDefinition(RecordType);
            query.Distinct = true;
            if (IsQuickFind)
            {
                query.IsQuickFind = true;
                query.QuickFindText = QuickFindText;
                if (!string.IsNullOrWhiteSpace(QuickFindText))
                {
                    var quickFindFields = RecordService.GetStringQuickfindFields(RecordType);
                    //there was a bug in SDK when querying on queues
                    //which required adding our or filter as a child filter
                    //rather than adding in the root filter
                    var nestedFilter = new Filter();
                    nestedFilter.ConditionOperator = FilterOperator.Or;
                    nestedFilter.Conditions.AddRange(quickFindFields.Select(f => new Condition(f, ConditionType.BeginsWith, QuickFindText)));
                    query.RootFilter.SubFilters.Add(nestedFilter);
                }
            }
            else
            {
                query.RootFilter = FilterConditions.GetAsFilter();
                query.Joins = Joins.GetAsJoins().ToList();
            }
            var view = DynamicGridViewModel.RecordService.GetView(DynamicGridViewModel.RecordType, DynamicGridViewModel.ViewType);
            query.Sorts = view.Sorts.Where(s => !s.FieldName.Contains(".")).ToList();

            //okay lets add joins for all the columns in referenced types
            if (ExplicitlySelectedColumns != null)
            {
                var linkGroups = ExplicitlySelectedColumns
                    .Where(c => c.AliasedFieldName != null)
                    .GroupBy(c => c.AliasedFieldName.Split('.')[0]);
                foreach (var linkGroup in linkGroups)
                {
                    var joinToRecordType = linkGroup.First().AltRecordType;
                    var lookupField = linkGroup.Key.Substring(0, linkGroup.Key.Length - (joinToRecordType.Length + 1));
                    var join = new Join(lookupField, joinToRecordType, RecordService.GetPrimaryKey(joinToRecordType));
                    join.JoinType = JoinType.LeftOuter;
                    join.Fields = linkGroup.Select(lgf => lgf.FieldName);
                    join.Alias = linkGroup.Key;
                    query.Joins.Add(join);
                }
            }

            return query;
        }

        private string _quickFindText;
        public string QuickFindText
        {
            get
            {
                return _quickFindText;
            }
            set
            {
                _quickFindText = value;
                OnPropertyChanged(nameof(QuickFindText));
            }
        }

        public XrmButtonViewModel ReturnButton { get; set; }

        public XrmButtonViewModel QueryTypeButton { get; set; }

        private FilterConditionsViewModel _filterConditions;
        public FilterConditionsViewModel FilterConditions
        {
            get
            {
                return _filterConditions;
            }
            set
            {
                _filterConditions = value;
                OnPropertyChanged(nameof(FilterConditions));
                RefreshConditionButtons();
            }
        }

        private FilterConditionsViewModel _notInFilterConditions;
        public FilterConditionsViewModel NotInFilterConditions
        {
            get
            {
                return _notInFilterConditions;
            }
            set
            {
                _notInFilterConditions = value;
                OnPropertyChanged(nameof(NotInFilterConditions));
            }
        }

        private JoinsViewModel _joins;
        public JoinsViewModel Joins
        {
            get
            {
                return _joins;
            }
            set
            {
                _joins = value;
                OnPropertyChanged(nameof(Joins));
            }
        }

        private JoinsViewModel _notInJoins;
        public JoinsViewModel NotInJoins
        {
            get
            {
                return _notInJoins;
            }
            set
            {
                _notInJoins = value;
                OnPropertyChanged(nameof(NotInJoins));
            }
        }

        public XrmButtonViewModel DeleteSelectedConditionsButton { get; set; }
        public XrmButtonViewModel GroupSelectedConditionsOr { get; set; }
        public XrmButtonViewModel GroupSelectedConditionsAnd { get; set; }
        public XrmButtonViewModel UngroupSelectedConditions { get; set; }
        public XrmButtonViewModel NotInDeleteSelectedConditionsButton { get; set; }
        public XrmButtonViewModel NotInGroupSelectedConditionsOr { get; set; }
        public XrmButtonViewModel NotInGroupSelectedConditionsAnd { get; set; }
        public XrmButtonViewModel NotInUngroupSelectedConditions { get; set; }
        public XrmButtonViewModel RunQueryButton { get; private set; }
        public XrmButtonViewModel IncludeNotInButton { get; private set; }

        private IEnumerable<string> _recordTypes;
        private IEnumerable<RecordType> _recordTypeItemsSource;
        public IEnumerable<RecordType> RecordTypeItemsSource
        {
            get
            {
                if (_recordTypeItemsSource == null)
                {
                    _recordTypeItemsSource = _recordTypes
                        .Select(r => new RecordType(r, RecordService.GetDisplayName(r)))
                        .OrderBy(r => r.Value)
                        .ToArray();
                }
                return _recordTypeItemsSource;
            }
        }

        private RecordType _selectedRecordType;
        public RecordType SelectedRecordType
        {
            get
            {
                return _selectedRecordType;
            }
            set
            {
                _selectedRecordType = value;
                RecordType = value?.Key;
            }
        }

        private string _recordType;
        public string RecordType
        {
            get
            {
                if (_recordType == null && RecordTypeItemsSource.Count() == 1)
                    _recordType = RecordTypeItemsSource.First().Key;
                return _recordType;
            }
            set
            {
                LoadingViewModel.IsLoading = true;
                try
                {
                    if(value != null)
                    {
                        LoadingViewModel.LoadingMessage = $"Loading {RecordService.GetDisplayName(value)} Fields";
                        var fieldNames = Task.Run(() => RecordService.GetFields(value)).Result;
                    }
                    _recordType = value;
                    ClearNotInIds();
                    if (IncludeNotIn)
                        NotInSwitch();
                    ExplicitlySelectedColumns = null;
                    if (_recordType != null && AllowQuery)
                    {
                        FilterConditions = CreateFilterCondition();
                        Joins = CreateJoins();
                        NotInFilterConditions = CreateFilterCondition(isNotIn: true);
                        NotInJoins = CreateJoins(isNotIn: true);
                    }
                    OnPropertyChanged(nameof(RecordTypeSelected));
                    if (_recordType != null)
                        RecreateGrid();
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
                finally
                {
                    LoadingViewModel.IsLoading = false;
                }
            }
        }

        public bool RecordTypeSelected
        {
            get { return RecordType != null; }
        }

        public bool MultipleRecordTypes
        {
            get
            {
                return RecordTypeItemsSource.Count() > 1;
            }
        }

        public FormServiceBase FormService
        {
            get
            {
                //just hack to get around the project heirachys without having to move all the form code into Record project
                //unsure the IFormService is of the type FormServiceBase
                var formService = RecordService.GetFormService();
                if (formService != null && !(formService is FormServiceBase))
                {
                    throw new NotSupportedException(string.Format("The {0} is An Unexpected Type Of {1}. It Is Required To Be A {2}", typeof(IFormService).Name, formService.GetType().Name, typeof(FormServiceBase).Name));
                }
                return formService as FormServiceBase;
            }
        }

        public RecordType First { get; set; }
    }
}
