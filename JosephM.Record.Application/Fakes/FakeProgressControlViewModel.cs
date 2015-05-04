#region

using JosephM.Record.Application.Shared;

#endregion

namespace JosephM.Record.Application.Fakes
{
    public class FakeProgressControlViewModel : ProgressControlViewModel
    {
        public FakeProgressControlViewModel()
            : base(new FakeApplicationController())
        {
            Message = "Fake Message";
            FractionCompleted = 3/(double) 4;
        }
    }
}