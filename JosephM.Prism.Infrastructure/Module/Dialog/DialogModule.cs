using JosephM.Application.Modules;
using JosephM.Core.Extentions;

namespace JosephM.Application.Prism.Module.Dialog
{
    public abstract class DialogModule<TDialog> : OptionActionModule
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<TDialog>();
        }

        public override string MainOperationName
        {
            get { return (typeof(TDialog)).GetDisplayName(); }
        }

        public override void DialogCommand()
        {
            NavigateTo<TDialog>();
        }

        public override string MenuGroup => MainOperationName;
    }
}