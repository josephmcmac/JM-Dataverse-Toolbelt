#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;

#endregion

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