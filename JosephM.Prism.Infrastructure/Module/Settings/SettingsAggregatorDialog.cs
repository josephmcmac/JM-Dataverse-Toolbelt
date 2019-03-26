using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.ObjectMapping;
using System;
using System.Collections.Generic;

namespace JosephM.Application.Desktop.Module.Settings
{
    public class SettingsAggregatorDialog
        : DialogViewModel
    {
        public override string TabLabel => "Settings";

        public SettingsAggregatorDialog(IDialogController controller) : base(controller)
        {
        }

        protected override void CompleteDialogExtention()
        {
            var aggregatedSettings = ApplicationController.ResolveType<SettingsAggregator>();

            var viewModels = new List<RecordEntryFormViewModel>();
            foreach(var type in aggregatedSettings.SettingTypes)
            {
                var mapper = new ClassSelfMapper();
                var instance = mapper.Map(ApplicationController.ResolveType(type));

                var viewModel = new ObjectEntryViewModel(null, null, instance, FormController.CreateForObject(instance, ApplicationController, null));
                viewModel.DisplayRightEdgeButtons = false;
                viewModel.OnSave = () =>
                {
                    ApplicationController.ResolveType<ISettingsManager>().SaveSettingsObject(instance);
                    ApplicationController.RegisterInstance(instance);
                    viewModel.ValidationPrompt = "The Settings Have Been Saved";
                };
                viewModels.Add(viewModel);
            }
            var mainViewModel = new RecordEntryAggregatorViewModel(viewModels, ApplicationController);
            Controller.LoadToUi(mainViewModel);
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }
    }
}