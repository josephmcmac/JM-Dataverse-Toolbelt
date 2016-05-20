using $safeprojectname$.Xrm;

namespace $safeprojectname$.Services
{
    /// <summary>
    /// A service class for performing logic
    /// </summary>
    public class TemplateService
    {
        private XrmService XrmService { get; set; }

        public TemplateService(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        public void SampleServiceMethod()
        {

        }
    }
}
