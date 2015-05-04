using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Practices.Prism.Regions;

namespace JosephM.Record.Application.Navigation
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

        public T GetObject<T>(string key)
        {
            var json = GetValue(key);
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var tObject = (T)serializer.ReadObject(stream);
                return tObject;
            }
        }
    }
}