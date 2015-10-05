#region

using System.Windows.Input;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;

#endregion

namespace JosephM.Application.ViewModel.TabArea
{
    /// <summary>
    ///     Base Class For A ViewModel Object Displayed In The Main Tab Area Region Of The Application
    /// </summary>
    public abstract class TabAreaViewModelBase : ViewModelBase, INavigationAware
    {
        protected TabAreaViewModelBase(IApplicationController controller)
            : base(controller)
        {
            LoadingViewModel = new LoadingViewModel(controller);
        }

        public virtual string TabLabel
        {
            get { return GetType().Name.SplitCamelCase(); }
        }

        public ICommand TabCloseCommand
        {
            get { return new DelegateCommand(OnTabClose); }
        }

        protected void OnTabClose()
        {
            if (ConfirmTabClose())
                ApplicationController.Remove(RegionNames.MainTabRegion, this);
        }

        public LoadingViewModel LoadingViewModel { get; set; }

        protected virtual bool ConfirmTabClose()
        {
            return true;
        }

        #region INavigationAware Members

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        #endregion
    }
}