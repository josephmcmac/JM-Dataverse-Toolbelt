using JosephM.Core.Test;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;

namespace JosephM.Prism.Infrastructure.Test
{
    public class PrismModuleTest<TModule> : CoreTest
        where TModule : PrismModuleBase, new()
    {
        public PrismModuleTest()
        {
            FakePrismApplication.Initialise();
        }

        private FakePrismApplication<TModule> _fakePrismApplication;

        public FakePrismApplication<TModule> FakePrismApplication
        {
            get
            {
                if (_fakePrismApplication == null)
                    _fakePrismApplication = new FakePrismApplication<TModule>();
                return _fakePrismApplication;
            }
        }

        public PrismContainer Container
        {
            get { return FakePrismApplication.PrismContainer; }
        }
    }
}