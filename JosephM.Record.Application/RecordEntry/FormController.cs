using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Record.IService;
using JosephM.Record.Service;
using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry
{
    public class FormController
    {
        public static FormController CreateForObject(object objectToEnter, IApplicationController applicationController, IRecordService lookupService, IDictionary<string,IEnumerable<string>> optionSetLimitedvalues = null)
        {
            var recordService = new ObjectRecordService(objectToEnter, lookupService, optionSetLimitedvalues, applicationController);
            var formService = new ObjectFormService(objectToEnter, recordService);
            var formController = new FormController(recordService, formService, applicationController);
            return formController;
        }

        public FormController(IRecordService recordService, FormServiceBase formService,
            IApplicationController applicationController)
        {
            ApplicationController = applicationController;
            RecordService = recordService;
            FormService = formService;
        }

        public IRecordService RecordService { get; private set; }
        public FormServiceBase FormService { get; private set; }

        public IApplicationController ApplicationController { get; private set; }
    }
}