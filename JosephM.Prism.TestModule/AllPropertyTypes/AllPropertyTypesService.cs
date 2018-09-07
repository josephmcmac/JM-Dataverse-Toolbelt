using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using System;
using System.Threading;

namespace JosephM.TestModule.AllPropertyTypesModule
{
    public class AllPropertyTypesService :
        ServiceBase<AllPropertyTypesRequest, AllPropertyTypesResponse, AllPropertyTypesResponseItem>
    {
        public override void ExecuteExtention(AllPropertyTypesRequest request, AllPropertyTypesResponse response,
            LogController controller)
        {
            response.EnumerableField = new[]
            {
                new AllPropertyTypesResponse(),
                new AllPropertyTypesResponse()
            };
        }
    }
}