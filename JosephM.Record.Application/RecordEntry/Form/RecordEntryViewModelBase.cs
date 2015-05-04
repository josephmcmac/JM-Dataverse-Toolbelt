#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Application.TabArea;
using JosephM.Record.Application.Validation;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.RecordEntry.Form
{
    public abstract class RecordEntryViewModelBase : TabAreaViewModelBase
    {
        protected RecordEntryViewModelBase(FormController formController)
            : base(formController.ApplicationController)
        {
            FormController = formController;
        }

        public virtual bool Validate()
        {
            var isValid = true;
            foreach (var recordField in FieldViewModels)
            {
                if (recordField.IsVisible && !recordField.Validate())
                    isValid = false;
            }
            return isValid;
        }

        public virtual bool ShowCancelButton
        {
            get { return true; }
        }

        public virtual bool ShowSaveButton
        {
            get { return true; }
        }

        protected internal FormController FormController { get; set; }

        public abstract IRecord GetRecord();

        public abstract Action<FieldViewModelBase> GetOnFieldChangeDelegate();


        public FormServiceBase FormService
        {
            get { return FormController.FormService; }
        }

        public IRecordService RecordService
        {
            get { return FormController.RecordService; }
        }

        protected internal abstract IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName);

        public void StartNewAction(Action action)
        {
            ApplicationController.DoOnAsyncThread(action);
        }

        public FieldViewModelBase GetFieldViewModel(string fieldName)
        {
            if (FieldViewModels.Any(f => f.FieldName == fieldName))
            {
                return FieldViewModels.First(f => f.FieldName == fieldName);
            }
            throw new ArgumentOutOfRangeException(fieldName, "No Field In Has The Name: " + fieldName);
        }

        public abstract IEnumerable<FieldViewModelBase> FieldViewModels { get; }

        internal void RunOnChanges()
        {
            if (FieldViewModels != null)
            {
                foreach (var field in FieldViewModels)
                {
                    GetOnFieldChangeDelegate()(field);
                }
            }
        }

        internal void RefreshVisibility()
        {
            if (FieldViewModels != null)
            {
                foreach (var field in FieldViewModels)
                {
                    field.IsVisible = FormService.IsFieldInContext(field.FieldName, GetRecord());
                }
            }
            RefreshVisibilityExtention();
        }

        protected virtual void RefreshVisibilityExtention()
        {
        }
    }
}