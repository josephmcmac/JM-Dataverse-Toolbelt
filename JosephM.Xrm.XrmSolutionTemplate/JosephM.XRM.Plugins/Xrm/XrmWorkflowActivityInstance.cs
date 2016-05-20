using System;

namespace $safeprojectname$.Xrm
{
    public abstract class XrmWorkflowActivityInstance<T> : XrmWorkflowActivityInstanceBase
        where T : XrmWorkflowActivityRegistration
    {
        public T ActivityThisType
        {
            get
            {
                if (Activity is T)
                    return (T)Activity;
                throw new Exception(string.Format("Activity Should Be Of Type {0}", typeof(T).Name));
            }
        }
    }
}