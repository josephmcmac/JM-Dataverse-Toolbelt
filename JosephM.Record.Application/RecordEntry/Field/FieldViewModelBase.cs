#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JosephM.Core.Service;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Application.Validation;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public abstract class FieldViewModelBase : ViewModelBase, INotifyDataErrorInfo, IValidatable
    {
        private readonly List<object> _errors = new List<object>();
        private bool _isEditable;
        private bool _isVisible;

        protected FieldViewModelBase(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(recordForm.ApplicationController)
        {
            RecordEntryViewModel = recordForm;
            Label = label;
            FieldName = fieldName;
            IsVisible = true;
            IsRecordServiceField = true;
        }

        public bool IsNotNullable { get; set; }

        private object DeltaValue { get; set; }

        private IRecord Record
        {
            get { return GetRecordForm().GetRecord(); }
        }

        public string GetRecordType()
        {
            return Record.Type;
        }

        private Action<FieldViewModelBase> OnChangeDelegate
        {
            get { return GetRecordForm().GetOnFieldChangeDelegate(); }
        }

        public IRecordService GetRecordService()
        {
            return GetRecordForm().RecordService;
        }

        protected FormServiceBase FormService
        {
            get { return GetRecordForm().FormService; }
        }

        public IEnumerable<ValidationRuleBase> ValidationRules
        {
            get { return RecordEntryViewModel.GetValidationRules(FieldName); }
        }

        internal RecordEntryViewModelBase RecordEntryViewModel { get; set; }

        public RecordEntryViewModelBase GetRecordForm()
        {
            return RecordEntryViewModel;
        }

        public string ReferenceName
        {
            get { return FieldName; }
        }

        public string FieldName { get; private set; }
        public string Label { get; private set; }
        public bool IsRecordServiceField { get; set; }

        public virtual object ValueObject
        {
            get { return Record[FieldName]; }
            set
            {
                if (value == null && ValueObject == null)
                    return;
                if (value != null && value.Equals(ValueObject))
                    return;
                //verifying and parsing of values before setting to the record
                var setValue = false;
                var valueToSet = value;
                var errorMessage = "";
                //verify from the view model
                var verifyValueResponse = VerifyValueRequest(value);
                if (verifyValueResponse.IsValid == false)
                {
                    errorMessage = verifyValueResponse.GetErrorString();
                }
                else if (IsRecordServiceField)
                {
                    //verify and parse from the record service
                    var parseFieldResponse =
                        GetRecordService().ParseFieldRequest(new ParseFieldRequest(FieldName, Record.Type, value));
                    if (parseFieldResponse.Success == false)
                    {
                        errorMessage = parseFieldResponse.Error;
                    }
                    else
                    {
                        valueToSet = parseFieldResponse.ParsedValue;
                        setValue = true;
                    }
                }
                else
                    setValue = true;
                if (setValue)
                {
                    Record.SetField(FieldName, valueToSet, GetRecordService());
                    DeltaValue = Record.GetField(FieldName);
                    OnChangeBase();
                }
                else
                {
                    ValueObject = DeltaValue;
                    ApplicationController.UserMessage(errorMessage);
                }
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
                OnPropertyChanged("IsVisibleAndEditable");
            }
        }

        public bool IsVisibleAndEditable
        {
            get { return IsVisible && IsEditable; }
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;
                OnPropertyChanged("IsEditable");
                OnPropertyChanged("IsReadOnly");
                OnPropertyChanged("IsVisibleAndEditable");
            }
        }

        public bool IsReadOnly
        {
            get { return !IsEditable; }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private IEnumerable<string> ValidationPropertyNames
        {
            //todo this aint right
            get { return new[] { "ValueObject", "Value", "StringDisplay", "EnteredText" }; }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (ValidationPropertyNames.Contains(propertyName))
                return _errors;
            else
                return new string[] {};
        }

        public string GetErrorsString()
        {
            if (_errors.Any())
                return string.Join("\n", _errors);
            else
                return null;
        }

        public bool HasErrors
        {
            get { return _errors.Any(); }
        }

        /// <summary>
        ///     gives scope for the viewmodel types to constrain entered values
        /// </summary>
        protected virtual IsValidResponse VerifyValueRequest(object value)
        {
            return new IsValidResponse();
        }

        public virtual bool Validate()
        {
            _errors.Clear();
            foreach (var validationRule in ValidationRules)
            {
                var validationResponse = validationRule.Validate(this);
                if (validationResponse.IsValid == false)
                {
                    _errors.Add(validationResponse.ErrorContent);
                    foreach (var validationPropertyName in ValidationPropertyNames)
                    {
                        NotifyErrorsChanged(validationPropertyName);
                    }
                    return false;
                }
            }
            return true;
        }

        public void AddError(string error)
        {
            _errors.Add(error);
        }

        public void NotifyErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void OnChangeBase()
        {
            //this should just defer these onloads to the on changes
            RecordEntryViewModel.RefreshVisibility();
            OnPropertyChanged("ValueObject");
            Validate();
            OnChange();
            OnChangeDelegate(this);
        }

        public virtual void OnChange()
        {
        }
    }
}