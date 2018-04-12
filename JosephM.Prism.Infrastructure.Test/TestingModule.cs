#region

using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Fakes;

#endregion

namespace JosephM.Application.Desktop.Test
{
    public class TestingModule : ModuleBase
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