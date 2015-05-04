using System.Collections.Generic;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.ApplicationOptions;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Fakes;

namespace JosephM.Prism.Infrastructure.Test
{
    public class FakePrismApplication<TModule> : PrismApplication
        where TModule : PrismModuleBase, new ()
    {
        public FakePrismApplication()
            : base("Fake Script Application")
        {
            
        }

        public void Initialise()
        {
            UnityContainer.RegisterInfrastructure<FakeDialogController>(ApplicationController);
            AddModule<TModule>();
            foreach (var moduleType in Modules)
            {
                var type = moduleType.GetGenericArguments()[0];
                var instance = (PrismModuleBase)type.CreateFromParameterlessConstructor();
                instance.Controller = PrismModuleController;
                instance.RegisterTypes();
                instance.InitialiseModule();
                ModuleInstances.Add(instance);
            }
        }

        private IUnityContainer _unityContainer;

        public IUnityContainer UnityContainer
        {
            get
            {
                if (_unityContainer == null)
                    _unityContainer = new UnityContainer();
                return _unityContainer;
            }
        }

        private PrismModuleController _prismModuleController;

        public PrismModuleController PrismModuleController
        {
            get
            {
                if (_prismModuleController == null)
                    _prismModuleController = new PrismModuleController(UnityContainer, ApplicationController,
                        new PrismSettingsManager(ApplicationController),
                        new ApplicationOptionsViewModel(ApplicationController));
                return _prismModuleController;
            }
        }

        public PrismContainer PrismContainer
        {
            get { return PrismModuleController.Container; }
        }

        public FakeRecordService FakeRecordService
        {
            get
            {
                return
                    FakeRecordService.Get();
            }
        }

        private IApplicationController _applicationController;

        public IApplicationController ApplicationController
        {
            get
            {
                if (_applicationController == null)
                    _applicationController = new FakeApplicationController();
                return _applicationController;
            }
        }

        private readonly List<PrismModuleBase> _moduleInstances = new List<PrismModuleBase>();

        private List<PrismModuleBase> ModuleInstances
        {
            get { return _moduleInstances; }
        }
    }
}