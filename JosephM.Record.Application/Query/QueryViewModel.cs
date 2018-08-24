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
            RunQueryButton = new XrmButtonViewModel("Run Query", QuickFind, ApplicationController);
            ChangeQueryType();

            QueryTypeButton.IsVisible = AllowQuery;

            _recordTypes = recordTypes;
            if (_recordTypes.Count() == 1)
                RecordType = _recordTypes.First();
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
                    GetGridRecords = GetGridRecords,
                    MultiSelect = true,
                    GridLoaded = false,
                    FieldMetadata = ExplicitlySelectedColumns
                };
                var customFunctionList = new List<CustomGridFunction>()
                {
                    new CustomGridFunction("QUERY", "Run Query", QuickFind),
                    new CustomGridFunction("BACKTOQUERY", "Back To Query", (g) => { ResetToQueryEntry(); }, (g) => !IsQuickFind && QueryRun),
                    new CustomGridFunction("EDITCOLUMNS", "Edit Columns", (g) => LoadColumnEdit(), (g) => DynamicGridViewModel != null)
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
                                DynamicGridViewModel.ReloadGrid();
                            };
                            var record = selectedRow.Record;

                            var newForm = new CreateOrUpdateViewModel(RecordService.Get(record.Type, record.Id), formController, onSave, ClearChildForm);
                            LoadChildForm(newForm);
                        }
                    };
                    DynamicGridViewModel.AddRow = () =>
                    {
                        var formMetadata = FormService.GetFormMetadata(RecordType, RecordService);
                        var formController = new FormController(RecordService, FormService, ApplicationController);
                        Action onSave = () =>
                        {
                            ClearChildForm();
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

                if (resetQueryRun)
                    QueryRun = false;
            }
        }

        private void LoadColumnEdit()
        {
            DoOnMainThread(() =>
            {
                //okay lets spawn a dialog for editing the columns in the grid
                var currentColumns = DynamicGridViewModel.FieldMetadata
                    .OrderBy(f => f.Order)
                    .Select(f => new KeyValuePair<string,double>(f.FieldName, f.WidthPart))
                    .ToArray();

                Action<IEnumerable<ColumnEditDialogViewModel.SelectableColumn>> letsLoadTheColumns = (newColumnSet) =>
                {
                    DoOnMainThread(() =>
                    {
                        ExplicitlySelectedColumns = new List<GridFieldMetadata>();
                        for (var i = 1; i <= newColumnSet.Count(); i++)
                        {
                            var thisOne = newColumnSet.ElementAt(i - 1);
                            ExplicitlySelectedColumns.Add(new GridFieldMetadata(new ViewField(thisOne.FieldName, i, Convert.ToInt32(thisOne.Width))));
                        }
                        ClearChildForm();
                        RecreateGrid();
                        QuickFind();
                    });
                };

                var columnEditDialog = new ColumnEditDialogViewModel(RecordType, currentColumns, RecordService, letsLoadTheColumns, ClearChildForm, ApplicationController);
                LoadChildForm(columnEditDialog);
            });
        }


        private List<GridFieldMetadata> ExplicitlySelectedColumns
        {
            get; set;
        }

        private void ResetToQueryEntry()
        {
            QueryRun = false;
            RecreateGrid();
        }

        private bool AllowQuery { get; set; }

        private FilterConditionsViewModel CreateFilterCondition()
        {
            return new FilterConditionsViewModel(RecordType, RecordService, ApplicationController, () => RefreshConditionButtons());
        }

        private JoinsViewModel CreateJoins()
        {
            return new JoinsViewModel(RecordType, RecordService, ApplicationController, () => RefreshConditionButtons());
        }

        private void GroupSelected(FilterOperator filterOperator)
        {
            FilterConditions?.GroupSelected(filterOperator);
            if (Joins != null && Joins.Joins != null)
            {
                foreach (var join in Joins.Joins)
                {
                    join.GroupSelected(filterOperator);
                }
            }
            RefreshConditionButtons();
        }

        public void RefreshConditionButtons(FilterConditionsViewModel filter = null, bool isRootFilter = true, bool processJoins = true, JoinsViewModel joins = null)
        {
            joins = joins ?? Joins;
            if (filter == null)
            {
                DeleteSelectedConditionsButton.IsVisible = false;
                GroupSelectedConditionsAnd.IsVisible = false;
                GroupSelectedConditionsOr.IsVisible = false;
                UngroupSelectedConditions.IsVisible = false;
                filter = FilterConditions;
                if (!AllowQuery)
                    return;
            }
            if (filter != null)
            {
                var selectedCount = filter.SelectedConditions.Count();
                if (selectedCount > 0)
                    DeleteSelectedConditionsButton.IsVisible = true;
                if (selectedCount > 0 && !isRootFilter)
                    UngroupSelectedConditions.IsVisible = true;
                if (selectedCount > 1 && filter.FilterOperator == FilterOperator.And)
                    GroupSelectedConditionsOr.IsVisible = true;
                if (selectedCount > 1 && filter.FilterOperator == FilterOperator.Or)
                    GroupSelectedConditionsAnd.IsVisible = true;
                foreach (var item in filter.FilterConditions)
                {
                    RefreshConditionButtons(item, isRootFilter: false, processJoins: false);
                }
                if (processJoins && joins != null && joins.Joins != null)
                {
                    foreach (var join in joins.Joins)
                    {
                        if (join.FilterConditions != null)
                        {
                            RefreshConditionButtons(join.FilterConditions, isRootFilter: true, processJoins: join.Joins != null, joins: join.Joins);
                        }
                    }
                }
            }
        }


        private void UnGroupSelected(FilterConditionsViewModel filterConditions = null, FilterConditionsViewModel parentFilterConditions = null)
        {
            FilterConditions?.UnGroupSelected(null);
            if (Joins != null && Joins.Joins != null)
            {
                foreach (var join in Joins.Joins)
                {
                    join.UngroupSelectedConditions();
                }
            }
            RefreshConditionButtons();
        }

        private static void CheckRemoveFilter(FilterConditionsViewModel filterConditions, FilterConditionsViewModel parentFilterConditions)
        {
            if (filterConditions.Conditions.Count == 1
                && filterConditions.FilterConditions.Count == 0
                    && parentFilterConditions != null)
                parentFilterConditions.FilterConditions.Remove(filterConditions);
        }

        private void DeleteSelected()
        {
            FilterConditions?.DeleteSelected(null);
            if(Joins != null && Joins.Joins != null)
            {
                foreach(var join in Joins.Joins)
                {
                    join.DeleteSelectedConditions();
                }
            }
            RefreshConditionButtons();
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
                    result = false;
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
            get { return DynamicGridViewModel.GridRecords; }
        }

        public GetGridRecordsResponse GetGridRecords(bool ignorePages)
        {
            var isValid = ValidateCurrentSearch();
            if (!isValid)
                return new GetGridRecordsResponse(new IRecord[0]);
            var query = GenerateQuery();

            if (!DynamicGridViewModel.HasPaging || ignorePages)
            {
                var records = DynamicGridViewModel.RecordService.RetreiveAll(query);
                return new GetGridRecordsResponse(records);
            }
            else
            {
                return DynamicGridViewModel.GetGridRecordPage(query);
            }
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
                    query.RootFilter.ConditionOperator = FilterOperator.Or;
                    query.RootFilter.Conditions.AddRange(quickFindFields.Select(f => new Condition(f, ConditionType.BeginsWith, QuickFindText)));
                }
            }
            else
            {
                query.RootFilter = FilterConditions.GetAsFilter();
                query.Joins = Joins.GetAsJoins().ToList();
            }
            var view = DynamicGridViewModel.RecordService.GetView(DynamicGridViewModel.RecordType, DynamicGridViewModel.ViewType);
            query.Sorts = view.Sorts.ToList();
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

        public XrmButtonViewModel DeleteSelectedConditionsButton { get; set; }

        public XrmButtonViewModel GroupSelectedConditionsOr { get; set; }

        public XrmButtonViewModel GroupSelectedConditionsAnd { get; set; }

        public XrmButtonViewModel UngroupSelectedConditions { get; set; }
        public XrmButtonViewModel RunQueryButton { get; private set; }

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
                    ExplicitlySelectedColumns = null;
                    if (_recordType != null && AllowQuery)
                    {
                        FilterConditions = CreateFilterCondition();
                        Joins = CreateJoins();
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
