using $safeprojectname$.Xrm;

namespace $safeprojectname$.Services
{
    /// <summary>
    /// A service class for performing logic
    /// </summary>
    public class $ext_jmobjprefix$Service
    {
        private XrmService XrmService { get; set; }
        private $ext_jmobjprefix$Settings $ext_jmobjprefix$Settings { get; set; }

        public $ext_jmobjprefix$Service(XrmService xrmService, $ext_jmobjprefix$Settings settings)
        {
            XrmService = xrmService;
            $ext_jmobjprefix$Settings = settings;
        }
    }
}
