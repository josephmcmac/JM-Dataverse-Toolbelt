using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;


namespace JosephM.TestModule.TestSettings
{
    public class TestSettingsDialog : AppSettingsDialog<ITestSettings, TestSettings>
    {
        public TestSettingsDialog(IDialogController dialogController)
            : base(dialogController)
        {
        }
    }
}