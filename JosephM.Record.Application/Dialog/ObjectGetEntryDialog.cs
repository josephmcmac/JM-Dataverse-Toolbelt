#region

using System;
using JosephM.Application.Application;
using JosephM.Record.IService;
using System.Collections.Generic;

#endregion

namespace JosephM.Application.ViewModel.Dialog
{
    public class ObjectGetEntryDialog : ObjectEntryDialogBase
    {
        public ObjectGetEntryDialog(Func<object> objectToEnter, 
            IDialogController dialogController, Action saveMethod, IDictionary<string, Type> objectTypeMaps = null)
            : base(dialogController, null, null, saveMethod, objectTypeMaps)
        {
            _objectToEnter = objectToEnter;
        }

        public ObjectGetEntryDialog(Func<object> objectToEnter, DialogViewModel parentDialog,
            IApplicationController applicationController, Action saveMethod, IDictionary<string, Type> objectTypeMaps = null)
            : base(parentDialog, applicationController, null, null, saveMethod, objectTypeMaps)
        {
            _objectToEnter = objectToEnter;
        }

        public ObjectGetEntryDialog(Func<object> objectToEnter, DialogViewModel parentDialog,
    IApplicationController applicationController, IRecordService lookupService, IDictionary<string, Type> objectTypeMaps = null)
    : base(parentDialog, applicationController, lookupService, null, null, objectTypeMaps)
        {
            _objectToEnter = objectToEnter;
        }

        private readonly Func<object> _objectToEnter;

        protected override object ObjectToEnter
        {
            get { return _objectToEnter(); }
        }
    }
}