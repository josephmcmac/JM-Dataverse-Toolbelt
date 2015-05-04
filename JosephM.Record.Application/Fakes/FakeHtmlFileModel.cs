#region

using System;
using JosephM.Record.Application.HTML;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Metadata;

#endregion

namespace JosephM.Record.Application.Fakes
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