#region

using System;
using System.Collections.Generic;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Validation;
using JosephM.Record.IService;
using JosephM.Record.Query;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public abstract class FormServiceBase : IFormService
    {
        public abstract FormMetadata GetFormMetadata(string recordType, IRecordService recordService = null); 

        public virtual bool IsFieldInContext(string fieldName, IRecord record)
        {
            return true;
        }

        public virtual bool IsSectionInContext(string sectionIdentifier, IRecord record)
        {
            return true;
        }

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

        internal virtual IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName)
        {
            return new Action<RecordEntryViewModelBase>[] {};
        }

        internal virtual IEnumerable<Action<RecordEntryViewModelBase>> GetOnChanges(string fieldName, string subGrid)
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

        internal virtual IEnumerable<IRecord> GetLookupPicklist(string fieldName, string recordType, string reference, IRecord record, IRecordService lookupService, string recordTypeToLookup)
        {
            throw new NotImplementedException();
        }

        internal virtual string GetPicklistDisplayField(string fieldName, string recordType, IRecordService lookupService, string recordTypeToLookup)
        {
            throw new NotImplementedException();
        }

        internal virtual IEnumerable<CustomGridFunction> GetCustomFunctionsFor(string referenceName, RecordEntryViewModelBase recordForm)
        {
            return new CustomGridFunction[0];
        }

        internal virtual bool AllowAddRow(string subGridName)
        {
            return true;
        }

        public virtual bool UsePicklist(string fieldName, string recordType)
        {
            return false;
        }

        public virtual IEnumerable<GridFieldMetadata> GetGridMetadata(string recordType)
        {
            throw new NotImplementedException();
        }

        public virtual Action GetBulkAddFunctionFor(string referenceName, RecordEntryViewModelBase recordForm)
        {
            return null;
        }
    }
}