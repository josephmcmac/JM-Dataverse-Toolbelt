#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.ObjectMapping;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Record.IService;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace JosephM.Prism.Infrastructure.Dialog
{
    public class SavedRequestDialog
        : DialogViewModel
    {
        public SavedRequestDialog(IDialogController dialogController)
            : base(dialogController)
        {
        }

        protected override void LoadDialogExtention()
        {
            var objectTypeMaps = new Dictionary<string, Type>()
            {
                { "SavedRequests", RequestType }
            };

            var configEntryDialog = new ObjectGetEntryDialog(() => SettingsObject, this, ApplicationController, saveMethod: null, objectTypeMaps: objectTypeMaps);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
            StartNextAction();
        }

        private SavedSettings _settingsObject;
        public SavedSettings SettingsObject
        {
            get
            {
                if (_settingsObject == null)
                {
                    var resolve = ApplicationController.ResolveType<PrismSettingsManager>().Resolve<SavedSettings>(RequestType);
                    if(!resolve.SavedRequests.Any())
                    {
                        ApplicationController.UserMessage(string.Format("There are no saved {0} records", RequestType.GetDisplayName()));
                        OnCancel();
                    }
                    foreach(var item in resolve.SavedRequests)
                    {
                        if (item is ServiceRequestBase)
                            ((ServiceRequestBase)item).DisplaySavedSettingFields = true;
                    }
                    _settingsObject = resolve;
                }
                return _settingsObject;
            }
        }

        public Type RequestType { get; private set; }

        //protected TSettingsObject SettingsObject { get; set; }

        protected override void CompleteDialogExtention()
        {
            ApplicationController.ResolveType<PrismSettingsManager>().SaveSettingsObject(SettingsObject, RequestType);
            //ApplicationController.RegisterInstance<TSettingsInterface>(SettingsObject);
            if (CompletionMessage == null)
                CompletionMessage = "The Settings Have Been Saved";
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            var navigationProvider = new PrismNavigationProvider(navigationContext);
            if (string.IsNullOrWhiteSpace(navigationProvider.GetValue("Type")))
                throw new NullReferenceException("Error missing type argument in query");

            var typename = navigationProvider.GetValue("Type");
            RequestType = Type.GetType(typename);
        }
    }
}