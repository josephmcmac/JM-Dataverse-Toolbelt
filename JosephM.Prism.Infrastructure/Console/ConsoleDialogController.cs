#region

using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.HTML;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Extentions = JosephM.Application.ViewModel.Extentions.Extentions;
using MessageBox = System.Windows.MessageBox;
using JosephM.Application.ViewModel;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Service;
using System.Linq;
using JosephM.Core.Utility;

#endregion

namespace JosephM.Prism.Infrastructure.Console
{
    /// <summary>
    ///     Implementation Of IApplicationController For The Prism Application
    /// </summary>
    public class ConsoleDialogController : DialogController
    {
        public ConsoleDialogController(IApplicationController applicationController)
            : base(applicationController)
        {
        }

        public override void LoadToUi(ViewModelBase viewModel)
        {
            //todo need to somehow have the progress control log into the console

            if(viewModel is ObjectEntryViewModel)
            {
                var oeVm = (ObjectEntryViewModel)viewModel;
                oeVm.LoadFormSections();
                var validate = oeVm.Validate();
                if(!validate)
                {
                    throw new Exception(string.Format("The {0} Object Could Not Be Validated For Processing: {1}", oeVm.GetObject().GetType().Name, oeVm.GetValidationSummary()));
                }
                oeVm.OnSave();
            }

            if (viewModel is CompletionScreenViewModel)
            {
                var completion = (CompletionScreenViewModel)viewModel;
                var completionObject = completion.CompletionDetails?.GetObject();
                if(completionObject is IProcessCompletion)
                {
                    var processCompletion = (IProcessCompletion)completionObject;
                    if (!processCompletion.Success)
                    {
                        throw processCompletion.Exception;
                    }
                    else
                    {
                        var errors = processCompletion.GetResponseItemsWithError();
                        if (errors.Any())
                        {
                            var folder = ApplicationController.LogPath;
                            var fileName = string.Format("{0}Errors_{1}.csv", errors.First().GetType().Name, DateTime.Now.ToFileTime());
                            CsvUtility.CreateCsv(ApplicationController.LogPath, fileName, errors);
                            throw new Exception(string.Format("Errors occured during the process and have been output into {0}", Path.Combine(folder, fileName)));
                        }
                    }
                }
            }
        }
    }
}