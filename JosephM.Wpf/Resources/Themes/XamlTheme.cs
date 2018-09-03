using JosephM.Core.Extentions;

namespace JosephM.Wpf.Resources.Themes
{
    public class XamlTheme
    {
        public string ResourceRelativePath { get; set; }

        public string ResourceRelativePathForLoading
        {
            get
            {
                return ResourceRelativePath.Replace(".baml", ".xaml");
            }
        }

        public override string ToString()
        {
            var startIndex = ResourceRelativePath.LastIndexOf("/") + 1;
            var endIndex = ResourceRelativePath.LastIndexOf(".");
            return ResourceRelativePath.Substring(startIndex, endIndex - startIndex).ToTitleCase();
        }
    }
}