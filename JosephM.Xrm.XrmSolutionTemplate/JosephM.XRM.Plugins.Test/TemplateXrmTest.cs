using $ext_safeprojectname$.Plugins.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace $safeprojectname$
{
    [TestClass]
    public class TemplateXrmTest : XrmTest
    {
        //class for shared services or settings objects for tests
        //or extending base class logic
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
