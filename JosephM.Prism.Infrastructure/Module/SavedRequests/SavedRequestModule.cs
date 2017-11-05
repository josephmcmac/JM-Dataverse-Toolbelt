using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.ObjectMapping;
using JosephM.Record.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JosephM.Prism.Infrastructure.Module.SavedRequests
{
    /// <summary>
    /// This module adds a function to save (and subsequently load) details entered into a service request object
    /// </summary>
    public class SavedRequestModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
            //todo autoload logic should be in this module class
            AddSavedRequestsFormFunctions();
            AddSavedRequestLoadFunction();
        }

        /// <summary>
        /// This function adds the save and load buttons onto the form for an object type which implements IAllowSaveAndLoad
        /// </summary>
        private void AddSavedRequestsFormFunctions()
        {
            var customFormFunction = new CustomFormFunction("SAVEREQUEST", "Save Details", SaveObject, (re) => { return true; });
            this.AddCustomFormFunction(customFormFunction, typeof(IAllowSaveAndLoad));
            customFormFunction = new CustomFormFunction("LOADREQUEST", "Load/Edit Saved Details", LoadObject, AreSavedRequests);
            this.AddCustomFormFunction(customFormFunction, typeof(IAllowSaveAndLoad));
        }

        /// <summary>
        /// This function adds load button onto the SavedRequests subgrid when the "Load/Edit Saved Details" is clicked
        /// </summary>
        private void AddSavedRequestLoadFunction()
        {
            var customGridFunction = new CustomGridFunction("LOADREQUEST", "Load Selected", LoadSelected, (re) => { return true; });
            this.AddCustomGridFunction(customGridFunction, typeof(IAllowSaveAndLoad));
        }

        public bool AreSavedRequests(RecordEntryFormViewModel re)
        {
            var settingsManager = ApplicationController.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
            if (settingsManager == null)
                throw new NullReferenceException("settingsManager");

            var type = re.RecordType;
            var savedSettings = settingsManager.Resolve<SavedSettings>(Type.GetType(type));
            return savedSettings != null && savedSettings.SavedRequests.Any();
        }

        /// <summary>
        /// Load the selected grid row's object into the parent parent form
        /// </summary>
        public void LoadSelected(DynamicGridViewModel g)
        {
            try
            {
                if(!(g.SelectedRows.Count() == 1))
                {
                    g.ApplicationController.UserMessage("You Must Select 1 Row To Load");
                    return;
                }
                var parentForm = g.ParentForm as ObjectEntryViewModel;
                if (parentForm == null)
                    throw new NullReferenceException(string.Format("Error parent form is not of type {0}", typeof(ObjectEntryViewModel)));

                //get the selected object
                var selectionSubGrid = parentForm.GetSubGridViewModel(nameof(SavedSettings.SavedRequests));
                var selectedObjectRecord = selectionSubGrid.DynamicGridViewModel.SelectedRows.Count() == 1
                    ? selectionSubGrid.DynamicGridViewModel.SelectedRows.First().GetRecord() as ObjectRecord : null;
                var selectedObject = selectedObjectRecord == null ? null : selectedObjectRecord.Instance;

                if (selectedObject != null)
                {
                    //map the selected object into the parent parent forms object
                    var parentParentForm = parentForm.ParentForm as ObjectEntryViewModel;
                    if (parentParentForm == null)
                        throw new NullReferenceException(string.Format("Error parent parent form is not of type {0}", typeof(ObjectEntryViewModel)));

                    var mapper = new ClassSelfMapper();
                    mapper.Map(selectedObject, parentParentForm.GetObject());
                    if (selectedObject is ServiceRequestBase)
                        ((ServiceRequestBase)selectedObject).DisplaySavedSettingFields = false;

                    parentParentForm.LoadingViewModel.IsLoading = true;
                    //allow loading to display
                    Thread.Sleep(1000);

                    //reload the parent parent form fo4r the updated object
                    parentParentForm.Reload();
                    foreach (var grid in parentParentForm.SubGrids)
                    {
                        grid.DynamicGridViewModel.ReloadGrid();
                    }
                    
                }
                parentForm.LoadSubgridsToObject();
                parentForm.OnSave();
            }
            catch (Exception ex)
            {
                ApplicationController.ThrowException(ex);
            }
        }

        /// <summary>
        /// Load a form displaying the saved requests for selection
        /// </summary>
        public void LoadObject(RecordEntryFormViewModel re)
        {
            try
            {
                if (re is ObjectEntryViewModel)
                {
                    var oevm = re as ObjectEntryViewModel;
                    var theObject = oevm.GetObject();
                    var theObjectType = theObject.GetType();

                    var settingsManager = ApplicationController.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
                    if (settingsManager == null)
                        throw new NullReferenceException("settingsManager");

                    //get the saved requests
                    var savedSettings = settingsManager.Resolve<SavedSettings>(theObjectType);
                    if (!savedSettings.SavedRequests.Any())
                    {
                        ApplicationController.UserMessage(string.Format("There are no saved {0} records", theObjectType.GetDisplayName()));
                        return;
                    }
                    //set the dsaved requests to display the saved request details
                    foreach (var savedSetting in savedSettings.SavedRequests)
                    {
                        var casted = savedSetting as IAllowSaveAndLoad;
                        if (casted != null)
                            casted.DisplaySavedSettingFields = true;
                    }

                    //this tells the form to use this type for the properties list of objects
                    var objectTypeMaps = new Dictionary<string, Type>()
                    {
                        { nameof(SavedSettings.SavedRequests), theObjectType }
                    };

                    //this tells the form to only validate the name property of saved requests
                    var onlyValidate = new Dictionary<string, IEnumerable<string>>()
                    {
                        { theObjectType.AssemblyQualifiedName, new [] { nameof(IAllowSaveAndLoad.Name) } }
                    };

                    //on save any changes should be saved in the settings
                    Action savedLoadForm = () =>
                    {
                        settingsManager.SaveSettingsObject(savedSettings, theObjectType);
                        oevm.LoadCustomFunctions();
                        oevm.ClearChildForms();
                    };

                    //load the form
                    var dialogController = new DialogController(ApplicationController);
                    var recordService = new ObjectRecordService(savedSettings, null, null, ApplicationController, objectTypeMaps);
                    var formService = new ObjectFormService(savedSettings, recordService, objectTypeMaps);
                    var vm = new ObjectEntryViewModel(savedLoadForm, oevm.ClearChildForms, savedSettings, 
                        new FormController(recordService, formService, ApplicationController), re, "LOADING", onlyValidate: onlyValidate);

                    oevm.LoadChildForm(vm);
                }
            }
            catch (Exception ex)
            {
                ApplicationController.ThrowException(ex);
            }
        }

        /// <summary>
        /// Load a form for saving the details
        /// </summary>
        public void SaveObject(RecordEntryFormViewModel viewModel)
        {
            try
            {
                //subgrids don't map directly to object so need to unload them to object
                //before saving the record
                if (viewModel is ObjectEntryViewModel)
                {
                    var oevm = viewModel as ObjectEntryViewModel;
                    oevm.LoadSubgridsToObject();
                    var theObject = oevm.GetObject();
                    var theObjectType = theObject.GetType();
                    if (!theObjectType.IsTypeOf(typeof(IAllowSaveAndLoad)))
                        throw new Exception(string.Format("type {0} is not of type {1}", theObjectType.Name, typeof(IAllowSaveAndLoad).Name));

                    //this is an object specifically for entering the name and autoload properties
                    //they are mapped into the IAllowSaveAndLoad object after entry then it is saved
                    var saveObject = new SaveAndLoadFields();

                    Action saveSettings = () =>
                    {
                        //map the entered properties into the new object we are saving
                        var mapper = new ClassSelfMapper();
                        mapper.Map(saveObject, theObject);

                        var settingsManager = viewModel.ApplicationController.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
                        var settings = settingsManager.Resolve<SavedSettings>(theObjectType);

                        //if we selected autoload then set it false for the others
                        if (saveObject.Autoload)
                        {
                            foreach (var item in settings.SavedRequests.Cast<IAllowSaveAndLoad>())
                                item.Autoload = false;
                        }
                        //add the one and save
                        settings.SavedRequests = settings.SavedRequests.Union(new[] { theObject }).ToArray();
                        settingsManager.SaveSettingsObject(settings, theObjectType);

                        //reload the form and notify
                        viewModel.ClearChildForms();
                        viewModel.LoadCustomFunctions();
                        viewModel.ApplicationController.UserMessage("Saved!");
                    };

                    //load the entry form
                    var os = new ObjectRecordService(saveObject, viewModel.ApplicationController, null);
                    var ofs = new ObjectFormService(saveObject, os, null);
                    var fc = new FormController(os, ofs, viewModel.ApplicationController);

                    var vm = new ObjectEntryViewModel(saveSettings, () => viewModel.ClearChildForms(), saveObject, fc);
                    viewModel.LoadChildForm(vm);
                }
            }
            catch (Exception ex)
            {
                ApplicationController.ThrowException(ex);
            }
        }
    }
}
