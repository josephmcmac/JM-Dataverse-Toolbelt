using Prism.Regions;
using System;

namespace JosephM.Application.ViewModel.Fakes
{
    class FakeRegionNavigationService : IRegionNavigationService
    {
        public void RequestNavigate(Uri target, Action<NavigationResult> navigationCallback)
        {
            throw new NotImplementedException();
        }

        public void RequestNavigate(Uri target, Action<NavigationResult> navigationCallback, NavigationParameters navigationParameters)
        {
            throw new NotImplementedException();
        }

        public IRegion Region { get; set; }
        public IRegionNavigationJournal Journal { get; private set; }
        public event EventHandler<RegionNavigationEventArgs> Navigating;
        public event EventHandler<RegionNavigationEventArgs> Navigated;
        public event EventHandler<RegionNavigationFailedEventArgs> NavigationFailed;
    }
}
