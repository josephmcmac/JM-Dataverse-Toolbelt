using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using $ext_safeprojectname$.Plugins.Localisation;
using $ext_safeprojectname$.Plugins.Services;

namespace $safeprojectname$
{
    [TestClass]
    public class $ext_jmobjprefix$XrmTest : XrmTest
    {
        //public override Guid? TestUserId => new Guid("00000000-0000-0000-0000-000000000000");

        protected override IEnumerable<string> EntitiesToDelete
        {
            get
            {
                return new string[0];
            }
        }

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