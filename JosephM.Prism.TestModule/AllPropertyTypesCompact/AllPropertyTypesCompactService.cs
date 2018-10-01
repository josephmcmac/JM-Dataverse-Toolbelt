using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using System;
using System.Threading;

namespace JosephM.TestModule.AllPropertyTypesCompact
{
    public class AllPropertyTypesCompactService :
        ServiceBase<AllPropertyTypesCompactRequest, AllPropertyTypesCompactResponse, AllPropertyTypesCompactResponseItem>
    {
        public override void ExecuteExtention(AllPropertyTypesCompactRequest request, AllPropertyTypesCompactResponse response,
            ServiceRequestController controller)
        {
            response.EnumerableField = new[]
            {
                new AllPropertyTypesCompactResponse(),
                new AllPropertyTypesCompactResponse()
            };
        }
    }
}