using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Linq;

namespace JosephM.Application.Desktop.Console.Test
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
            var validationDialog = new ValidationDisplayDialog(this);
            SubDialogs = SubDialogs.Union(new[] { validationDialog }).ToArray();
        }

        /// <summary>
        /// this is to replicate the
        /// import dialogs which display validation
        /// prior to the import
        /// </summary>
        public class ValidationDisplayDialog : DialogViewModel
        {
            public ValidationDisplayDialog(DialogViewModel parentDialog)
                : base(parentDialog)
            {
            }

            protected override void CompleteDialogExtention()
            {

            }

            protected override void LoadDialogExtention()
            {
                var iDisplay = new ArbitraryObject();
                AddObjectToUi(iDisplay,
                    nextAction: () =>
                    {
                        RemoveObjectFromUi(iDisplay);
                        StartNextAction();
                    },
                    backAction: () =>
                    {
                        RemoveObjectFromUi(iDisplay);
                        MoveBackToPrevious();
                    });

            }

            public class ArbitraryObject
            {
                public string IDisplaySomething {  get { return "WTF"; } }
            }
        }
    }

    public class TestSavedRequestDialogService : ServiceBase<TestSavedRequestDialogRequest, TestSavedRequestDialogResponse, TestSavedRequestDialogResponseItem>
    {
        public override void ExecuteExtention(TestSavedRequestDialogRequest request, TestSavedRequestDialogResponse response, ServiceRequestController controller)
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
