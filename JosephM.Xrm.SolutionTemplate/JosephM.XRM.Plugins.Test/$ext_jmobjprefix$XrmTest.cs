using $ext_safeprojectname$.Plugins.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace $safeprojectname$
{
    [TestClass]
    public class $ext_jmobjprefix$XrmTest : XrmTest
    {
        //class for shared services or settings objects for tests
        //or extending base class logic
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
                    _service = new $ext_jmobjprefix$Service(XrmService);
                return _service;
            }
        }
    }
}
