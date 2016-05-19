#region

using System;
using JosephM.Application.Application;
using JosephM.Record.IService;

#endregion

namespace JosephM.Application.ViewModel.Dialog
{
    public class ObjectGetEntryDialog : ObjectEntryDialogBase
    {
        public ObjectGetEntryDialog(Func<object> objectToEnter, DialogViewModel parentDialog,
            IApplicationController applicationController, Action saveMethod)
            : base(parentDialog, applicationController, null, null, saveMethod)
        {
            _objectToEnter = objectToEnter;
        }

        public ObjectGetEntryDialog(Func<object> objectToEnter, DialogViewModel parentDialog,
    IApplicationController applicationController, IRecordService lookupService)
    : base(parentDialog, applicationController, lookupService, null, null)
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