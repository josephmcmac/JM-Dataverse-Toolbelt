using $safeprojectname$.Services;
using $safeprojectname$.Xrm;

namespace $safeprojectname$.Workflow
{
    //base class for services or settings used across all workflow activities
    public abstract class $ext_jmobjprefix$WorkflowActivity<T> : XrmWorkflowActivityInstance<T>
        where T : XrmWorkflowActivityRegistration
    {
        //class for shared services or settings objects for workflow activities
        private $ext_jmobjprefix$Settings _settings;
        public $ext_jmobjprefix$Settings $ext_jmobjprefix$Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new $ext_jmobjprefix$Settings(XrmService);
                return _settings;
            }
        }

        private $ext_jmobjprefix$Service _service;
        public $ext_jmobjprefix$Service $ext_jmobjprefix$Service
        {
            get
            {
                if (_service == null)
                    _service = new $ext_jmobjprefix$Service(XrmService, $ext_jmobjprefix$Settings);
                return _service;
            }
        }
    }
}
