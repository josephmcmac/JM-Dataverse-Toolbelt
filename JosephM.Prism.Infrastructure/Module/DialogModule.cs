using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Wpf.Application;

namespace JosephM.Prism.Infrastructure.Module
{
    public class DialogModule<TDialog> : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<TDialog>();
        }

        public override void InitialiseModule()
        {
            AddOption(MainOperationName, DialogCommand);
        }

        protected virtual string MainOperationName
        {
            get { return (typeof(TDialog)).GetDisplayName(); }
        }

        public void DialogCommand()
        {
            NavigateTo<TDialog>();
        }
    }
}