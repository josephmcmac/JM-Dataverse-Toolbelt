using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;

namespace JosephM.Prism.Infrastructure.Module
{
    public abstract class MaintainRecordModule<TFormService, TMaintainRecordViewModel> : PrismModuleBase
        where TFormService : FormServiceBase
        where TMaintainRecordViewModel : MaintainViewModel
    {
        protected abstract string Type { get; }
        protected abstract string IdName { get; }
        protected abstract string Id { get; }

        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<TMaintainRecordViewModel>();
            RegisterType<TFormService>();
        }

        public override void InitialiseModule()
        {
            ApplicationOptions.AddOption(typeof (TMaintainRecordViewModel).GetDisplayName(), MenuNames.Crm,
                MaintainRecordCommand);
        }

        private void MaintainRecordCommand()
        {
            ApplicationController.OpenRecord(Type, IdName, Id, typeof (TMaintainRecordViewModel));
        }
    }
}