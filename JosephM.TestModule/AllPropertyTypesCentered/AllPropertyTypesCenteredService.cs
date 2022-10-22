using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using System;
using System.Threading;

namespace JosephM.TestModule.AllPropertyTypesCentered
{
    public class AllPropertyTypesCenteredService :
        ServiceBase<AllPropertyTypesCenteredRequest, AllPropertyTypesCenteredResponse, AllPropertyTypesCenteredResponseItem>
    {
        public override void ExecuteExtention(AllPropertyTypesCenteredRequest request, AllPropertyTypesCenteredResponse response,
            ServiceRequestController controller)
        {
            response.EnumerableField = new[]
            {
                new AllPropertyTypesCenteredResponse(),
                new AllPropertyTypesCenteredResponse()
            };
        }
    }
}