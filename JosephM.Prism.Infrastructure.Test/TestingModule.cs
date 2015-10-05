#region

using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Record.Application.Fakes;

#endregion

namespace JosephM.Prism.Infrastructure.Test
{
    public class TestingModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterInstance(FakeRecordService.Get());
        }

        public override void InitialiseModule()
        {
        }
    }
}