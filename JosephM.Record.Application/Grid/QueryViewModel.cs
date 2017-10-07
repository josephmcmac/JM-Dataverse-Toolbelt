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

namespace JosephM.Application.ViewModel.Grid
{
    public class QueryViewModel : TabAreaViewModelBase, INotifyPropertyChanged
    {
        public QueryViewModel(IEnumerable<string> recordTypes, IRecordService recordService, IApplicationController controller, bool allowQuery = false, bool loadInitially = false, CustomGridFunction closeFunction = null
            , CustomGridFunction processSelectedFunction = null)
            : base(controller)
        {
            AllowQuery = allowQuery;
            RecordTypes = recordTypes;
            RecordService = recordService;
            if (closeFunction != null)
                ReturnButton = new XrmButtonViewModel(closeFunction.Label, () => { closeFunction.Function(DynamicGridViewModel); }, controller);
            QueryTypeButton = new XrmButtonViewModel("Change Query Type", ChangeQueryType, ApplicationController);
            DeleteSelectedConditionsButton = new XrmButtonViewModel("Delete Selected", () => DeleteSelected(), ApplicationController);
            GroupSelectedConditionsOr = new XrmButtonViewModel("Group Selected Or", () => GroupSelected(FilterOperator.Or), ApplicationController);
            GroupSelectedConditionsAnd = new XrmButtonViewModel("Group Selected And", () => GroupSelected(FilterOperator.And), ApplicationController);
            UngroupSelectedConditions = new XrmButtonViewModel("Ungroup Selected", () => UnGroupSelected(), ApplicationController);
            ChangeQueryType();

            if (AllowQuery)
                FilterConditions = CreateFilterCondition();
            QueryTypeButton.IsVisible = AllowQuery;

            DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
            {
                PageSize = StandardPageSize,
                ViewType = ViewType.MainApplicationView,
                RecordService = recordService,
                RecordType = recordTypes.First(),
                IsReadOnly = true,
                FormController = new FormController(recordService, null, controller),
                GetGridRecords = GetGridRecords,
                MultiSelect = true,
                GridLoaded = false
            };
            var customFunctions = new List<CustomGridFunction>()
            {
                new CustomGridFunction("Run Query", QuickFind)
            };
            if (processSelectedFunction != null)
                customFunctions.Add(processSelectedFunction);

            DynamicGridViewModel.LoadGridButtons(customFunctions);
            if(loadInitially)
                DynamicGridViewModel.ReloadGrid();

            RefreshConditionButtons();
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
            var selectedCount = filter.SelectedConditions.Count();
            if (selectedCount > 0)
                DeleteSelectedConditionsButton.IsVisible = true;
            if (selectedCount > 0 && !isRootFilter)
                UngroupSelectedConditions.IsVisible = true;
            if (selectedCount > 1 && filter.FilterOperator == FilterOperator.And)
                GroupSelectedConditionsOr.IsVisible = true;
            if (selectedCount > 1 && filter.FilterOperator == FilterOperator.Or)
                GroupSelectedConditionsAnd.IsVisible = true;
            foreach(var item in filter.FilterConditions)
            {
                RefreshConditionButtons(item);
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

        public DynamicGridViewModel DynamicGridViewModel { get; private set; }

        private ObservableCollection<ViewModelBase> _childForms = new ObservableCollection<ViewModelBase>();

        public ObservableCollection<GridRowViewModel> GridRecords
        {
            get { return DynamicGridViewModel.GridRecords; }
        }

        public GetGridRecordsResponse GetGridRecords(bool ignorePages)
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


            if (!DynamicGridViewModel.HasPaging)
            {
                var records = DynamicGridViewModel.RecordService.RetreiveAll(query);
                return new GetGridRecordsResponse(records);
            }
            else
            {
                return DynamicGridViewModel.GetGridRecordPage(query);
            }
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

        public FilterConditionsViewModel FilterConditions { get; set; }

        public XrmButtonViewModel DeleteSelectedConditionsButton { get; set; }

        public XrmButtonViewModel GroupSelectedConditionsOr { get; set; }

        public XrmButtonViewModel GroupSelectedConditionsAnd { get; set; }

        public XrmButtonViewModel UngroupSelectedConditions { get; set; }

        public IEnumerable<string> RecordTypes
        {
            get; set;
        }

        public string RecordType
        {
            get
            {
                return RecordTypes.First(); ;
            }
        }
    }
}
