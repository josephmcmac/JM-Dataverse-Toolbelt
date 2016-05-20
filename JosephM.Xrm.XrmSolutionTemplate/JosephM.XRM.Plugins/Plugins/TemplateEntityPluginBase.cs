using $safeprojectname$.Services;
using $safeprojectname$.Xrm;

namespace $safeprojectname$.Plugins
{
    public class TemplateEntityPluginBase : XrmEntityPlugin
    {
        //class for shared services or settings objects for plugins
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
