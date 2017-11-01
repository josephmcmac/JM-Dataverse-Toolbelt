using JosephM.Application.Modules;
using JosephM.Core.Extentions;

namespace JosephM.Prism.Infrastructure.Module
{
    public class DialogModule<TDialog> : ActionModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<TDialog>();
        }

        public override void InitialiseModule()
        {
            AddOption(MenuGroup, MainOperationName, DialogCommand);
        }

        protected virtual string MainOperationName
        {
            get { return (typeof(TDialog)).GetDisplayName(); }
        }

        public override void DialogCommand()
        {
            NavigateTo<TDialog>();
        }

        public virtual string MenuGroup => MainOperationName;
    }
}