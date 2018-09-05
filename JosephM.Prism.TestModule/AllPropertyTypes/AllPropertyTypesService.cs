using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using System;
using System.Threading;

namespace JosephM.AllPropertyTypesModule.AllPropertyTypesDialog
{
    public class AllPropertyTypesService :
        ServiceBase<AllPropertyTypesRequest, AllPropertyTypesResponse, AllPropertyTypesResponseItem>
    {
        public override void ExecuteExtention(AllPropertyTypesRequest request, AllPropertyTypesResponse response,
            LogController controller)
        {
        }
    }
}