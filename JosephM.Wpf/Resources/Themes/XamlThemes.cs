using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace JosephM.Wpf.Resources.Themes
{
    public class XamlThemes
    {
        public IEnumerable<XamlTheme> Themes { get; set; }

        private static IEnumerable<string> GetThemeResources()
        {
            var themeRelativeFiles = new List<string>();

            var assembly = Assembly.GetExecutingAssembly();
            var resourceManager = new ResourceManager(assembly.GetName().Name + ".g", assembly);
            var resources = resourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            foreach (object resource in resources)
            {
                if (resource is DictionaryEntry)
                {
                    var entry = (DictionaryEntry)resource;
                    if (entry.Key is string)
                    {
                        var stringKey = entry.Key as string;
                        if (stringKey.ToLower().StartsWith("resources/themes/"))
                        {
                            themeRelativeFiles.Add(stringKey);
                        }
                    }
                }
            }

            return themeRelativeFiles;
        }

        public static XamlThemes LoadThemes()
        {
            return new XamlThemes
            {
                Themes = GetThemeResources().Select(s => new XamlTheme() { ResourceRelativePath = s })
            };
        }
    }
}