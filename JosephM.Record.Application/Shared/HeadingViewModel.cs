using JosephM.Application.Application;

namespace JosephM.Application.ViewModel.Shared
{
    public class HeadingViewModel : ViewModelBase
    {
        public HeadingViewModel(string heading, IApplicationController controller)
            : base(controller)
        {
            Heading = heading;
        }

        public string Heading { get; private set; }
    }
}