#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

#endregion

namespace JosephM.Application.ViewModel.TabArea
{
    /// <summary>
    ///     Base Class For A ViewModel Object Displayed In The Main Tab Area Region Of The Application
    /// </summary>
    public abstract class TabAreaViewModelBase : ViewModelBase//, INavigationAware
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

        public void LoadChildForm(TabAreaViewModelBase viewModel)
        {
            ApplicationController.DoOnMainThread(() =>
            {
                ChildForms.Add(viewModel);
                OnPropertyChanged("MainFormInContext");
            });
        }

        internal void ClearChildForm()
        {
            ApplicationController.DoOnMainThread(() =>
            {
                ChildForms.Clear();
                OnPropertyChanged("MainFormInContext");
            });
        }

        private ObservableCollection<TabAreaViewModelBase> _childForms = new ObservableCollection<TabAreaViewModelBase>();

        /// <summary>
        /// DONT USE CLEAR USER ClearChildForm()
        /// </summary>
        public ObservableCollection<TabAreaViewModelBase> ChildForms
        {
            get { return _childForms; }
            set
            {
                _childForms = value;
                OnPropertyChanged(nameof(ChildForms));
                OnPropertyChanged(nameof(MainFormInContext));
            }
        }

        public void ClearChildForms()
        {
            DoOnMainThread(() =>
            {
                ChildForms.Clear();
                OnPropertyChanged(nameof(MainFormInContext));
            });
        }

        public bool MainFormInContext
        {
            get
            {
                return !ChildForms.Any();
            }
        }

        //#region INavigationAware Members

        //public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        //{
        //    return false;
        //}

        //public void OnNavigatedFrom(NavigationContext navigationContext)
        //{
        //}

        //public virtual void OnNavigatedTo(NavigationContext navigationContext)
        //{
        //}

        //#endregion
    }
}