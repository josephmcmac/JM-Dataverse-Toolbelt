using System.Diagnostics;

namespace JosephM.XRM.VSIX.Commands.OpenWeb
{
    internal sealed class OpenCrmAdvancedFindCommand : CommandBase<OpenCrmAdvancedFindCommand>
    {
        public override int CommandId
        {
            get { return 0x010D; }
        }

        public override void DoDialog()
        {
            var xrmRecordService = GetXrmRecordService();
            var url = xrmRecordService.WebUrl;
            var solutionUrl = string.Format("{0}{1}main.aspx?pagetype=advancedfind", url, url.EndsWith("/") ? "" : "/");
            Process.Start(solutionUrl);
        }
    }
}
