using $safeprojectname$.Services;
using $safeprojectname$.Xrm;
using $safeprojectname$.Localisation;

namespace $safeprojectname$.Plugins
{
    /// <summary>
    /// class for shared services or settings objects for plugins
    /// </summary>
    public abstract class $ext_jmobjprefix$EntityPluginBase : XrmEntityPlugin
    {
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

        private LocalisationService _localisationService;
        public LocalisationService LocalisationService
        {
            get
            {
                if (_localisationService == null)
                    _localisationService = new LocalisationService(new LocalisationSettings());
                return _localisationService;
            }
        }
    }
}
