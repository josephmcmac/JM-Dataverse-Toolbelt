#region

using JosephM.ObjectMapping;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Dialog;
using JosephM.Record.IService;

#endregion

namespace JosephM.Prism.Infrastructure.Dialog
{
    /// <summary>
    ///     Base Class Implementing A Dialog To Enter And Save User Defined Settings
    ///     Extend For Setting Types Specific To Application Modules
    /// </summary>
    /// <typeparam name="TSettingsInterface">Settings Interface Type</typeparam>
    /// <typeparam name="TSettingsObject">Settings Type Required To Have Get/Set Properties Implementing The Settings Interface</typeparam>
    public abstract class AppSettingsDialog<TSettingsInterface, TSettingsObject>
        : DialogViewModel
        where TSettingsObject : TSettingsInterface, new()
    {
        protected AppSettingsDialog(IDialogController dialogController, PrismContainer container)
            : this(dialogController, container, null)
        {
        }

        protected AppSettingsDialog(IDialogController dialogController, PrismContainer container,
            IRecordService lookupService)
            : base(dialogController)
        {
            Container = container;
            //map the existing config to the new record
            var settingsInterface = Container.Resolve<TSettingsInterface>();
            var configMapper = new InterfaceMapperFor<TSettingsInterface, TSettingsObject>();
            SettingsObject = configMapper.Map(settingsInterface);
            var configEntryDialog = new ObjectEntryDialog(SettingsObject, this, ApplicationController, lookupService,
                null);

            SubDialogs = new DialogViewModel[] {configEntryDialog};
        }

        protected PrismContainer Container { get; set; }

        protected TSettingsObject SettingsObject { get; set; }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            DoWhileLoading("Verifying Your Data...", () =>
            {
                if (Validate())
                {
                    Container.Resolve<PrismSettingsManager>().SaveSettingsObject(SettingsObject);
                    Container.RegisterInstance((TSettingsInterface) SettingsObject);
                    if (CompletionMessage == null)
                        CompletionMessage = "The Settings Have Been Saved";
                }
            });
        }

        protected virtual bool Validate()
        {
            return true;
        }
    }
}