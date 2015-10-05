#region

using System.Threading;
using JosephM.Application.Options;
using JosephM.Application.ViewModel.ApplicationOptions;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    /// <summary>
    ///     Object for access to the main UI thread and adding or removing UI items
    /// </summary>
    public class FakeApplicationOptions : ApplicationOptionsViewModel
    {
        public FakeApplicationOptions()
            : base(new FakeApplicationController())
        {
            AddOption("Fake Option", Nothing, ApplicationOptionType.Main);
            AddOption("Fake Option 2", Nothing, ApplicationOptionType.Main);
            AddOption("Fake Option 3", Nothing, ApplicationOptionType.Main);
            AddOption("Fake Setting", Nothing, ApplicationOptionType.Setting);
            AddOption("Fake Setting", Nothing, ApplicationOptionType.Setting);
            AddOption("Fake Setting", Nothing, ApplicationOptionType.Setting);

            new Thread(() =>
            {
                Thread.Sleep(1000);
                OpenSettings = true;
            }).Start();
        }

        public void Nothing()
        {
        }
    }
}