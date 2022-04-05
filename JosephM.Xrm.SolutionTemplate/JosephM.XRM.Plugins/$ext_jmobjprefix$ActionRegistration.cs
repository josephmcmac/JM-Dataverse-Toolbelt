using $safeprojectname$.Action;
using $safeprojectname$.Xrm;
using Schema;
using System;

namespace $safeprojectname$
{
    public class $ext_jmobjprefix$ActionRegistration : XrmActionRegistration
    {
        public $ext_jmobjprefix$ActionRegistration(string unsecureConfig, string secureConfig)
            : base(unsecureConfig, secureConfig)
        {

        }

        public override XrmAction CreateActionInstance(string actionName)
        {
            switch (actionName)
            {
                //case Actions.dcs_ActionName.Name : return new ActionInstance();
            }
            throw new NotImplementedException($"Action {actionName} has not been added to class {GetType().Name} for initialising the action instance");
        }
    }
}
