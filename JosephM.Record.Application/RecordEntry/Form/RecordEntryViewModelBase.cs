#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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

        public string GetRecordType()
        {
            var record = GetRecord();
            return record == null ? null : record.Type;
        }

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
            throw new ArgumentOutOfRangeException(fieldName, string.Format("No Field In {0} Object Has The Name {1}", GetRecord().Type, fieldName));
        }

        public T GetFieldViewModel<T>(string fieldName)
            where T : FieldViewModelBase
        {
            var viewModel = GetFieldViewModel(fieldName);
            var typedViewModel = viewModel as T;
            if (typedViewModel == null)
            {
                var type = viewModel == null ? "null" : viewModel.GetType().Name;
                throw new Exception(
                    string.Format("Expected Field Of Type {0} For {1}. Actual Type Is {2}", typeof (T).Name, fieldName,
                        type));
            }
            return typedViewModel;
        }

        public RecordTypeFieldViewModel GetRecordTypeFieldViewModel(string fieldName)
        {
            return GetFieldViewModel<RecordTypeFieldViewModel>(fieldName);
        }

        public RecordFieldFieldViewModel GetRecordFieldFieldViewModel(string fieldName)
        {
            return GetFieldViewModel<RecordFieldFieldViewModel>(fieldName);
        }

        public ObjectFieldViewModel GetObjectFieldFieldViewModel(string fieldName)
        {
            return GetFieldViewModel<ObjectFieldViewModel>(fieldName);
        }

        public LookupFieldViewModel GetLookupFieldFieldViewModel(string fieldName)
        {
            return GetFieldViewModel<LookupFieldViewModel>(fieldName);
        }

        public PicklistFieldViewModel GetPicklistFieldFieldViewModel(string fieldName)
        {
            return GetFieldViewModel<PicklistFieldViewModel>(fieldName);
        }

        public BooleanFieldViewModel GetBooleanFieldFieldViewModel(string fieldName)
        {
            return GetFieldViewModel<BooleanFieldViewModel>(fieldName);
        }

        public IntegerFieldViewModel GetIntegerFieldFieldViewModel(string fieldName)
        {
            return GetFieldViewModel<IntegerFieldViewModel>(fieldName);
        }

        public StringFieldViewModel GetStringFieldFieldViewModel(string fieldName)
        {
            return GetFieldViewModel<StringFieldViewModel>(fieldName);
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

        internal void OnLoad()
        {
            RefreshVisibility();
            RefreshEditabilityExtention();
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

        internal virtual void RefreshEditabilityExtention()
        {
        }

        internal abstract RecordEntryViewModelBase ParentForm { get; }

        internal abstract string ParentFormReference { get; }
    }
}