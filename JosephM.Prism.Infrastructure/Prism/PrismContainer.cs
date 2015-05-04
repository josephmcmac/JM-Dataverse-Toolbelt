#region

using Microsoft.Practices.Unity;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    ///     Encapsulates The Applications Dependency Resolver
    /// </summary>
    public class PrismContainer
    {
        public PrismContainer(IUnityContainer container)
        {
            Container = container;
        }

        private IUnityContainer Container { get; set; }

        public void RegisterTypeForNavigation<T>()
        {
            Container.RegisterTypeForNavigation<T>();
        }

        public void RegisterInstance<T>(T instance)
        {
// ReSharper disable once RedundantCast
            Container.RegisterInstance((T) instance);
        }

        public void RegisterType<T>()
        {
            Container.RegisterType<T>();
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}