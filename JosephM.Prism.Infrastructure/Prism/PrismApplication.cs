#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Application.Modules;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Module;

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
            where T : new()
        {
            AddModule(typeof (T));
        }

        private void AddModule(Type moduleType)
        {
            var prismModuleType = typeof(Module<>);
            //if (!moduleType.IsTypeOf(prismModuleType))
            //    throw new Exception(string.Format("Type {0} registered as a module is not of type {1}", moduleType.Name, prismModuleType.Name));

            var dependantModuleAttributes =
                moduleType.GetCustomAttributes(typeof(DependantModuleAttribute), true)
                    .Cast<DependantModuleAttribute>();
            foreach (var dependantModule in dependantModuleAttributes)
                AddModule(dependantModule.DependantModule);
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