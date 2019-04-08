using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Core.Test;
using JosephM.Record.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Application.ViewModel.Test
{
    public class RecordApplicationTests : CoreTest
    {
        public ObjectEntryViewModel LoadToObjectEntryViewModel(object objectToEnter)
        {
            var applicationController = new FakeApplicationController();
            var recordService = new ObjectRecordService(objectToEnter, applicationController);
            var formService = new ObjectFormService(objectToEnter, recordService);
            var viewModel = new ObjectEntryViewModel(EmptyMethod, EmptyMethod, objectToEnter,
                new FormController(recordService, formService, applicationController));
            viewModel.LoadFormSections();
            Assert.IsNotNull(viewModel.FormSectionsAsync);
            return viewModel;
        }

        public void EmptyMethod()
        {

        }
    }
}
