using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.RecordEntry.Section;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.Query
{
    public class ConditionViewModel : RecordEntryViewModelBase
    {
        public ConditionViewModel(QueryCondition conditionObject, string recordType, IRecordService recordService, IApplicationController controller)
            : base(FormController.CreateForObject(conditionObject, controller, recordService, optionSetLimitedvalues: new Dictionary<string, IEnumerable<string>> { { nameof(QueryCondition.FieldName), GetValidFields(recordType, recordService) } }))
        {
            _queryCondition = conditionObject;
            _queryConditionRecord = new ObjectRecord(conditionObject);

            var metadata = FormService.GetFormMetadata(GetRecord().Type, RecordService);
            var sections = metadata.FormSections;
            var firstSection = sections.First();
            var sectionViewModel = new FieldSectionViewModel(
                        (FormFieldSection)firstSection,
                        this
                        );
            FormFieldSection = sectionViewModel;
            OnLoad();
        }

        private static IEnumerable<string> GetValidFields(string recordType, IRecordService lookupService)
        {
            var invalidFieldTypes = new[] { RecordFieldType.ActivityParty, RecordFieldType.Unknown, RecordFieldType.Virtual, RecordFieldType.Image };
            return lookupService
                .GetFieldMetadata(recordType)
                //.Where(f => f.Searchable)
                .Where(f => !invalidFieldTypes.Contains(f.FieldType))
                .Select(f => f.SchemaName)
                .ToArray();
        }

        public override IEnumerable<FieldSectionViewModel> FieldSections
        {
            get { return new[] { FormFieldSection }; }
        }

        public override Action<FieldViewModelBase> GetOnFieldChangeDelegate()
        {
            return f =>
            {
                foreach (var action in FormService.GetOnChanges(f.FieldName, this))
                {
                    try
                    {
                        action(this);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
            };
        }

        protected internal override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
        {
            return FormService.GetValidationRules(fieldName, GetRecordType());
        }

        public override RecordEntryViewModelBase ParentForm
        {
            get { return null; }
        }

        internal override string ParentFormReference
        {
            get { return null; }
        }

        internal override void RefreshEditabilityExtention()
        {
            base.RefreshEditabilityExtention();
        }


        private QueryCondition _queryCondition;
        public QueryCondition QueryConditionObject
        {
            get { return _queryCondition; }
        }

        private IRecord _queryConditionRecord;
        public override IRecord GetRecord()
        {
            return _queryConditionRecord;
        }
        
        public FieldSectionViewModel FormFieldSection { get; set; }

        public override IEnumerable<FieldViewModelBase> FieldViewModels
        {
            get { return FormFieldSection.Fields; }
        }

        public Condition GetAsCondition()
        {
            object value = null;
            if (_queryCondition.IsInContext(nameof(QueryCondition.Value)))
                value = _queryCondition.Value;
            if (_queryCondition.IsInContext(nameof(QueryCondition.ValueInt)))
                value = _queryCondition.ValueInt;
            var condition = new Condition(_queryCondition.FieldName?.Key, _queryCondition.ConditionType.HasValue ? _queryCondition.ConditionType.Value : ConditionType.Equal, value);
            return condition;
        }

        public override bool AllowNewLookup { get { return false; } }

        [Group(Sections.Main, Group.DisplayLayoutEnum.HorizontalInputOnly)]
        public class QueryCondition
        {
            public QueryCondition(Action onFieldSelected, Action onselectedChanged)
            {
                OnFieldSelected = onFieldSelected;
                OnSelectedChanged = onselectedChanged;
            }

            [Group(Sections.Main)] //removing this causes it not to initialise
            [Hidden]
            [RecordTypeFor(nameof(FieldName))]
            [DisplayOrder(100)]
            public RecordType RecordType { get; set; }

            private bool _isSelected;

            [DisplayOrder(5)]
            [Group(Sections.Main)]
            [PropertyInContextByPropertyNotNull(nameof(FieldName))]
            public bool IsSelected
            {
                get
                {
                    return _isSelected;
                }
                set
                {
                    _isSelected = value;
                    OnSelectedChanged();
                }
            }

            private RecordField _fieldName;

            //don't set this required as empty one always appended at end of query
            [RecordFieldFor(nameof(ConditionType))]
            [RecordFieldFor(nameof(Value))]
            [DisplayOrder(10)]
            [Group(Sections.Main)]
            public RecordField FieldName
            {
                get
                {
                    return _fieldName;
                }
                set
                {
                    var triggerOnFieldSelected = _fieldName == null && value != null && OnFieldSelected != null;
                    _fieldName = value;
                    if (triggerOnFieldSelected)
                        OnFieldSelected();
                }
            }

            [RequiredProperty]
            [DisplayOrder(20)]
            [Group(Sections.Main)]
            [PropertyInContextByPropertyNotNull(nameof(FieldName))]
            public ConditionType? ConditionType { get; set; }

            [RequiredProperty]
            [DisplayOrder(30)]
            [Group(Sections.Main)]
            [PropertyInContextByPropertyValues(nameof(ConditionType), new object[] {
                Record.Query.ConditionType.BeginsWith,
                Record.Query.ConditionType.Equal,
                Record.Query.ConditionType.NotEqual,
                Record.Query.ConditionType.Like,
                Record.Query.ConditionType.LessEqual,
                Record.Query.ConditionType.LessThan,
                Record.Query.ConditionType.GreaterEqual,
                Record.Query.ConditionType.GreaterThan,
                Record.Query.ConditionType.In,
                Record.Query.ConditionType.EndsWith,
                Record.Query.ConditionType.On,
                Record.Query.ConditionType.OnOrBefore,
                Record.Query.ConditionType.OnOrAfter,
                Record.Query.ConditionType.NotOn,})]
            public object Value { get; set; }

            [MinimumIntValue(1)]
            [RequiredProperty]
            [DisplayOrder(30)]
            [Group(Sections.Main)]
            [PropertyInContextByPropertyValues(nameof(ConditionType), new object[] {
                Record.Query.ConditionType.LastXHours,
                Record.Query.ConditionType.NextXHours,
                Record.Query.ConditionType.LastXDays,
                Record.Query.ConditionType.NextXDays,
                Record.Query.ConditionType.LastXWeeks,
                Record.Query.ConditionType.NextXWeeks,
                Record.Query.ConditionType.LastXMonths,
                Record.Query.ConditionType.NextXMonths,
                Record.Query.ConditionType.LastXYears,
                Record.Query.ConditionType.NextXYears,
                Record.Query.ConditionType.LastXFiscalYears,
                Record.Query.ConditionType.NextXFiscalYears,
                Record.Query.ConditionType.LastXFiscalPeriods,
                Record.Query.ConditionType.NextXFiscalPeriods,
                Record.Query.ConditionType.OlderThanXMinutes,
                Record.Query.ConditionType.OlderThanXHours,
                Record.Query.ConditionType.OlderThanXDays,
                Record.Query.ConditionType.OlderThanXWeeks,
                Record.Query.ConditionType.OlderThanXMonths,
                Record.Query.ConditionType.OlderThanXYears,})]
            public int? ValueInt { get; set; }

            private Action OnFieldSelected { get; set; }
            private Action OnSelectedChanged { get; set; }

            private static class Sections
            {
                public const string Main = "Main";
            }
        }
    }
}
