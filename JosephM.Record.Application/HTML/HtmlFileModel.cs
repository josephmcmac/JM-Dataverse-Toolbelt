using System;
using System.IO;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Application.ViewModel.TabArea;
using Microsoft.Practices.Prism.Regions;

namespace JosephM.Application.ViewModel.HTML
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
