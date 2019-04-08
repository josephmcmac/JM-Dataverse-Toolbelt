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
            AddOption("Main", "Fake Option", Nothing);
            AddOption("Main", "Fake Option 2", Nothing);
            AddOption("Main", "Fake Option 3", Nothing);
            AddOption("Main", "Fake Setting", Nothing);
            AddOption("Main", "Fake Setting", Nothing);
            AddOption("Main", "Fake Setting", Nothing);

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