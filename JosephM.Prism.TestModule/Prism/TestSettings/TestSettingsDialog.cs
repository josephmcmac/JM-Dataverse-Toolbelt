using JosephM.Application.Prism.Module.Settings;
using JosephM.Application.ViewModel.Dialog;


namespace JosephM.Prism.TestModule.Prism.TestSettings
{
    public class TestSettingsDialog : AppSettingsDialog<ITestSettings, TestSettings>
    {
        public TestSettingsDialog(IDialogController dialogController)
            : base(dialogController)
        {
        }
    }
}