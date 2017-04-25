using $safeprojectname$.Xrm;

namespace $safeprojectname$.Services
{
    /// <summary>
    /// A service class for performing logic
    /// </summary>
    public class $ext_jmobjprefix$Service
    {
        private XrmService XrmService { get; set; }

        public $ext_jmobjprefix$Service(XrmService xrmService)
        {
            XrmService = xrmService;
        }
    }
}
