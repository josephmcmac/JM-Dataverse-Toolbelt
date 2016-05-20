using $safeprojectname$.Services;
using $safeprojectname$.Xrm;

namespace $safeprojectname$.Workflow
{
    //base class for services or settings used across all workflow activities
    public abstract class DefenceHealthWorkflowActivity<T> : XrmWorkflowActivityInstance<T>
        where T : XrmWorkflowActivityRegistration
    {
        //class for shared services or settings objects for workflow activities
        private TemplateSettings _templateSettings;

        public TemplateSettings TemplateSettings
        {
            get
            {
                if (_templateSettings == null)
                    _templateSettings = new TemplateSettings(XrmService);
                return _templateSettings;
            }
        }

        private TemplateService _templateService;
        public TemplateService TemplateService
        {
            get
            {
                if (_templateService == null)
                    _templateService = new TemplateService(XrmService);
                return _templateService;
            }
        }
    }
}
