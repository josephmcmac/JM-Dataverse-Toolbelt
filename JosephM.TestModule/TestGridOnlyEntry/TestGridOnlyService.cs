using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using System;
using System.Threading;

namespace JosephM.TestModule.TestGridOnly
{
    public class TestGridOnlyService :
        ServiceBase<TestGridOnlyRequest, TestGridOnlyResponse, TestGridOnlyResponseItem>
    {
        public override void ExecuteExtention(TestGridOnlyRequest request, TestGridOnlyResponse response,
            ServiceRequestController controller)
        {
        }
    }
}