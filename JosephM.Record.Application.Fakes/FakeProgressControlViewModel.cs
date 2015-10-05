#region

using JosephM.Application.ViewModel.Shared;

#endregion

namespace JosephM.Application.ViewModel.Fakes
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