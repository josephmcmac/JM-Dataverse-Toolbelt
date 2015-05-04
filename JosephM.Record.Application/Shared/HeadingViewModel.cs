using JosephM.Record.Application.Controller;

namespace JosephM.Record.Application.Shared
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