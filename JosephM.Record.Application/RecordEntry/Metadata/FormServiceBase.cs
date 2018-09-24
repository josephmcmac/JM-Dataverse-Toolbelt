#region

using System;
using System.Collections.Generic;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Validation;
using JosephM.Record.IService;
using JosephM.Record.Query;
using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Record.Extentions;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public abstract class FormServiceBase : IFormService
    {
        public FormServiceBase()
        {
            AllowLookupFunctions = true;
        }

        public virtual bool AllowGridFieldEditEdit(string fieldName)
        {
            return true;
        }

        public abstract FormMetadata GetFormMetadata(string recordType, IRecordService recordService = null);

        public virtual bool IsFieldInContext(string fieldName, IRecord record)
        {
            return true;
        }

        public virtual bool IsSectionInContext(string sectionIdentifier, IRecord record)
        {
            return true;
        }

        public bool AllowLookupFunctions { get; set; }

        public virtual IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName, string recordType)
        {
            return new ValidationRuleBase[] {};
        }

        public virtual IEnumerable<ValidationRuleBase> GetSubgridValidationRules(string fieldName, string subGrid)
        {
            return new ValidationRuleBase[] {};
        }

        public virtual IEnumerable<ValidationRuleBase> GetSectionValidationRules(string sectionIdentifier)
        {
            return new ValidationRuleBase[] { };
        }

        internal virtual IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName, RecordEntryViewModelBase entryViewModel)
        {
            return new Action<RecordEntryViewModelBase>[] {};
        }

        internal virtual IEnumerable<ReferenceFieldViewModel<T>.ReferencePicklistItem> OrderPicklistItems<T>(string fieldName, string recordType, IEnumerable<ReferenceFieldViewModel<T>.ReferencePicklistItem> picklistItems)
        {
            return picklistItems.OrderBy(p => { return p.Record == null ? 0 : 1; }).ThenBy(p => p.Name);
        }

        internal virtual bool InitialisePicklistIfOneOption(string fieldName, string recordType)
        {
            return false;
        }

        internal virtual IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName, string subGrid, RecordEntryViewModelBase entryViewModel)
        {
            return new Action<RecordEntryViewModelBase>[] {};
        }

        internal virtual RecordEntryFormViewModel GetLoadRowViewModel(string subGridName, RecordEntryViewModelBase parentForm, Action<IRecord> onSave, Action onCancel)
        {
            return null;
        }

        public virtual RecordEntryFormViewModel GetEditRowViewModel(string subGridName, RecordEntryViewModelBase parentForm, Action<IRecord> onSave, Action onCancel, GridRowViewModel gridRow)
        {
            return null;
        }

        public virtual RecordEntryFormViewModel GetEditEnumerableViewModel(string subGridName, string fieldName, RecordEntryViewModelBase parentForm, Action<IRecord> onSave, Action onCancel, GridRowViewModel gridRow)
        {
            return null;
        }

        internal virtual string GetDependantValue(string field, string recordType, RecordEntryViewModelBase viewModel)
        {
            return null;
        }

        internal virtual IEnumerable<Action<RecordEntryViewModelBase>> GetOnLoadTriggers(string fieldName, string recordType)
        {
            return new Action<RecordEntryViewModelBase>[0];
        }

        internal virtual string GetLookupTargetType(string field, string recordType, RecordEntryViewModelBase recordForm)
        {
            return recordForm.RecordService.GetLookupTargetType(field, recordType);
        }

        public virtual IEnumerable<Condition> GetLookupConditions(string fieldName, string recordType, string reference, IRecord record)
        {
            return new Condition[0];
        }

        internal virtual string GetPicklistDisplayField(string fieldName, string recordType, IRecordService lookupService, string recordTypeToLookup)
        {
            var targetType = lookupService.GetLookupTargetType(fieldName, recordType);
            return lookupService.GetPrimaryField(targetType);
        }

        internal virtual IEnumerable<CustomFormFunction> GetCustomFunctions(string recordType, RecordEntryFormViewModel recordForm)
        {
            return new CustomFormFunction[0];
        }

        public virtual IEnumerable<CustomGridFunction> GetCustomFunctionsFor(string referenceName, RecordEntryFormViewModel recordForm)
        {
            return new CustomGridFunction[0];
        }

        public virtual bool AllowAddNew(string fieldName, string recordType)
        {
            return true;
        }

        public virtual bool AllowDelete(string fieldName, string recordType)
        {
            return true;
        }

        public virtual bool AllowGridOpen(string fieldName, RecordEntryViewModelBase recordForm)
        {
            return true;
        }

        public virtual bool AllowNestedGridEdit(string subGridName, string fieldName)
        {
            return false;
        }

        public virtual bool UsePicklist(string fieldName, string recordType)
        {
            return false;
        }

        public virtual Action GetBulkAddFunctionFor(string referenceName, RecordEntryViewModelBase recordForm)
        {
            return null;
        }

        private readonly object _lockObject = new Object();
        private readonly IDictionary<string, CachedPicklist> _cachedPicklist = new Dictionary<string, CachedPicklist>();

        private class CachedPicklist
        {
            private IEnumerable<Condition> Conditions { get; set; }
            public IRecordService LookupService { get; set; }
            public IEnumerable<IRecord> Picklist { get; set; }


            public CachedPicklist(IEnumerable<IRecord> picklist, IEnumerable<Condition> conditions,
                IRecordService lookupService)
            {
                {
                    Picklist = picklist;
                    Conditions = conditions;
                    LookupService = lookupService;
                }
            }
        }
        internal IEnumerable<IRecord> GetLookupPicklist(string fieldName, string recordType, string reference, IRecord record, IRecordService lookupService, string recordTypeToLookup)
        {
            var conditions = GetLookupConditions(fieldName, recordType, reference, record);
            lock (_lockObject)
            {
                if (!_cachedPicklist.ContainsKey(fieldName) || _cachedPicklist[fieldName].LookupService != lookupService)
                {
                    var displayField = GetPicklistDisplayField(fieldName, recordType, lookupService, recordTypeToLookup);

                    var picklist = lookupService.RetrieveAllAndClauses(recordTypeToLookup, conditions,
                        new[] { displayField });
                    var cache = new CachedPicklist(picklist, conditions, lookupService);
                    if (_cachedPicklist.ContainsKey(fieldName))
                        _cachedPicklist[fieldName] = cache;
                    else
                        _cachedPicklist.Add(fieldName, cache);
                }
                return _cachedPicklist[fieldName].Picklist;
            }
        }
    }
}