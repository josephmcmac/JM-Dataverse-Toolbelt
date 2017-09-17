#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.RecordEntry.Section;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using JosephM.Core.AppConfig;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using JosephM.Application.ViewModel.TabArea;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public abstract class RecordEntryFormViewModel : RecordEntryViewModelBase
    {
        public virtual int GridPageSize { get { return StandardPageSize; } }

        private ObservableCollection<SectionViewModelBase> _formSections;

        private List<FieldViewModelBase> _recordFields;
        private string _recordType;

        protected RecordEntryFormViewModel(FormController formController, RecordEntryViewModelBase parentForm, string parentFormReference, IDictionary<string, IEnumerable<string>> onlyValidate = null)
            : this(formController, onlyValidate)
        {
            _parentForm = parentForm;
            _parentFormReference = parentFormReference;
        }

        protected RecordEntryFormViewModel(FormController formController, IDictionary<string, IEnumerable<string>> onlyValidate = null)
            : base(formController, onlyValidate)
        {
            SaveButtonViewModel = new XrmButtonViewModel(SaveButtonLabel, DoOnSave, ApplicationController)
            {
                IsVisible = false
            };
            CancelButtonViewModel = new XrmButtonViewModel(CancelButtonLabel, () => OnCancel(), ApplicationController)
            {
                IsVisible = false
            };
            LoadRequestButtonViewModel = new XrmButtonViewModel("Load Saved Details", LoadObject, ApplicationController)
            {
                IsVisible = false
            };
            SaveRequestButtonViewModel = new XrmButtonViewModel("Save Entered Details", SaveObject, ApplicationController)
            {
                IsVisible = false
            };
            ChangedPersistentFields = new List<string>();
            LoadingViewModel.IsLoading = true;
        }

        public List<string> ChangedPersistentFields { get; private set; }

        /// <summary>
        ///     WARNING!!! Populates itself asyncronously the first time its requested
        /// </summary>
        public virtual ObservableCollection<SectionViewModelBase> FormSectionsAsync
        {
            get
            {
                if (_formSections == null)
                {
                    //Note this return an empty collection and spawns a new thread to load the sections
                    //this is to free up the ui
                    //once loaded it raises the property updated event
                    Reload();
                }
                return _formSections;
            }
            set
            {
                _formSections = value;
                foreach (var item in _formSections)
                {
                    item.IsVisible = FormService.IsSectionInContext(item.SectionIdentifier, GetRecord());
                }
                OnPropertyChanged("FormSectionsAsync");
            }
        }

        protected void Reload()
        {
            _formSections = new ObservableCollection<SectionViewModelBase>();
            StartNewAction(LoadFormSections);
        }

        public XrmButtonViewModel SaveButtonViewModel { get; private set; }

        public XrmButtonViewModel CancelButtonViewModel { get; private set; }

        public XrmButtonViewModel SaveRequestButtonViewModel { get; private set; }

        public XrmButtonViewModel LoadRequestButtonViewModel { get; private set; }

        public string RecordIdName { get; set; }

        public override string TabLabel
        {
            get { return "Create"; }
        }

        public virtual string SaveButtonLabel
        {
            get { return "Save"; }
        }

        public virtual string CancelButtonLabel
        {
            get { return "Cancel"; }
        }

        protected bool HasChangedPersistentFields
        {
            get { return ChangedPersistentFields.Any(); }
        }

        public string RecordType
        {
            get { return _recordType; }
            set
            {
                _recordType = value;
                OnPropertyChanged("TabLabel");
            }
        }

        public string RecordId { get; set; }

        public override Action<FieldViewModelBase> GetOnFieldChangeDelegate()
        {
            return f =>
            {
                AddChangedField(f);
                foreach (var action in FormService.GetOnChanges(f.FieldName))
                    action(this);
            };
        }
        //
        public IEnumerable<EnumerableFieldViewModel> SubGrids
        {
            get
            {
                return FieldSections
                  .SelectMany(s => s.Fields)
                  .Where(f => f is EnumerableFieldViewModel)
                  .Cast<EnumerableFieldViewModel>();
            }
        }

        public IEnumerable<FieldSectionViewModel> FieldSections
        {
            get { return FormSectionsAsync.Where(s => s is FieldSectionViewModel).Cast<FieldSectionViewModel>(); }
        }

        public void UserMessage(string message)
        {
            ApplicationController.UserMessage(message);
        }

        private void AddChangedField(FieldViewModelBase fieldViewModel)
        {
            if (fieldViewModel.IsRecordServiceField)
                if (!ChangedPersistentFields.Contains(fieldViewModel.FieldName))
                    ChangedPersistentFields.Add(fieldViewModel.FieldName);
        }

        public Action OnCancel { get; set; }

        public virtual void SaveObject()
        {
            throw new NotImplementedException();
        }

        public virtual void LoadObject()
        {
            throw new NotImplementedException();
        }

        private bool ConfirmClose()
        {
            var continueCancel = true;
            if (HasChangedPersistentFields)
            {
                continueCancel =
                    ApplicationController.UserConfirmation(
                        "The form has pending changes are you sure you want to cancel");
            }
            return continueCancel;
        }

        private void DoOnSave()
        {
            DoOnAsynchThread(() =>
            {
                try
                {
                    if (Validate())
                    {
                        OnSave();
                        ApplicationController.Remove(RegionNames.MainTabRegion, this);
                    }
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
            });
        }

        public Action OnSave { get; set; }

        protected virtual void PreValidateExtention()
        {
        }

        public override bool Validate()
        {
            try
            {
                LoadingViewModel.IsLoading = true;
                PreValidateExtention();
                ValidationPrompt = null;
                var isValid = base.Validate();

                foreach (var section in FormSectionsAsync)
                {
                    if (!section.Validate())
                        isValid = false;
                }
                if (!isValid)
                {
                    ValidationPrompt = "There Were Validation Errors - Please Review Your Input And Retry";
                }
                else
                {
                    var finalResponse = ValidateFinal();
                    isValid = finalResponse.IsValid;
                    if (!isValid)
                        ValidationPrompt = finalResponse.GetErrorString();
                }
                return isValid;
            }
            finally
            {
                LoadingViewModel.IsLoading = false;
            }

        }

        public virtual IsValidResponse ValidateFinal()
        {
            return new IsValidResponse();
        }

        private string _validationPrompt;
        public string ValidationPrompt
        {
            get { return _validationPrompt; }
            set
            {
                _validationPrompt = value;
                OnPropertyChanged("ValidationPrompt");
            }
        }

        public void LoadFormSections()
        {
            //forcing enumeration up front
            var sections = FormService.GetFormMetadata(RecordType).FormSections.ToArray();
            var sectionViewModels = new List<SectionViewModelBase>();
            //Create the section view models

            foreach (var section in sections)
            {
                if (section is FormFieldSection)
                {
                    sectionViewModels.Add(new FieldSectionViewModel(
                        (FormFieldSection)section,
                        this
                        ));
                }
            }
            //we need to populate the RecordFields property with the generated field view models
            _recordFields = new List<FieldViewModelBase>();
            foreach (
                var formSection in
                    sectionViewModels.Where(fs => fs is FieldSectionViewModel).Cast<FieldSectionViewModel>()
                )
            {
                _recordFields.AddRange(formSection.Fields);
            }
            //now set the section view model property in the ui thread which will notify the ui with the sections
            DoOnMainThread(
                () =>
                {
                    FormSectionsAsync = new ObservableCollection<SectionViewModelBase>(sectionViewModels);
                    OnSectionLoaded();
                });
        }

        public virtual bool AllowSaveAndLoad
        {
            get { return false; }
        }

        protected override bool ConfirmTabClose()
        {
            return ConfirmClose();
        }

        public override IEnumerable<FieldViewModelBase> FieldViewModels
        {
            get
            {
                if (_recordFields == null)
                    throw new NullReferenceException("The Field Sections Are Not Loaded Yet. The Reload Method Needs To Have Been Called And Completed To Initialise It");
                return _recordFields;
            }
        }

        protected internal override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
        {
            return FormService.GetValidationRules(fieldName);
        }

        public void OnNavigatedTo(INavigationProvider navigationProvider)
        {
            if (!navigationProvider.GetValue(NavigationParameters.RecordType).IsNullOrWhiteSpace())
                RecordType = navigationProvider.GetValue(NavigationParameters.RecordType);
            if (!navigationProvider.GetValue(NavigationParameters.RecordIdName).IsNullOrWhiteSpace())
                RecordIdName = navigationProvider.GetValue(NavigationParameters.RecordIdName);
            if (!navigationProvider.GetValue(NavigationParameters.RecordId).IsNullOrWhiteSpace())
                RecordId = navigationProvider.GetValue(NavigationParameters.RecordId);
        }

        public string GetValidationSummary()
        {
            var validationBuilder = new StringBuilder();
            foreach (var fieldViewModelBase in FieldViewModels)
            {
                var message = fieldViewModelBase.GetErrorsString();
                if (!message.IsNullOrWhiteSpace())
                    validationBuilder.AppendLine(string.Format("{0}: {1}", fieldViewModelBase.Label, message));
            }
            foreach (var subGrid in SubGrids)
            {
                foreach (var gridRecord in subGrid.DynamicGridViewModel.GridRecords)
                {
                    foreach (var fieldViewModelBase in gridRecord.FieldViewModels)
                    {
                        var message = fieldViewModelBase.GetErrorsString();
                        if (!message.IsNullOrWhiteSpace())
                            validationBuilder.AppendLine(string.Format("{0} - {1}: {2}", subGrid.FieldName, fieldViewModelBase.Label, message));
                    }
                }
            }
            if (validationBuilder.ToString().Length == 0)
            {
                var finalResponse = ValidateFinal();
                var isValid = finalResponse.IsValid;
                if (!isValid)
                    validationBuilder.AppendLine(finalResponse.GetErrorString());
            }
            return validationBuilder.ToString();
        }

        protected override void RefreshVisibilityExtention()
        {
            if (FormSectionsAsync != null)
            {
                foreach (var section in SubGrids)
                {
                    section.IsVisible = FormService.IsSectionInContext(section.ReferenceName, GetRecord());
                }
                foreach (var section in FieldSections)
                {
                    section.IsVisible = section.Fields.Any(f => f.IsVisible);
                }
            }
        }

        public EnumerableFieldViewModel GetSubGridViewModel(string subgridName)
        {
            var matchingFields = FieldSections.SelectMany(s => s.Fields).Where(g => g.FieldName == subgridName);
            if (matchingFields.Any())
            {
                return (EnumerableFieldViewModel)matchingFields.First();
            }
            throw new ArgumentOutOfRangeException("subgridName", "No SubGrid In Has The SectionIdentifier: " + subgridName);
        }

        private readonly RecordEntryViewModelBase _parentForm;
        internal override RecordEntryViewModelBase ParentForm
        {
            get { return _parentForm; }
        }

        private readonly string _parentFormReference;
        internal override string ParentFormReference
        {
            get { return _parentFormReference; }
        }

        internal void OnSectionLoaded()
        {
            if (FormSectionsAsync.All(s => s.IsLoaded))
            {
                OnLoad();
                foreach (var section in SubGrids)
                {
                    if (section.GridRecords != null)
                        foreach (var record in section.GridRecords)
                            record.OnLoad();
                }

                SaveButtonViewModel.IsVisible = OnSave != null;
                CancelButtonViewModel.IsVisible = OnCancel != null;
                if (AllowSaveAndLoad)
                    SaveRequestButtonViewModel.IsVisible = true;
                if (AllowSaveAndLoad)
                    LoadRequestButtonViewModel.IsVisible = true;

                PostLoading();

                LoadingViewModel.IsLoading = false;
            }
        }

        protected virtual void PostLoading()
        {
            return;
        }
    }
}