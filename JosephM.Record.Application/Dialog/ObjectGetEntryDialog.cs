#region

using System;
using JosephM.Application.Application;

#endregion

namespace JosephM.Application.ViewModel.Dialog
{
    public class ObjectGetEntryDialog : ObjectEntryDialogBase
    {
        public ObjectGetEntryDialog(Func<object> objectsToEnter, DialogViewModel parentDialog,
            IApplicationController applicationController, Action saveMethod)
            : base(parentDialog, applicationController, null, null, saveMethod)
        {
            _objectToEnter = objectsToEnter;
        }

        private readonly Func<object> _objectToEnter;

        protected override object ObjectToEnter
        {
            get { return _objectToEnter(); }
        }
    }
}