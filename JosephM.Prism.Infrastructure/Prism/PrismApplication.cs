#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Prism.Infrastructure.Attributes;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Record.Application.ApplicationOptions;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    ///     Class For A Prism Application Instance To Load Modules The Run
    /// </summary>
    public class PrismApplication
    {
        public PrismApplication(string applicationName)
        {
            ApplicationName = applicationName;
            Modules = new List<Type>();
        }

        public string ApplicationName { get; set; }

        protected List<Type> Modules { get; set; }

        public void AddModule<T>()
            where T : PrismModuleBase, new()
        {
            AddModule(typeof (T));
        }

        private void AddModule(Type moduleType)
        {
            var dependantModuleAttributes =
                moduleType.GetCustomAttributes(typeof(DependantModuleAttribute), true)
                    .Cast<DependantModuleAttribute>();
            foreach (var dependantModule in dependantModuleAttributes)
                AddModule(dependantModule.DependantModule);
            var prismModuleType = typeof(Module<>);
            prismModuleType = prismModuleType.MakeGenericType(moduleType);
            if (Modules.All(m => m != prismModuleType))
                Modules.Add(prismModuleType);
        }

        public void Run()
        {
            var bootstrapper = new UnityBootstrapperExtention(this, Modules.ToArray());
            bootstrapper.Run();
        }
    }
}