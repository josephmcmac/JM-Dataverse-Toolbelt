using $safeprojectname$.Xrm;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace $safeprojectname$.Workflow
{
    /// <summary>
    /// Workflow activity to stored a boolean value in a workflow variable
    /// This for example allows setting of boolean fields in forms to be more visible in workflows
    /// where OOTB CRM does not display the explicit setting of boolean fields in workflow forms
    /// This class is for the static type required for registration of the custom workflow activity in CRM
    /// </summary>
    public class BooleanValue : XrmWorkflowActivityRegistration
    {
        [Input("Boolean In")]
        [Default("false")]
        public InArgument<bool> BooleanValueIn { get; set; }

        [Input("Invert")]
        [Default("false")]
        public InArgument<bool> Invert { get; set; }

        [Output("Result")]
        public OutArgument<bool> BooleanValueOut { get; set; }

        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new BooleanValueInstance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class BooleanValueInstance
        : DefenceHealthWorkflowActivity<BooleanValue>
    {
        protected override void Execute()
        {
            var result = ActivityThisType.BooleanValueIn.Get(ExecutionContext);
            if (ActivityThisType.Invert.Get(ExecutionContext))
                result = !result;
            ActivityThisType.BooleanValueOut.Set(ExecutionContext, result);
        }

    }
}
