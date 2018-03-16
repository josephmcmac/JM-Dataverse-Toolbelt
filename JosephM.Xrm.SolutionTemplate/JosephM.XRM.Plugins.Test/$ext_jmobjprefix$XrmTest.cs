using $ext_safeprojectname$.Plugins.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace $safeprojectname$
{
    [TestClass]
    public class $ext_jmobjprefix$XrmTest : XrmTest
    {
        //USE THIS IF NEED TO VERIFY SCRIPTS FOR A PARTICULAR SECURITY ROLE
        //private XrmService _xrmService;
        //public override XrmService XrmService
        //{
        //    get
        //    {
        //        if (_xrmService == null)
        //        {
        //            var xrmConnection = new XrmConfiguration()
        //            {
        //                AuthenticationProviderType = XrmConfiguration.AuthenticationProviderType,
        //                DiscoveryServiceAddress = XrmConfiguration.DiscoveryServiceAddress,
        //                OrganizationUniqueName = XrmConfiguration.OrganizationUniqueName,
        //                Username = "",
        //                Password = ""
        //            };
        //            _xrmService = new XrmService(xrmConnection);
        //        }
        //        return _xrmService;
        //    }
        //}

        protected override IEnumerable<string> EntitiesToDelete
        {
            get
            {
                return new string[0];
            }
        }

        private $ext_jmobjprefix$Settings _settings;
        public $ext_jmobjprefix$Settings abcSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new $ext_jmobjprefix$Settings(XrmService);
                return _settings;
            }
        }

        private $ext_jmobjprefix$Service _service;
        public $ext_jmobjprefix$Service abcService
        {
            get
            {
                if (_service == null)
                    _service = new $ext_jmobjprefix$Service(XrmService, abcSettings);
                return _service;
            }
        }
    }
}