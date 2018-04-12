using JosephM.Core.Log;
using JosephM.Core.Service;
using System.Threading;

namespace JosephM.TestModule.TestGridEdit
{
    public class TestGridEditService :
        ServiceBase<TestGridEditRequest, TestGridEditResponse, TestGridEditResponseItem>
    {
        public override void ExecuteExtention(TestGridEditRequest request, TestGridEditResponse response,
            LogController controller)
        {
            response.AddResponseItem(new TestGridEditResponseItem("Dummy Response", null));
            for (var i = 0; i < 100; i++)
            {
                controller.UpdateProgress(i, 100, "Fake Progress");
                Thread.Sleep(10);
            }
        }
    }
}