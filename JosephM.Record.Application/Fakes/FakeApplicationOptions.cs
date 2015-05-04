#region

using System.Threading;
using JosephM.Record.Application.ApplicationOptions;

#endregion

namespace JosephM.Record.Application.Fakes
{
    /// <summary>
    ///     Object for access to the main UI thread and adding or removing UI items
    /// </summary>
    public class FakeApplicationOptions : ApplicationOptionsViewModel
    {
        public FakeApplicationOptions()
            : base(new FakeApplicationController())
        {
            AddOption("Fake Option", "Fake Menu", Nothing);
            AddOption("Fake Option 2", "Fake Menu", Nothing);
            AddOption("Fake Option 3", "Fake Menu", Nothing);
            AddSetting("Fake Setting", "Fake Menu", Nothing);
            AddSetting("Fake Setting", "Fake Menu", Nothing);
            AddSetting("Fake Setting", "Fake Menu", Nothing);

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