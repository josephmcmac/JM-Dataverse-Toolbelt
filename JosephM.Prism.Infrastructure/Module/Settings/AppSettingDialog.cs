using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.ObjectMapping;
using JosephM.Record.IService;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Settings
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
        protected AppSettingsDialog(IDialogController dialogController,  IRecordService lookupService = null, string saveButtonLabel = null)
            //map the existing config to the new record
            : this(dialogController, lookupService, new InterfaceMapperFor<TSettingsInterface, TSettingsObject>().Map(dialogController.ApplicationController.ResolveType<TSettingsInterface>()), saveButtonLabel: saveButtonLabel)
        {
        }

        protected AppSettingsDialog(IDialogController dialogController,
            IRecordService lookupService, TSettingsObject objectToEnter, string saveButtonLabel = null)
            : base(dialogController)
        {
            LookupService = lookupService;
            SettingsObject = objectToEnter;
            var configEntryDialog = new ObjectEntryDialog(SettingsObject, this, ApplicationController, lookupService,
                null, OnSave, null, saveButtonLabel: saveButtonLabel ?? "Save");
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        protected AppSettingsDialog(DialogViewModel parentDialog, IRecordService lookupService, TSettingsObject objectToEnter)
            : base(parentDialog)
        {
            SettingsObject = objectToEnter;
            var configEntryDialog = new ObjectEntryDialog(SettingsObject, this, ApplicationController, lookupService,
                null, OnSave, null, saveButtonLabel: "Next");

            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }


        protected virtual void OnSave()
        {

        }

        protected TSettingsObject SettingsObject { get; set; }
        public IRecordService LookupService { get; }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            ApplicationController.ResolveType<ISettingsManager>().SaveSettingsObject(SettingsObject);
            ApplicationController.RegisterInstance<TSettingsInterface>(SettingsObject);
            if (CompletionMessage == null)
                CompletionMessage = "The Settings Have Been Saved";

            if (OverideCompletionScreenMethod == null && !HasParentDialog)
            {
                //okay in this case let set the dialog to
                //keep appending the settings entry to itself when completed
                OverideCompletionScreenMethod = ()
                    =>
                {
                    //append new dialog for the setting entry and
                    //trigger this dialog to start it
                    var configEntryDialog = new ObjectEntryDialog(SettingsObject, this, ApplicationController, LookupService,
                        null, OnSave, null, saveButtonLabel: "Next", initialMessage: "Changes Have Been Saved");
                    SubDialogs = SubDialogs.Union(new[] { configEntryDialog }).ToArray();
                    DialogCompletionCommit = false;
                    StartNextAction();
                };
            }
        }
    }
}