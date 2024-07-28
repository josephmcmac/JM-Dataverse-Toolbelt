using JosephM.Application.Application;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using JosephM.Record.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JosephM.Application.ViewModel.Query
{
    public class FilterConditionsViewModel : ViewModelBase
    {
        private readonly QueryViewModel _queryViewModel;

        public FilterConditionsViewModel(string recordType, IRecordService recordService, IApplicationController controller, Action onConditionSelectedChanged, QueryViewModel queryViewModel)
            : base(controller)
        {
            OnConditionSelectedChanged = onConditionSelectedChanged;
            _queryViewModel = queryViewModel;
            RecordType = recordType;
            RecordService = recordService;
            Conditions = new ObservableCollection<ConditionViewModel>();
            AddNewCondition();
            FilterConditions = new ObservableCollection<FilterConditionsViewModel>();
        }

        Action OnConditionSelectedChanged { get; set; }

        private string RecordType { get; set; }

        private IRecordService RecordService { get; set; }

        private void AddNewCondition()
        {
            var queryConditionObject = new ConditionViewModel.QueryCondition(
                () => AddNewCondition(), OnConditionSelectedChanged);
            queryConditionObject.RecordType = new RecordType(RecordType, RecordType);
            var condition = new ConditionViewModel(queryConditionObject, RecordType, RecordService, ApplicationController, _queryViewModel);
            Conditions.Add(condition);
        }

        public ObservableCollection<ConditionViewModel> Conditions { get; set; }

        public IEnumerable<ConditionViewModel> SelectedConditions
        {
            get
            {
                return Conditions.Where(c => c.QueryConditionObject.IsSelected).ToArray();
            }
        }

        public ObservableCollection<FilterConditionsViewModel> FilterConditions { get; set; }

        public FilterOperator FilterOperator { get; set; }

        public Filter GetAsFilter()
        {
            var filter = new Filter();
            filter.ConditionOperator = FilterOperator;
            filter.Conditions = Conditions
                    .Select(c => c.GetAsCondition())
                    .Where(c => c.FieldName != null)
                    .ToList();
            filter.SubFilters = FilterConditions
                    .Select(c => c.GetAsFilter())
                    .ToList();
            return filter;
        }

        public bool Validate()
        {
            var result = true;
            foreach (var condition in Conditions)
            {
                if (!condition.Validate())
                {
                    result = false;
                }
            }
            if (FilterConditions != null)
            {
                foreach (var childFilter in FilterConditions)
                {
                    if (!childFilter.Validate())
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        private void CheckRemoveFilter(FilterConditionsViewModel parentFilterConditions)
        {
            if (Conditions.Count == 1
                && FilterConditions.Count == 0
                    && parentFilterConditions != null)
                parentFilterConditions.FilterConditions.Remove(this);
        }

        public void DeleteSelected(FilterConditionsViewModel parentFilterConditions)
        {
            var isRootFilter = parentFilterConditions == null;
            foreach (var item in Conditions.Where(c => c.QueryConditionObject.IsSelected).ToArray())
            {
                Conditions.Remove(item);
            }
            foreach (var item in FilterConditions.ToArray())
            {
                item.DeleteSelected(this);
            }
            CheckRemoveFilter(parentFilterConditions);
        }

        public void UnGroupSelected(FilterConditionsViewModel parentFilterConditions)
        {
            var isRootFilter = parentFilterConditions == null;
            var selectedConditions = SelectedConditions;
            if (selectedConditions.Count() > 0
                && parentFilterConditions != null)
            {
                foreach (var item in selectedConditions)
                {
                    Conditions.Remove(item);
                    parentFilterConditions.Conditions.Insert(0, item);
                }
            }
            foreach (var item in FilterConditions.ToArray())
            {
                item.UnGroupSelected(this);
            }
            CheckRemoveFilter(parentFilterConditions);
        }

        public void GroupSelected(FilterOperator filterOperator, FilterConditionsViewModel parentFilterConditions = null)
        {
            var isRootFilter = parentFilterConditions == null;
            var selectedConditions = SelectedConditions;
            if (selectedConditions.Count() > 1
                && FilterOperator != filterOperator)
            {
                var newFilterCondition = CreateFilterCondition();
                newFilterCondition.FilterOperator = filterOperator;
                foreach (var item in selectedConditions)
                {
                    Conditions.Remove(item);
                    newFilterCondition.Conditions.Insert(0, item);
                }
                FilterConditions.Add(newFilterCondition);
            }
            foreach (var item in FilterConditions.ToArray())
            {
                item.GroupSelected(filterOperator, this);
            }
        }

        public FilterConditionsViewModel CreateFilterCondition()
        {
            return new FilterConditionsViewModel(RecordType, RecordService, ApplicationController, OnConditionSelectedChanged, _queryViewModel);
        }
    }
}
