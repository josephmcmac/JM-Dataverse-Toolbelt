using System.Diagnostics;

namespace JosephM.XRM.VSIX.Commands.OpenWeb
{
    internal sealed class OpenCrmWebCommand : CommandBase<OpenCrmWebCommand>
    {
        public override int CommandId
        {
            get { return 0x010B; }
        }

        public override void DoDialog()
        {
            var xrmRecordService = GetXrmRecordService();
            var url = xrmRecordService.WebUrl;
            Process.Start(url);
        }
    }
}
