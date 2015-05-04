#region

using System.Collections.Generic;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.IService;
using JosephM.Record.Service;

#endregion

namespace JosephM.Record.Application.Dialog
{
    public class ObjectEntryDialog : DialogViewModel
    {
        /// <summary>
        ///     Implementation Of DialogViewModel For Entering Data Into A CLR Object
        /// </summary>
        public ObjectEntryDialog(object objectsToEnter, DialogViewModel parentDialog,
            IApplicationController applicationController)
            : this(objectsToEnter, parentDialog, applicationController, null, null)
        {
        }

        public ObjectEntryDialog(object objectsToEnter, DialogViewModel parentDialog,
            IApplicationController applicationController, IRecordService lookupService,
            IDictionary<string, IEnumerable<string>> optionsetLimitedValues)
            : base(parentDialog)
        {
            ObjectToEnter = objectsToEnter;
            ApplicationController = applicationController;
            LookupService = lookupService;
            OptionsetLimitedValues = optionsetLimitedValues;
        }

        private object ObjectToEnter { get; set; }

        private ObjectEntryViewModel ViewModel { get; set; }

        private IRecordService LookupService { get; set; }

        private IDictionary<string, IEnumerable<string>> OptionsetLimitedValues { get; set; }

        protected override void LoadDialogExtention()
        {
            var recordService = new ObjectRecordService(ObjectToEnter, LookupService, OptionsetLimitedValues);
            var formService = new ObjectFormService(ObjectToEnter, recordService);
            ViewModel = new ObjectEntryViewModel(StartNextAction, OnCancel, ObjectToEnter,
                new FormController(recordService, formService, ApplicationController));
            Controller.LoadToUi(ViewModel);
        }

        protected override void CompleteDialogExtention()
        {
            Controller.RemoveFromUi(ViewModel);
        }
    }
}