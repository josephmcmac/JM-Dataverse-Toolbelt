#region

using JosephM.Application.Modules;
using Microsoft.Practices.Prism.Modularity;

#endregion

namespace JosephM.Prism.Infrastructure.Module
{
    /// <summary>
    ///     Class Maps A Custom PrismModuleBase Type Into The Module Type Required By The Prism Unity Engine
    /// </summary>
    /// <typeparam name="TModule"></typeparam>
    internal class Module<TModule> : IModule
        where TModule : ModuleBase, new()
    {
        public Module(ModuleController controller)
        {
            PrismModuleBase = new TModule { Controller = controller };
        }

        private ModuleBase PrismModuleBase { get; set; }

        public void Initialize()
        {
            PrismModuleBase.RegisterTypes();
            PrismModuleBase.InitialiseModule();
            PrismModuleBase.Controller.AddLoadedModule(PrismModuleBase);
        }
    }
}