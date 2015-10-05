#region

using System;
using System.Collections.Generic;
using JosephM.Application.Application;
using JosephM.Record.IService;

#endregion

namespace JosephM.Application.ViewModel.Dialog
{
    public class ObjectEntryDialog : ObjectEntryDialogBase
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
            : this(objectsToEnter, parentDialog, applicationController, lookupService, optionsetLimitedValues, null)
        {
            _objectToEnter = objectsToEnter;
        }

        public ObjectEntryDialog(object objectsToEnter, DialogViewModel parentDialog,
    IApplicationController applicationController, IRecordService lookupService,
    IDictionary<string, IEnumerable<string>> optionsetLimitedValues,Action onSave)
            : base(parentDialog, applicationController, lookupService, optionsetLimitedValues, onSave)
        {
            _objectToEnter = objectsToEnter;
        }


        private readonly object _objectToEnter;

        protected override object ObjectToEnter
        {
            get { return _objectToEnter; }
        }
    }
}