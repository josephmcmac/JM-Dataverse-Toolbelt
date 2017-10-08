using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Application.Application;
using System.Collections.ObjectModel;
using JosephM.Record.IService;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Core.FieldType;
using JosephM.Record.Query;

namespace JosephM.Application.ViewModel.Query
{
    public class FilterConditionsViewModel : ViewModelBase
    {
        public FilterConditionsViewModel(string recordType, IRecordService recordService, IApplicationController controller, Action onConditionSelectedChanged)
            : base(controller)
        {
            OnConditionSelectedChanged = onConditionSelectedChanged;
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
            var condition = new ConditionViewModel(queryConditionObject, RecordType, RecordService, ApplicationController);
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
    }
}
