using $ext_safeprojectname$.Plugins.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace $safeprojectname$
{
    [TestClass]
    public class abcXrmTest : XrmTest
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

        private abcSettings _settings;
        public abcSettings abcSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new abcSettings(XrmService);
                return _settings;
            }
        }

        private abcService _service;
        public abcService abcService
        {
            get
            {
                if (_service == null)
                    _service = new abcService(XrmService, abcSettings);
                return _service;
            }
        }
    }
}