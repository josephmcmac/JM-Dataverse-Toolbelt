
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.Test;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Record.Application.Test
{
    public class RecordApplicationTests : CoreTest
    {
        public ObjectEntryViewModel LoadToObjectEntryViewModel(TestViewModelValidationObject objectToEnter)
        {
            var recordService = new ObjectRecordService(objectToEnter);
            var formService = new ObjectFormService(objectToEnter, recordService);
            var viewModel = new ObjectEntryViewModel(EmptyMethod, EmptyMethod, objectToEnter,
                new FormController(recordService, formService, new FakeApplicationController()));
            Assert.IsNotNull(viewModel.FormSectionsAsync);
            return viewModel;
        }

        public void EmptyMethod()
        {

        }
    }
}
