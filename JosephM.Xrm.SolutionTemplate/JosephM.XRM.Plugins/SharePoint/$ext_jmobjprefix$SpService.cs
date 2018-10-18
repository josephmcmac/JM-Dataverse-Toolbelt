using System.Collections.Generic;
using $safeprojectname$.Xrm;

namespace $safeprojectname$.SharePoint
{
    public class $ext_jmobjprefix$SharepointService : SharePointService
    {
        public $ext_jmobjprefix$SharepointService(XrmService xrmService, ISharePointSettings sharepointSettings)
            : base(sharepointSettings, xrmService)
        {
        }

        public override IEnumerable<SharepointFolderConfig> SharepointFolderConfigs
        {
            get
            {

                return new SharepointFolderConfig[]
                {
                };
            }
        }
    }
}
