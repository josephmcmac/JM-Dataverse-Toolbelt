using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Attributes;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Application.Prism.Console.Test
{
    /// <summary>
    /// module for saving a request and running in the console application
    /// including the service constructor using the active xrm connection
    /// </summary>
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class TestSavedRequestWithXrmConnectionModule : ServiceRequestModule<TestSavedRequestWithXrmConnectionDialog, TestSavedRequestWithXrmConnectionDialogService, TestSavedRequestWithXrmConnectionDialogRequest, TestSavedRequestWithXrmConnectionDialogResponse, TestSavedRequestWithXrmConnectionDialogResponseItem>
    {
    }

    [AllowSaveAndLoad]
    public class TestSavedRequestWithXrmConnectionDialogRequest : ServiceRequestBase
    {
        public string SomeArbitraryString { get; set; }
    }

    public class TestSavedRequestWithXrmConnectionDialog : ServiceRequestDialog<TestSavedRequestWithXrmConnectionDialogService, TestSavedRequestWithXrmConnectionDialogRequest, TestSavedRequestWithXrmConnectionDialogResponse, TestSavedRequestWithXrmConnectionDialogResponseItem>
    {
        public TestSavedRequestWithXrmConnectionDialog(IDialogController controller, TestSavedRequestWithXrmConnectionDialogService service)
            : base(service, controller)
        {

        }
    }

    public class TestSavedRequestWithXrmConnectionDialogService : ServiceBase<TestSavedRequestWithXrmConnectionDialogRequest, TestSavedRequestWithXrmConnectionDialogResponse, TestSavedRequestWithXrmConnectionDialogResponseItem>
    {
        public TestSavedRequestWithXrmConnectionDialogService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(TestSavedRequestWithXrmConnectionDialogRequest request, TestSavedRequestWithXrmConnectionDialogResponse response, LogController controller)
        {
            var isValid = XrmRecordService.VerifyConnection();
            if (!isValid.IsValid)
                throw new Exception($"Connection Error: {isValid.GetErrorString()}");
            return;
        }
    }

    public class TestSavedRequestWithXrmConnectionDialogResponseItem : ServiceResponseItem
    {
    }

    public class TestSavedRequestWithXrmConnectionDialogResponse : ServiceResponseBase<TestSavedRequestWithXrmConnectionDialogResponseItem>
    {
    }
}
