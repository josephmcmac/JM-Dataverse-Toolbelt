#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public abstract class FieldViewModelBase : ViewModelBase, INotifyDataErrorInfo, IValidatable
    {
        private readonly List<object> _errors = new List<object>();
        private bool _isEditable;
        private bool _isVisible;

        protected FieldViewModelBase(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(recordForm.ApplicationController)
        {
            LoadingViewModel = new LoadingViewModel(recordForm.ApplicationController);
            LoadingViewModel.LoadingMessage = null;
            RecordEntryViewModel = recordForm;
            Label = label;
            FieldName = fieldName;
            IsVisible = true;
            IsRecordServiceField = true;
            DisplayLabel = true;
        }


        public HorizontalJustify HorizontalJustify
        {
            get
            {
                return RecordEntryViewModel.GetHorizontalJustify(GetRecordService().GetFieldType(FieldName, GetRecordType()));
            }
        }

        public bool IsNotNullable { get; set; }

        private object DeltaValue { get; set; }

        protected IRecord Record
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

        public RecordEntryViewModelBase RecordEntryViewModel { get; set; }

        public RecordEntryViewModelBase GetRecordForm()
        {
            return RecordEntryViewModel;
        }

        public string ReferenceName
        {
            get { return FieldName; }
        }

        public string FieldName { get; private set; }

        private string _label;
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
                OnPropertyChanged("Label");
            }
        }

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

        public virtual bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
                OnPropertyChanged(nameof(IsVisibleAndEditable));
                OnPropertyChanged(nameof(IsVisibleAndReadonly));
            }
        }

        public bool IsVisibleAndEditable
        {
            get { return IsVisible && IsEditable; }
        }

        public bool IsVisibleAndReadonly
        {
            get { return IsVisible && IsReadOnly; }
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;
                OnPropertyChanged(nameof(IsEditable));
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(IsVisibleAndEditable));
                OnPropertyChanged(nameof(IsVisibleAndReadonly));
            }
        }

        public bool IsReadOnly
        {
            get { return !IsEditable; }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        internal IEnumerable<string> ValidationPropertyNames
        {
            //could be in extended classes - is used to notify the ui of validation error
            //as sometimes it is a different property displayed in ui for different view model types
            get { return new[] { "ValueObject", "Value", "StringDisplay", "EnteredText", "SelectedItem", "ValueString" }; }
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

        private bool lastValidationResult = true;

        public virtual bool Validate()
        {
            _errors.Clear();
            foreach (var validationRule in ValidationRules)
            {
                var validationResponse = validationRule.Validate(this);
                if (validationResponse.IsValid == false)
                {
                    lastValidationResult = false;
                    _errors.Add(validationResponse.ErrorContent);
                    foreach (var validationPropertyName in ValidationPropertyNames)
                        NotifyErrorsChanged(validationPropertyName);
                    return false;
                }
            }
            lastValidationResult = true;
            foreach (var validationPropertyName in ValidationPropertyNames)
                NotifyErrorsChanged(validationPropertyName);
            return true;
        }

        public void AddError(string error)
        {
            _errors.Add(error);
            foreach (var validationPropertyName in ValidationPropertyNames)
                NotifyErrorsChanged(validationPropertyName);
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
            OnPropertyChanged("Value");
            //Removed On Change Validation Because Some Do Service Connections (XrmRecordConfiguration) To Validate
            //And Caused Selection To Delay
            //So Just Validate On Save
            //Validate();
            OnChange();
            OnChangeDelegate(this);
           if (!lastValidationResult)
                DoOnAsynchThread(() => Validate());
        }

        public void SetLoading()
        {
            LoadingViewModel.IsLoading = true;
        }

        public void SetNotLoading()
        {
            LoadingViewModel.IsLoading = false;
        }

        public virtual void OnChange()
        {
        }

        public LoadingViewModel LoadingViewModel { get; set; }
        public bool DisplayLabel { get; set; }
        public virtual bool IsLoaded { get { return true; } }

        public string Description { get; set; }

        public bool DoNotLimitDisplayHeight { get; set; }
    }
}