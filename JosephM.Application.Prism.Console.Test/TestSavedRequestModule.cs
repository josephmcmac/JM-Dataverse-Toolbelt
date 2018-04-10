using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Attributes;
using JosephM.Core.Log;
using JosephM.Core.Service;

namespace JosephM.Application.Prism.Console.Test
{
    /// <summary>
    /// module for saving a request and running in the console application
    /// </summary>
    [AllowSaveAndLoad]
    public class TestSavedRequestDialogRequest : ServiceRequestBase
    {
        public string SomeArbitraryString { get; set; }
    }

    public class TestSavedRequestModule : ServiceRequestModule<TestSavedRequestDialog, TestSavedRequestDialogService, TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
    {
    }

    public class TestSavedRequestDialog : ServiceRequestDialog<TestSavedRequestDialogService, TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
    {
        public TestSavedRequestDialog(IDialogController controller)
            : base(new TestSavedRequestDialogService(), controller)
        {

        }
    }

    public class TestSavedRequestDialogService : ServiceBase<TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
    {
        public override void ExecuteExtention(TestSavedRequestDialogRequest request, TestSavedRequestDialogResponse response, LogController controller)
        {
            return;
        }
    }

    public class TestSavedRequestDialogResponseItem : ServiceResponseItem
    {
    }

    public class TestSavedRequestDialogResponse : ServiceResponseBase<TestSavedRequestDialogResponseItem>
    {
    }
}
