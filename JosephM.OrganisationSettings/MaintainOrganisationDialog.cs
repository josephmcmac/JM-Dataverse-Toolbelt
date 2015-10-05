using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.OrganisationSettings
{
    public class MaintainOrganisationDialog : DialogViewModel
    {
        private XrmRecordService RecordService { get; set; }

        public MaintainOrganisationDialog(XrmRecordService recordService, IDialogController dialogController)
            : base(dialogController)
        {
            RecordService = recordService;
        }

        public MaintainViewModel EntryViewModel { get; set; }

        protected override void LoadDialogExtention()
        {
            LoadingViewModel.IsLoading = true;
            try
            {
                EntryViewModel =
                    new MaintainViewModel(new XrmOrganisationFormController(RecordService,
                        new XrmOrganisationFormService(), ApplicationController));
                EntryViewModel.OnSave = () =>
                {
                    LoadingViewModel.IsLoading = true;
                    try
                    {
                        RecordService.Update(EntryViewModel.GetRecord(), EntryViewModel.ChangedPersistentFields);
                        Controller.RemoveFromUi(EntryViewModel);
                    }
                    finally
                    {
                        LoadingViewModel.IsLoading = false;
                    }
                    StartNextAction();
                };
                EntryViewModel.OnCancel = () => TabCloseCommand.Execute(null);
                EntryViewModel.SetRecord(RecordService.GetFirst("organization"));
                Controller.LoadToUi(EntryViewModel);
            }
            finally
            {
                LoadingViewModel.IsLoading = false;
            }
        }

        protected override void CompleteDialogExtention()
        {
            CompletionMessage = "Settings Saved";
        }
    }
}