#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.ObjectMapping;
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
        protected AppSettingsDialog(IDialogController dialogController)
            : this(dialogController, null)
        {
        }

        protected AppSettingsDialog(IDialogController dialogController,
            IRecordService lookupService)
            : base(dialogController)
        {
            //map the existing config to the new record
            var settingsInterface = ApplicationController.ResolveType<TSettingsInterface>();
            var configMapper = new InterfaceMapperFor<TSettingsInterface, TSettingsObject>();
            SettingsObject = configMapper.Map(settingsInterface);



            var configEntryDialog = new ObjectEntryDialog(SettingsObject, this, ApplicationController, lookupService,
                null, OnSave);

            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        protected virtual void OnSave()
        {

        }

        protected TSettingsObject SettingsObject { get; set; }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            ApplicationController.ResolveType<PrismSettingsManager>().SaveSettingsObject(SettingsObject);
            ApplicationController.RegisterInstance<TSettingsInterface>(SettingsObject);
            if (CompletionMessage == null)
                CompletionMessage = "The Settings Have Been Saved";
        }
    }
}