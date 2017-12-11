using JosephM.Core.Log;
using JosephM.Core.Service;
using System;
using System.Threading;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    public class TestDialogService :
        ServiceBase<TestDialogRequest, TestDialogResponse, TestDialogResponseItem>
    {
        public override void ExecuteExtention(TestDialogRequest request, TestDialogResponse response,
            LogController controller)
        {
            response.AddResponseItem(new TestDialogResponseItem("Dummy Response", null));
            if (request.ThrowResponseErrors)
            {
                for (var i = 0; i < 100; i++)
                {
                    response.AddResponseItem(new TestDialogResponseItem("Requested Error " + (i + 1),
                        new NotSupportedException("The Request Explicitly requested Errors",
                            new Exception("This Is The Inner Expection Text"))));
                }
            }
            for (var i = 0; i < 100; i++)
            {
                controller.UpdateProgress(i, 100, "Fake Progress");
                Thread.Sleep(10);
            }
        }
    }
}