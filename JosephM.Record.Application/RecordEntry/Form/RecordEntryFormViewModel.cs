using JosephM.Application.ViewModel.Navigation;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.RecordEntry.Section;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.Validation;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public abstract class RecordEntryFormViewModel : RecordEntryViewModelBase
    {
        public virtual int GridPageSize { get { return StandardPageSize; } }

        private ObservableCollection<SectionViewModelBase> _formSections;

        private string _recordType;

        protected RecordEntryFormViewModel(FormController formController, RecordEntryViewModelBase parentForm, string parentFormReference, IDictionary<string, IEnumerable<string>> onlyValidate = null, string saveButtonLabel = null, string cancelButtonLabel = null)
            : this(formController, onlyValidate, saveButtonLabel: saveButtonLabel, cancelButtonLabel: cancelButtonLabel)
        {
            _parentForm = parentForm;
            _parentFormReference = parentFormReference;
        }

        protected RecordEntryFormViewModel(FormController formController, IDictionary<string, IEnumerable<string>> onlyValidate = null, string saveButtonLabel = null, string cancelButtonLabel = null)
            : base(formController, onlyValidate)
        {
            SaveButtonViewModel = new XrmButtonViewModel(saveButtonLabel ?? "Save", DoOnSave, ApplicationController)
            {
                IsVisible = false
            };
            CancelButtonViewModel = new XrmButtonViewModel(cancelButtonLabel ?? "Cancel", () => OnCancel(), ApplicationController)
            {
                IsVisible = false
            };
            ChangedPersistentFields = new List<string>();
            LoadingViewModel.IsLoading = true;
            LoadingViewModel.LoadingMessage = "Please Wait While Loading";
        }

        public XrmButtonViewModel GetButton(string id)
        {
            if (CustomFunctions.Any(b => b.Id == id))
                return CustomFunctions.First(b => b.Id == id);
            if (CustomFunctions.Where(b => b.HasChildOptions).SelectMany(b => b.ChildButtons).Any(b => b.Id == id))
                return CustomFunctions.Where(b => b.HasChildOptions).SelectMany(b => b.ChildButtons).First(b => b.Id == id);
            throw new ArgumentOutOfRangeException("id", "No Button Found With Id Of " + id);
        }

        public void LoadCustomFunctions()
        {
            var customFunctions = FormService.GetCustomFunctions(GetRecordType(), this);
            LoadFormButtons(customFunctions);
        }

        private IEnumerable<CustomFormFunction> _loadedFormButtons;
        public void LoadFormButtons(IEnumerable<CustomFormFunction> functions)
        {
            ApplicationController.DoOnMainThread(() =>
            {
                if (functions == null)
                    functions = new CustomFormFunction[0];
                _loadedFormButtons = functions;
                _customFunctions =
                    new ObservableCollection<XrmButtonViewModel>(FormFunctionsToXrmButtons(functions));

                OnPropertyChanged("CustomFunctions");
            });
        }

        private ObservableCollection<XrmButtonViewModel> _customFunctions;

        public ObservableCollection<XrmButtonViewModel> CustomFunctions
        {
            get { return _customFunctions; }
        }

        public IEnumerable<XrmButtonViewModel> FormFunctionsToXrmButtons(IEnumerable<CustomFormFunction> functions)
        {
            var buttons = new List<XrmButtonViewModel>();
            foreach (var cf in functions)
            {
                var isVisible = cf.VisibleFunction(this);
                if (isVisible)
                {
                    if (cf.ChildFormFunctions != null && cf.ChildFormFunctions.Any())
                    {
                        var childButtons = FormFunctionsToXrmButtons(cf.ChildFormFunctions);
                        buttons.Add(new XrmButtonViewModel(cf.Id, cf.Label, childButtons, ApplicationController) { Description = cf.Description });
                    }
                    else
                    {
                        buttons.Add(new XrmButtonViewModel(cf.Id, cf.Label, () => cf.Function(this), ApplicationController) { Description = cf.Description });
                    }
                }
            }
            return buttons;
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
                OnPropertyChanged(nameof(FormSectionsAsync));
            }
        }

        public void Reload()
        {
            _formSections = new ObservableCollection<SectionViewModelBase>();
            StartNewAction(LoadFormSections);
        }

        public XrmButtonViewModel SaveButtonViewModel { get; private set; }

        public XrmButtonViewModel CancelButtonViewModel { get; private set; }

        public string RecordIdName { get; set; }

        public override string TabLabel
        {
            get { return "Create"; }
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
                if (value != null)
                    LoadCustomFunctions();
                OnPropertyChanged("TabLabel");
            }
        }

        public string RecordId { get; set; }

        public override Action<FieldViewModelBase> GetOnFieldChangeDelegate()
        {
            return f =>
            {
                AddChangedField(f);
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

        public override IEnumerable<FieldSectionViewModel> FieldSections
        {
            get { return FormSectionsAsync.Where(s => s is FieldSectionViewModel).Cast<FieldSectionViewModel>(); }
        }

        public FieldSectionViewModel GetFieldSection(string name)
        {
            return FieldSections.First(s => s.SectionLabel == name);
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

        public virtual void LoadFormSections()
        {
            //forcing enumeration up front
            var sections = FormService.GetFormMetadata(RecordType, RecordService).FormSections.ToArray();
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
            //now set the section view model property in the ui thread which will notify the ui with the sections
            DoOnMainThread(
                () =>
                {
                    FormSectionsAsync = new ObservableCollection<SectionViewModelBase>(sectionViewModels);
                    OnSectionLoaded();
                });
        }

        protected override bool ConfirmTabClose()
        {
            return ConfirmClose();
        }

        public override IEnumerable<FieldViewModelBase> FieldViewModels
        {
            get
            {
                if (_formSections == null)
                    throw new NullReferenceException("The Field Sections Are Not Loaded Yet. The Reload Method Needs To Have Been Called And Completed To Initialise It");
                return _formSections
                    .Where(fs => fs is FieldSectionViewModel)
                    .Cast<FieldSectionViewModel>()
                    .SelectMany(s => s.Fields)
                    .ToArray();
            }
        }

        protected internal override IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
        {
            return FormService.GetValidationRules(fieldName, GetRecordType());
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

        private readonly RecordEntryViewModelBase _parentForm;
        public override RecordEntryViewModelBase ParentForm
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
                    if (!section.DynamicGridViewModel.GridLoadError && section.GridRecords != null)
                        foreach (var record in section.GridRecords)
                            record.OnLoad();
                }

                SaveButtonViewModel.IsVisible = OnSave != null;
                CancelButtonViewModel.IsVisible = OnCancel != null;

                PostLoading();

                LoadingViewModel.IsLoading = false;
                LoadingViewModel.LoadingMessage = "Please Wait While Processing";
            }
        }

        protected virtual void PostLoading()
        {
            return;
        }

        public virtual string Instruction
        {
            get
            {
                return null;
            }
        }
    }
}