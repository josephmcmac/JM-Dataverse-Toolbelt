using System;
using JosephM.Core.Log;
using JosephM.Core.Service;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    public class TestDialogService :
        ServiceBase<TestDialogRequest, TestDialogResponse, TestDialogResponseItem>
    {
        public override void ExecuteExtention(TestDialogRequest request, TestDialogResponse response,
            LogController controller)
        {
            if (request.ThrowResponseErrors)
            {
                for (var i = 0; i < 100; i++)
                {
                    response.AddResponseItem(new TestDialogResponseItem("Requested Error " + (i + 1),
                        new NotSupportedException("The Request Explicitly requested Errors",
                            new Exception("This Is The Inner Expection Text"))));
                }
            }
        }
    }
}