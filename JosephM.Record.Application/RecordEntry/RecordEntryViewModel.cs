#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Core;
using JosephM.Record.Form;

#endregion

namespace JosephM.Record.Application.ViewModel
{
    public abstract class RecordEntryViewModel : TabAreaViewModelBase
    {
        private readonly List<IFormGrid> _subGrids = new List<IFormGrid>();
        private ObservableCollection<SectionViewModelBase> _formSections;

        private List<FieldViewModelBase> _recordFields;
        private string _recordType;

        protected RecordEntryViewModel(FormController formController)
            : base(formController.ApplicationController)
        {
            FormController = formController;
            SaveButtonViewModel = new XrmButtonViewModel(SaveButtonLabel, OnSave, ApplicationController)
            {
                IsVisible = false
            };
            CancelButtonViewModel = new XrmButtonViewModel(CancelButtonLabel, OnCancel, ApplicationController)
            {
                IsVisible = false
            };
            ChangedPersistentFields = new List<string>();

            FormInstance = FormService.CreateFormInstance(RecordType, this, RecordService);
        }

        public FormInstanceBase FormInstance { get; private set; }

        protected List<string> ChangedPersistentFields { get; private set; }

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
                    _formSections = new ObservableCollection<SectionViewModelBase>();

                    StartNewAction(LoadFormSections);
                }
                return _formSections;
            }
            set
            {
                _formSections = value;
                OnPropertyChanged("FormSectionsAsync");
            }
        }

        public IEnumerable<GridSectionViewModel> GetSubgrids()
        {
            return FormSectionsAsync.Where(s => s is GridSectionViewModel).Cast<GridSectionViewModel>();
        }

        public void StartNewAction(Action action)
        {
            ApplicationController.DoOnAsyncThread(action);
        }

        private FormController FormController { get; set; }

        public XrmButtonViewModel SaveButtonViewModel { get; private set; }

        public XrmButtonViewModel CancelButtonViewModel { get; private set; }

        public string RecordIdName { get; set; }

        public override string TabLabel
        {
            get { return "Create"; }
        }

        public virtual string SaveButtonLabel
        {
            get { return "Save"; }
        }

        public string CancelButtonLabel
        {
            get { return "Cancel"; }
        }

        protected bool HasChangedPersistentFields
        {
            get { return ChangedPersistentFields.Any(); }
        }

        public FormServiceBase FormService
        {
            get { return FormController.FormService; }
        }

        public IRecordService RecordService
        {
            get { return FormController.RecordService; }
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
        public abstract IRecord GetRecord();

        public Action<IFieldObject> GetOnFieldChangeDelegate()
        {
            return f =>
            {
                AddChangedField(f);
                foreach (var action in FormService.GetOnChanges(f.FieldName))
                    action(this);
                FormInstance.OnChange(f.FieldName);
            };
        }

        public IEnumerable<IFieldObject> RecordFields
        {
            get { return _recordFields; }
        }

        public IEnumerable<IFormGrid> SubGrids
        {
            get { return _subGrids; }
        }

        public void UserMessage(string message)
        {
            ApplicationController.UserMessage(message);
        }

        internal void AddSubGrid(IFormGrid subGrid)
        {
            _subGrids.Add(subGrid);
        }

        private void AddChangedField(IFieldObject fieldViewModel)
        {
            if (fieldViewModel.IsRecordServiceField)
                if (!ChangedPersistentFields.Contains(fieldViewModel.FieldName))
                    ChangedPersistentFields.Add(fieldViewModel.FieldName);
        }

        public void OnCancel()
        {
            var continueCancel = ConfirmClose();
            if (continueCancel)
            {
                OnCanceEntension();
            }
        }

        public virtual void OnCanceEntension()
        {
            ApplicationController.Remove(ApplicationRegionNames.MainTabRegion, this);
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

        public void OnSave()
        {
            DoWhileLoading(() =>
            {
                try
                {
                    if (Validate())
                    {
                        if (FormInstance.OnSaveConfirmation())
                        {
                            OnSaveExtention();
                            ApplicationController.Remove(ApplicationRegionNames.MainTabRegion, this);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApplicationController.UserMessage(ex.DisplayString());
                }
            });
        }

        public bool Validate()
        {
            var isValid = true;

            foreach (var recordField in _recordFields)
            {
                if (recordField.IsVisible && !recordField.Validate())
                    isValid = false;
            }
            return isValid;
        }

        public virtual void OnSaveExtention()
        {
        }

        public void LoadFormSections()
        {
            DoWhileLoading(() =>
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
                            FormController,
                            (FormFieldSection) section,
                            this
                            ));
                    }
                    else if (section is SubGridSection)
                    {
                        sectionViewModels.Add(new GridSectionViewModel(
                            FormController,
                            (SubGridSection) section,
                            this
                            ));
                    }
                }
                //we need to populate the RecordFields property with the generated field view models
                _recordFields = new List<FieldViewModelBase>();
                foreach (var formSection in sectionViewModels.Where(fs => fs is FieldSectionViewModel).Cast<FieldSectionViewModel>()
                    )
                {
                    _recordFields.AddRange(formSection.Fields);
                }
                //now set the section view model property in the ui thread which will notify the ui with the sections
                SendToDispatcher(
                    () => { FormSectionsAsync = new ObservableCollection<SectionViewModelBase>(sectionViewModels); });
                RefreshVisibility();

                FormInstance.OnLoad(this);
                SaveButtonViewModel.IsVisible = true;
                CancelButtonViewModel.IsVisible = true;
            });
        }

        protected override bool ConfirmTabClose()
        {
            return ConfirmClose();
        }

        internal void RefreshVisibility()
        {
            if (RecordFields != null)
            {
                foreach (var field in RecordFields)
                {
                    field.IsVisible = FormService.IsFieldInContext(field.FieldName, GetRecord());
                }
            }
        }

        internal IEnumerable<FieldViewModelBase> GetFieldViewModels()
        {
            return _recordFields;
        }

        internal FieldViewModelBase GetFieldViewModel(string fieldName)
        {
            if (GetFieldViewModels().Any(f => f.FieldName == fieldName))
            {
                return GetFieldViewModels().First(f => f.FieldName == fieldName);
            }
            throw new ArgumentOutOfRangeException(fieldName, "No Field In The Form Has The Name: " + fieldName);
        }

        internal IEnumerable<ValidationRuleBase> GetValidationRules(string fieldName)
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
    }
}