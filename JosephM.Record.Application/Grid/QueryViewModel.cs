using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Application.ViewModel.Shared;
using JosephM.Record.Query;
using JosephM.Record.Extentions;
using JosephM.Application.ViewModel.Query;
using JosephM.Core.FieldType;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.RecordEntry.Form;
using System.Threading;

namespace JosephM.Application.ViewModel.Grid
{
    public class QueryViewModel : TabAreaViewModelBase, INotifyPropertyChanged
    {
        public QueryViewModel(IEnumerable<string> recordTypes, IRecordService recordService, IApplicationController controller, bool allowQuery = false, bool loadInitially = false, CustomGridFunction closeFunction = null
            , IEnumerable<CustomGridFunction> customFunctions = null)
            : base(controller)
        {
            CustomFunctions = customFunctions;
            LoadInitially = loadInitially;
            AllowQuery = allowQuery;
            RecordService = recordService;
            if (closeFunction != null)
                ReturnButton = new XrmButtonViewModel(closeFunction.Label, () => { closeFunction.Function(DynamicGridViewModel); }, controller);
            QueryTypeButton = new XrmButtonViewModel("Change Query Type", ChangeQueryType, ApplicationController);
            DeleteSelectedConditionsButton = new XrmButtonViewModel("Delete Selected", () => DeleteSelected(), ApplicationController);
            GroupSelectedConditionsOr = new XrmButtonViewModel("Group Selected Or", () => GroupSelected(FilterOperator.Or), ApplicationController);
            GroupSelectedConditionsAnd = new XrmButtonViewModel("Group Selected And", () => GroupSelected(FilterOperator.And), ApplicationController);
            UngroupSelectedConditions = new XrmButtonViewModel("Ungroup Selected", () => UnGroupSelected(), ApplicationController);
            ChangeQueryType();

            QueryTypeButton.IsVisible = AllowQuery;

            _recordTypes = recordTypes;
            if (_recordTypes.Count() == 1)
                RecordType = _recordTypes.First();
        }

        private IEnumerable<CustomGridFunction> CustomFunctions { get; set; }
        private bool LoadInitially { get; set; }

        public bool TypeAhead { get; set; }

        public void RecreateGrid()
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
                    GridLoaded = false
                };
                var customFunctionList = new List<CustomGridFunction>()
                {
                    new CustomGridFunction("QUERY", "Run Query", QuickFind)
                };
                if (FormService != null)
                {
                    DynamicGridViewModel.EditRow = (g) =>
                    {
                        var formMetadata = FormService.GetFormMetadata(RecordType, RecordService);
                        var formController = new FormController(this.RecordService, FormService, ApplicationController);
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
            }
        }

        private bool AllowQuery { get; set; }

        private FilterConditionsViewModel CreateFilterCondition()
        {
            return new FilterConditionsViewModel(RecordType, RecordService, ApplicationController, () => RefreshConditionButtons());
        }

        private void GroupSelected(FilterOperator filterOperator, FilterConditionsViewModel filterConditions = null, FilterConditionsViewModel parentFilterConditions = null)
        {
            var isRootFilter = filterConditions == null;
            if (filterConditions == null)
                filterConditions = FilterConditions;
            var selectedConditions = filterConditions.SelectedConditions;
            if(selectedConditions.Count() > 1 
                && filterConditions.FilterOperator != filterOperator)
            {
                var newFilterCondition = CreateFilterCondition();
                newFilterCondition.FilterOperator = filterOperator;
                foreach (var item in selectedConditions)
                {
                    filterConditions.Conditions.Remove(item);
                    newFilterCondition.Conditions.Insert(0, item);
                }
                filterConditions.FilterConditions.Add(newFilterCondition);
            }
            foreach (var item in filterConditions.FilterConditions.ToArray())
            {
                GroupSelected(filterOperator, item, filterConditions);
            }
            if (isRootFilter)
                RefreshConditionButtons();
        }

        public void RefreshConditionButtons(FilterConditionsViewModel filter = null)
        {
            var isRootFilter = filter == null;
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
                    RefreshConditionButtons(item);
                }
            }
        }


        private void UnGroupSelected(FilterConditionsViewModel filterConditions = null, FilterConditionsViewModel parentFilterConditions = null)
        {
            var isRootFilter = filterConditions == null;
            if (filterConditions == null)
                filterConditions = FilterConditions;
            var selectedConditions = filterConditions.SelectedConditions;
            if (selectedConditions.Count() > 0
                && parentFilterConditions != null)
            {
                foreach (var item in selectedConditions)
                {
                    filterConditions.Conditions.Remove(item);
                    parentFilterConditions.Conditions.Insert(0, item);
                }
            }
            foreach (var item in filterConditions.FilterConditions.ToArray())
            {
                UnGroupSelected(item, filterConditions);
            }
            CheckRemoveFilter(filterConditions, parentFilterConditions);
            if (isRootFilter)
                RefreshConditionButtons();
        }

        private static void CheckRemoveFilter(FilterConditionsViewModel filterConditions, FilterConditionsViewModel parentFilterConditions)
        {
            if (filterConditions.Conditions.Count == 1
                && filterConditions.FilterConditions.Count == 0
                    && parentFilterConditions != null)
                parentFilterConditions.FilterConditions.Remove(filterConditions);
        }

        private void DeleteSelected(FilterConditionsViewModel filterConditions = null, FilterConditionsViewModel parentFilterConditions = null)
        {
            var isRootFilter = filterConditions == null;
            if (filterConditions == null)
                filterConditions = FilterConditions;
            var selectedConditions = filterConditions.SelectedConditions;
            foreach (var item in filterConditions.Conditions.Where(c => c.QueryConditionObject.IsSelected).ToArray())
            {
                filterConditions.Conditions.Remove(item);
            }
            foreach (var item in filterConditions.FilterConditions.ToArray())
            {
                DeleteSelected(item, filterConditions);
            }
            CheckRemoveFilter(filterConditions, parentFilterConditions);
            if(isRootFilter)
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
            }
        }

        private void ChangeQueryType()
        {
            IsQuickFind = !IsQuickFind;
            QueryTypeButton.Label = IsQuickFind ? "Use Query" : "Use Quick Find";
            if (!IsQuickFind)
                CreateFilterCondition();
        }

        public void QuickFind()
        {
            if (!IsQuickFind)
            {
                var anyNotValid = false;
                foreach (var condition in FilterConditions.Conditions)
                {
                    if (!condition.Validate())
                    {
                        anyNotValid = true;
                    }
                }
                if (anyNotValid)
                    return;
            }
            DynamicGridViewModel.ReloadGrid();
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
            if (IsQuickFind)
            {
                query.IsQuickFind = true;
                query.QuickFindText = QuickFindText;
                if (!string.IsNullOrWhiteSpace(QuickFindText))
                {
                    query.RootFilter.Conditions.Add(new Condition(RecordService.GetPrimaryField(DynamicGridViewModel.RecordType), ConditionType.BeginsWith, QuickFindText));
                }
            }
            else
            {
                query.RootFilter = FilterConditions.GetAsFilter();
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

        public XrmButtonViewModel DeleteSelectedConditionsButton { get; set; }

        public XrmButtonViewModel GroupSelectedConditionsOr { get; set; }

        public XrmButtonViewModel GroupSelectedConditionsAnd { get; set; }

        public XrmButtonViewModel UngroupSelectedConditions { get; set; }

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
                _recordType = value;
                if (_recordType != null && AllowQuery)
                    FilterConditions = CreateFilterCondition();
                OnPropertyChanged(nameof(RecordTypeSelected));
                if (_recordType != null)
                    RecreateGrid();
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
