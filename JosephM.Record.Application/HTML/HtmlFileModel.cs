using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Regions;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Navigation;
using JosephM.Record.Application.TabArea;

namespace JosephM.Record.Application.HTML
{
    public class HtmlFileModel : TabAreaViewModelBase
    {
        public HtmlFileModel(IApplicationController controller)
            : base(controller)
        {
        }

        public string FileNameQualified { get; set; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var navigationProvider = new PrismNavigationProvider(navigationContext);
            if (!string.IsNullOrWhiteSpace(navigationProvider.GetValue("path")))
                FileNameQualified = navigationProvider.GetValue("path");
        }

        public override string TabLabel
        {
            get
            {
                try
                {
                    var fileName = Path.GetFileName(FileNameQualified);
                    return fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.Ordinal));
                }
                catch (Exception ex)
                {
                    return base.TabLabel;
                }
            }
        }
    }
}
