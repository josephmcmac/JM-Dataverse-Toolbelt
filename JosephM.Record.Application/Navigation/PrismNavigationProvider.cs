using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using Microsoft.Practices.Prism.Regions;

namespace JosephM.Application.ViewModel.Navigation
{
    public class PrismNavigationProvider : INavigationProvider
    {
        private NavigationContext NavigationContext { get; set; }

        public PrismNavigationProvider(NavigationContext navigationContext)
        {
            NavigationContext = navigationContext;
        }

        public string GetValue(string key)
        {
            return NavigationContext.Parameters[key];
        }

        public bool HasValue(string key)
        {
            return !GetValue(key).IsNullOrWhiteSpace();
        }

        public T GetObject<T>(string key)
        {
            var json = GetValue(key);
            var type = typeof(T);
            return (T)JsonHelper.JsonStringToObject(json, type);
        }
    }
}