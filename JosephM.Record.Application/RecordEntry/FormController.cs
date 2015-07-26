using JosephM.Record.Application.Controller;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.IService;
using JosephM.Record.Service;

namespace JosephM.Record.Application.RecordEntry
{
    public class FormController
    {
        public static FormController CreateForObject(object objectToEnter, IApplicationController applicationController, IRecordService lookupService)
        {
            var recordService = new ObjectRecordService(objectToEnter, lookupService, null);
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