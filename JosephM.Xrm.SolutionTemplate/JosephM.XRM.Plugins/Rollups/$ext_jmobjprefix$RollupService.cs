using Microsoft.Xrm.Sdk;
using $safeprojectname$.Xrm;
using Schema;
using System;
using System.Collections.Generic;

namespace $safeprojectname$.Rollups
{
    public class $ext_jmobjprefix$RollupService : RollupService
    {
        public $ext_jmobjprefix$RollupService(XrmService xrmService)
            : base(xrmService)
        {
        }

        private IEnumerable<LookupRollup> _Rollups = new LookupRollup[]
        {
        };

        public override IEnumerable<LookupRollup> AllRollups => _Rollups;
    }
}