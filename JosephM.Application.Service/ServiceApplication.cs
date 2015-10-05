using JosephM.Application.Application;

namespace JosephM.Prism.Infrastructure.Prism
{
    public abstract class ServiceApplication : ApplicationBase
    {
        protected ServiceApplication(string applicationName)
            : base(new ServiceApplicationController(applicationName))
        {
        }

        public abstract void Run();
    }
}
