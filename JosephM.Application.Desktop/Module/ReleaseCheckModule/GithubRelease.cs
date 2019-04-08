using System.Collections.Generic;

namespace JosephM.Application.Desktop.Module.ReleaseCheckModule
{
    public class GithubRelease
    {
        public string tag_name { get; set; }

        public IEnumerable<Asset> assets { get; set; }

        public class Asset
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
        }
    }
}