using JosephM.Application.Modules;
using JosephM.Core.Extentions;

namespace JosephM.Prism.Infrastructure.Module
{
    public class DialogModule<TDialog> : ModuleBase
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