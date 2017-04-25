using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using $josephmrootnamespace$.Xrm;

namespace $rootnamespace$
{
    /// <summary>
    /// This class is for the static type required for registration of the custom workflow activity in CRM
    /// </summary>
    public class $safeitemname$ : XrmWorkflowActivityRegistration
    {
        //[Input("Input Arg")]
        //[ReferenceTarget(Entities.account)]
        //public InArgument<EntityReference> InputArg { get; set; }

        //[Output("Output Arg")]
        //public OutArgument<bool> OutputArg { get; set; }

        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new $safeitemname$Instance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class $safeitemname$Instance
        : $jmobjprefix$WorkflowActivity<$safeitemname$>
    {
        protected override void Execute()
        {
        }

    }
}
