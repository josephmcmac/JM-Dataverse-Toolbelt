using System;
using System.Collections;
using System.Linq;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Service;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Fakes;

namespace JosephM.Prism.Infrastructure.Test
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

            var testApplication = new TestApplication();
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
            var testApplication = new TestApplication();
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