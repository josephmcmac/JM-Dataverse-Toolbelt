using System.Activities;

namespace $safeprojectname$.Xrm
{
    public abstract class XrmWorkflowActivityRegistration : CodeActivity
    {
        protected override sealed void Execute(CodeActivityContext executionContext)
        {
            var instance = CreateInstance();
            instance.ExecuteBase(executionContext, this);
        }

        protected abstract XrmWorkflowActivityInstanceBase CreateInstance();
    }
}