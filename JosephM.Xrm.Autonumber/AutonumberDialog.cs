using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.Autonumber
{
    [RequiresConnection]
    public class AutonumberDialog : DialogViewModel
    {
        public AutonumberDialog(XrmRecordService xrmRecordService, IDialogController dialogController)
            : base(dialogController)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        protected override void CompleteDialogExtention()
        {
            //okay all this does is load a form for navigating autonumbers
            var autonumberNavigator = new AutonumberNavigator();
            var formController = FormController.CreateForObject(autonumberNavigator, ApplicationController, XrmRecordService);
            var viewModel = new ObjectEntryViewModel(null, null, autonumberNavigator, formController);
            Controller.LoadToUi(viewModel);
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }
    }
}