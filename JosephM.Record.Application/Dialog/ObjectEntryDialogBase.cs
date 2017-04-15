#region

using System;
using System.Collections.Generic;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Record.IService;
using JosephM.Record.Service;

#endregion

namespace JosephM.Application.ViewModel.Dialog
{
    /// <summary>
    ///     Implementation Of DialogViewModel For Entering Data Into A CLR Object
    /// </summary>
    public abstract class ObjectEntryDialogBase : DialogViewModel
    {
        protected ObjectEntryDialogBase(DialogViewModel parentDialog,
            IApplicationController applicationController, IRecordService lookupService,
            IDictionary<string, IEnumerable<string>> optionsetLimitedValues, Action saveMethod, IDictionary<string, Type> objectTypeMaps = null)
            : base(parentDialog)
        {
            ApplicationController = applicationController;
            LookupService = lookupService;
            OptionsetLimitedValues = optionsetLimitedValues;
            ObjectTypeMaps = objectTypeMaps;
            SaveMethod = saveMethod;
        }

        protected ObjectEntryDialogBase(
    IDialogController dialogController, IRecordService lookupService,
    IDictionary<string, IEnumerable<string>> optionsetLimitedValues, Action saveMethod, IDictionary<string, Type> objectTypeMaps = null)
    : base(dialogController)
        {
            ApplicationController = dialogController.ApplicationController;
            LookupService = lookupService;
            OptionsetLimitedValues = optionsetLimitedValues;
            ObjectTypeMaps = objectTypeMaps;
            SaveMethod = saveMethod;
        }

        protected abstract object ObjectToEnter { get; }

        public ObjectEntryViewModel ViewModel { get; set; }

        private IRecordService LookupService { get; set; }

        private IDictionary<string, IEnumerable<string>> OptionsetLimitedValues { get; set; }

        protected override void LoadDialogExtention()
        {
            var recordService = new ObjectRecordService(ObjectToEnter, LookupService, OptionsetLimitedValues, ApplicationController, ObjectTypeMaps);
            var formService = new ObjectFormService(ObjectToEnter, recordService, ObjectTypeMaps);
            ViewModel = new ObjectEntryViewModel(StartNextAction, OnCancel, ObjectToEnter,
                new FormController(recordService, formService, ApplicationController));
            Controller.LoadToUi(ViewModel);
        }

        protected Action SaveMethod{ get; set; }
        public IDictionary<string, Type> ObjectTypeMaps { get; private set; }

        protected override void CompleteDialogExtention()
        {
            try
            {
                if (SaveMethod != null)
                    SaveMethod();
                Controller.RemoveFromUi(ViewModel);
            }
            catch (Exception ex)
            {
                Controller.ApplicationController.ThrowException(ex);
                DialogCompletionCommit = false;
            }
        }
    }
}