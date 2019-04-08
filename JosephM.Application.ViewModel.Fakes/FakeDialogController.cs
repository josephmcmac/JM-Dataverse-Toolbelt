using System;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Field;

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeDialogController : DialogController
    {
        public FakeDialogController(IApplicationController applicationController)
            : base(applicationController)
        {

        }

        public override void LoadToUi(ViewModelBase viewModel)
        {
            base.LoadToUi(viewModel);
            //attempt to load the grids and data
            if (viewModel is RecordEntryFormViewModel)
                ProcessRecordEntryForm((RecordEntryFormViewModel)viewModel);
        }

        public override void ShowCompletionScreen(DialogViewModel dialog)
        {
            base.ShowCompletionScreen(dialog);
            if (dialog.FatalException != null)
                throw dialog.FatalException;
        }

        protected virtual void ProcessRecordEntryForm(RecordEntryFormViewModel viewModel)
        {
            viewModel.LoadFormSections();
        }

        public override void BeginDialog()
        {
            base.BeginDialog();
            if(UiItems.Any())
            {
                var uiItem = UiItems.First();
                if(uiItem is RecordEntryFormViewModel)
                {
                    var entryForm = (RecordEntryFormViewModel)uiItem;
                    entryForm.LoadFormSections();

                    //in the ui grids load asynch when the ui tries to bind to them
                    //so need to do it manually to get it to load in script
                    foreach (var field in entryForm.FieldViewModels)
                    {
                        if(field is EnumerableFieldViewModel)
                        {
                            var fm = (EnumerableFieldViewModel)field;
                            var loadIt = fm.DynamicGridViewModel.GridRecords;
                        }
                    }
                }
            }
        }
    }
}