using JosephM.Application.Desktop.Module.Dialog;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.Test;

namespace JosephM.Application.Desktop.Test
{
    public class DialogModuleTest<TModule, TDialog>
        : CoreTest
        where TDialog : DialogViewModel
        where TModule : DialogModule<TDialog>, new()
    {
        protected virtual void PrepareTest()
        {
            
        }

        public void ExecuteAutoEntryTest()
        {
            PrepareTest();

            var testApplication = TestApplication.CreateTestApplication();
            testApplication.Controller.RegisterType<IDialogController, AutoDialogController>();
            testApplication.AddModule<TModule>();
            var module = testApplication.GetModule<TModule>();
            module.DialogCommand();
            //autodialog should process the dialog when get it
            testApplication.GetNavigatedDialog<TDialog>();

            CompleteTest();
        }

        protected virtual void CompleteTest()
        {

        }

        public void ExecuteObjectEntryTest(object instanceToEnter)
        {
            var testApplication = TestApplication.CreateTestApplication();
            testApplication.Controller.RegisterType<IDialogController, FakeDialogController>();
            testApplication.AddModule<TModule>();
            var module = testApplication.GetModule<TModule>();
            module.DialogCommand();
            //autodialog should process the dialog when get it
            var dialog = testApplication.GetNavigatedDialog<TDialog>();
            var entryForm = testApplication.GetSubObjectEntryViewModel(dialog);
            testApplication.EnterAndSaveObject(instanceToEnter, entryForm);
        }
    }
}