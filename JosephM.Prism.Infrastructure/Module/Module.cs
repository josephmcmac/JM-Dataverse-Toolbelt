#region

using Microsoft.Practices.Prism.Modularity;
using JosephM.Prism.Infrastructure.Prism;

#endregion

namespace JosephM.Prism.Infrastructure.Module
{
    /// <summary>
    ///     Class Maps A Custom PrismModuleBase Type Into The Module Type Required By The Prism Unity Engine
    /// </summary>
    /// <typeparam name="TModule"></typeparam>
    internal class Module<TModule> : IModule
        where TModule : PrismModuleBase, new()
    {
        public Module(IPrismModuleController controller)
        {
            PrismModuleBase = new TModule {Controller = controller};
        }

        private PrismModuleBase PrismModuleBase { get; set; }

        public void Initialize()
        {
            PrismModuleBase.RegisterTypes();
            PrismModuleBase.InitialiseModule();
        }
    }
}