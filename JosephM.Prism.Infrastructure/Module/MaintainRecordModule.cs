using JosephM.Application.Modules;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;

namespace JosephM.Prism.Infrastructure.Module
{
    public abstract class MaintainRecordModule<TMaintainRecordViewModel> : ModuleBase
        where TMaintainRecordViewModel : MaintainViewModel
    {
        protected abstract string Type { get; }
        protected abstract string IdName { get; }
        protected abstract string Id { get; }

        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<TMaintainRecordViewModel>();
            //RegisterType<TFormService>();
        }

        public override void InitialiseModule()
        {
            AddOption(typeof(TMaintainRecordViewModel).GetDisplayName(), MaintainRecordCommand);
        }

        private void MaintainRecordCommand()
        {
            ApplicationController.OpenRecord(Type, IdName, Id, typeof(TMaintainRecordViewModel));
        }
    }
}