using System;
using $safeprojectname$.Xrm;

namespace $safeprojectname$.SharePoint
{
    public class $ext_jmobjprefix$SharePointSettings : ISharePointSettings
    {
        public $ext_jmobjprefix$SharePointSettings(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        private string _username;
        public string UserName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private XrmService XrmService { get; }
    }
}
