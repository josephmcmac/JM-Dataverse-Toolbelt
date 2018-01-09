#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using System;
using System.IO;
using System.Linq;

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
            UserInterface = new ConsoleUserInterface(true);
        }

        public IUserInterface UserInterface { get; set; }

        private object _lockObject = new object();
        public override void LoadToUi(ViewModelBase viewModel)
        {
            if(viewModel is ObjectEntryViewModel)
            {
                var oeVm = (ObjectEntryViewModel)viewModel;
                UserInterface.LogMessage(string.Format("Loading {0}", oeVm.RecordType));
                oeVm.LoadFormSections();
                UserInterface.LogMessage(string.Format("Validating {0}", oeVm.RecordType));
                var validate = oeVm.Validate();
                if(!validate)
                {
                    throw new Exception(string.Format("The {0} Object Could Not Be Validated For Processing: {1}", oeVm.GetObject().GetType().Name, oeVm.GetValidationSummary()));
                }
                UserInterface.LogMessage("Validation Complete");
                oeVm.OnSave();
            }

            if (viewModel is CompletionScreenViewModel)
            {
                var completion = (CompletionScreenViewModel)viewModel;
                var completionObject = completion.CompletionDetails?.GetObject() as IProcessCompletion;
                if(completionObject == null)
                {
                    throw new Exception($"The Process Failed To Complete: {completion.CompletionHeadingText}");
                }
                else
                {
                    UserInterface.LogMessage("Processing Completion");
                    if (!completionObject.Success)
                    {
                        throw completionObject.Exception;
                    }
                    else
                    {
                        var errors = completionObject.GetResponseItemsWithError();
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
            if (viewModel is ProgressControlViewModel)
            {
                var progressVm = (ProgressControlViewModel)viewModel;
                var lastPercentProgress = 0;
                var lastMessage = "";
                progressVm.PropertyChanged += (s, e) => {
                    if (e.PropertyName == nameof(ProgressControlViewModel.FractionCompleted))
                    {
                        lock (_lockObject)
                        {
                            var percentProgress = (progressVm.FractionCompleted / 1) * 100;
                            if (percentProgress == 0)
                                lastPercentProgress = 0;
                            while (lastPercentProgress < percentProgress)
                            {
                                lastPercentProgress++;
                                if (lastPercentProgress % 10 == 0)
                                    System.Console.WriteLine(lastPercentProgress + "%");
                            }
                        }
                    }
                    if (e.PropertyName == nameof(ProgressControlViewModel.Message))
                    {
                        lock (_lockObject)
                        {
                            if(progressVm.Message != lastMessage)
                            {
                                lastMessage = progressVm.Message;
                                System.Console.WriteLine(lastMessage);
                            }
                        }
                    }
                };
            }
        }
    }
}