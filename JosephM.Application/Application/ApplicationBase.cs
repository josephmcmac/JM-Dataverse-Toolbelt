using System;
using System.Collections.Generic;
using System.Reflection;
using JosephM.Application.Modules;
using JosephM.Application.Options;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;

namespace JosephM.Application.Application
{
    public class ApplicationBase
    {
        public ApplicationControllerBase Controller { get; set; }

        public ApplicationBase(ApplicationControllerBase applicationController, IApplicationOptions applicationOptions, ISettingsManager settingsManager)
        {
            Modules = new Dictionary<Type, ModuleBase>();
            Controller = applicationController;
            Controller.RegisterInfrastructure(applicationOptions, settingsManager);
        }

        public IDictionary<Type, ModuleBase> Modules { get; set; }

        public T AddModule<T>()
            where T : ModuleBase, new()
        {
            return AddModule(typeof(T)) as T;
        }

        public T GetModule<T>()
            where T : ModuleBase, new()
        {
            if (!Modules.ContainsKey(typeof(T)))
                throw new NullReferenceException(string.Format("Type {0} is not loaded as a module", typeof(T).Name));
            return (T)Modules[typeof(T)];
        }

        public object AddModule(Type moduleType)
        {
            if (Modules.ContainsKey(moduleType))
                return Modules[moduleType];

            var moduleController = Controller.ResolveType<ModuleController>();

            var dependantModuleAttributes =
                moduleType.GetCustomAttributes<DependantModuleAttribute>();
            foreach (var dependantModule in dependantModuleAttributes)
                AddModule(dependantModule.DependantModule);

            //okay it needs to add items to the container
            if (!moduleType.IsTypeOf(typeof(ModuleBase)))
                throw new NullReferenceException(string.Format("Object type {0} is not of type {1}", moduleType.Name,
                    typeof(ModuleBase).Name));
            if (!moduleType.HasParameterlessConstructor())
                throw new NullReferenceException(
                    string.Format("Object type {0} does not have a parameterless constructor", moduleType.Name));

            var theModule = (ModuleBase)moduleType.CreateFromParameterlessConstructor();
            theModule.Controller = moduleController;
            theModule.RegisterTypes();
            theModule.InitialiseModule();
            Modules.Add(moduleType, theModule);
            moduleController.AddLoadedModule(theModule);
            return theModule;
        }

        protected void LogError(string message)
        {
            Controller.UserMessage(message);
        }

        public Type AppImageUserControlType
        {
            get
            {
                return Controller.AppImageType;
            }
        }
        public bool HasAppImageUserControl
        {
            get
            {
                return Controller.AppImageType != null;
            }
        }    
    }
}