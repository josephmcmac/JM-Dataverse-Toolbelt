#region

using JosephM.Application.ViewModel.HTML;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeHtmlFileModel : HtmlFileModel
    {
        public FakeHtmlFileModel()
            : base(new FakeApplicationController())
        {
            FileNameQualified = "http://ntaagateway.myrw021.com/GatewayService.svc";
        }
    }
}