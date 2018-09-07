#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace JosephM.Application.ViewModel.TabArea
{
    /// <summary>
    ///     Base Class For A ViewModel Object Displayed In The Main Tab Area Region Of The Application
    /// </summary>
    public abstract class TabAreaViewModelBase : ViewModelBase
    {
        protected TabAreaViewModelBase(IApplicationController controller)
            : base(controller)
        {
            LoadingViewModel = new LoadingViewModel(controller);
            CloseOtherTabsCommand = new MyCommand(() =>
            {
                foreach(var item in ApplicationController.GetObjects().ToArray())
                {
                    if (item != this)
                    {
                        if(item is TabAreaViewModelBase)
                        {
                            ((TabAreaViewModelBase)item).TabCloseCommand.Execute();
                        }
                        else
                            ApplicationController.Remove(item);
                    }
                }

            });
            CloseAllTabsCommand = new MyCommand(() =>
            {
                foreach (var item in ApplicationController.GetObjects().ToArray())
                {
                    if (item is TabAreaViewModelBase)
                    {
                        ((TabAreaViewModelBase)item).TabCloseCommand.Execute();
                    }
                    else
                        ApplicationController.Remove(item);
                }
            });
        }

        public virtual string TabLabel
        {
            get { return GetType().Name.SplitCamelCase(); }
        }

        public MyCommand TabCloseCommand
        {
            get { return new MyCommand(OnTabClose); }
        }

        protected void OnTabClose()
        {
            if (ConfirmTabClose())
                ApplicationController.Remove(this);
        }

        public LoadingViewModel LoadingViewModel { get; set; }

        protected virtual bool ConfirmTabClose()
        {
            return true;
        }

        public virtual void LoadChildForm(TabAreaViewModelBase viewModel)
        {
            ApplicationController.DoOnMainThread(() =>
            {
                ChildForms.Add(viewModel);
                OnPropertyChanged("MainFormInContext");
            });
        }

        public virtual void ClearChildForm()
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

        public MyCommand CloseOtherTabsCommand { get; set; }

        public MyCommand CloseAllTabsCommand { get; set; }

        public bool AreMultipleTabs
        {
            get { return ApplicationController.GetObjects().Count() > 1; }
        }

        public bool IsActiveTabItem
        {
            get
            {
                return ApplicationController.ActiveTabItem == this;
            }
        }
    }
}