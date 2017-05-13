using System.Windows;

namespace JosephM.XRM.VSIX.Commands.ClearCache
{
    internal sealed class ClearCacheCommand : CommandBase<ClearCacheCommand>
    {
        public override int CommandId
        {
            get { return 0x0109; }
        }

        public override void DoDialog()
        {
            var xrmRecordService = GetXrmRecordService();
            xrmRecordService.ClearCache();
            MessageBox.Show("Cache Cleared");
        }
    }
}
