using System;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Form;

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
            else if (viewModel is CompletionScreenViewModel)
                ProcessCompletionScreen((CompletionScreenViewModel)viewModel);
        }

        public override void ShowCompletionScreen(DialogViewModel dialog)
        {
            base.ShowCompletionScreen(dialog);
            if (dialog.FatalException != null)
                throw dialog.FatalException;
        }

        private void ProcessCompletionScreen(CompletionScreenViewModel viewModel)
        {
            if (viewModel.CompletionOptions.Any())
            {
                foreach (var item in viewModel.CompletionOptions)
                {
                    try
                    {
                        item.Invoke();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error trigger completion option " + item.Label, ex);
                    }

                }
            }
        }

        protected virtual void ProcessRecordEntryForm(RecordEntryFormViewModel viewModel)
        {
            viewModel.LoadFormSections();
        }
    }
}