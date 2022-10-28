using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Record.Extentions;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using System;
using JosephM.Record.IService;
using JosephM.Application.ViewModel.Shared;

namespace JosephM.Deployment.SolutionsImport
{
    public static class ModuleExtensions
    {
        public static void AddSolutionDetailsFormEvent(this ModuleBase solutionsImportModule, Type requestType, string sourceSolutionPropertyName, string importingManagedPropertyName, string sourceVersionPropertyName, Func<RecordEntryViewModelBase, SolutionZipMetadata> getSourceSolutionMetadata = null)
        {
            Func<RecordEntryViewModelBase, LoadingViewModel> getLoadingViewModel = (revm) =>
                {
                    return revm.ParentForm == null
                    ? revm.LoadingViewModel
                    : revm.ParentForm.LoadingViewModel;
                };

            Action<RecordEntryViewModelBase> loadSolutionDetails = (revm) =>
            {
                if (getSourceSolutionMetadata != null)
                {
                    var solutionMetadata = getSourceSolutionMetadata(revm);
                    revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.IsManaged)).Value = solutionMetadata?.Managed;
                    revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.Version)).Value = solutionMetadata?.Version;
                    revm.GetRecord().SetField(nameof(ILoadSolutionForImport.UniqueName), solutionMetadata?.UniqueName, revm.RecordService);
                    revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.FriendlyName)).Value = solutionMetadata?.FriendlyName;
                }
            };
            Action<RecordEntryViewModelBase> loadCurrentSolutionVersionDetails = (revm) => {
                var connection = revm.GetRecord().GetField(nameof(ILoadSolutionForImport.TargetConnection)) as IXrmRecordConfiguration;
                var solutionuniqueName = revm.GetRecord().GetStringField(nameof(ILoadSolutionForImport.UniqueName));

                revm.ApplicationController.DoOnAsyncThread(() =>
                {
                    try
                    {
                        IRecord installedSolution = null;
                        if (connection != null && !string.IsNullOrWhiteSpace(solutionuniqueName))
                        {
                            var serviceFactory = solutionsImportModule.ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                            var recordService = new XrmRecordService(connection, serviceFactory);
                            installedSolution = recordService.GetFirst(Entities.solution, Fields.solution_.uniquename, solutionuniqueName);
                        }
                        revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.IsCurrentlyInstalledInTarget)).Value = installedSolution != null;
                        revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.CurrentTargetVersion)).Value = installedSolution?.GetStringField(Fields.solution_.version);
                        revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.CurrentTargetVersionManaged)).Value = installedSolution == null
                            ? null
                            : (bool?)installedSolution.GetBoolField(Fields.solution_.ismanaged);
                    }
                    catch (Exception ex)
                    {
                        solutionsImportModule.ApplicationController.ThrowException(ex);
                    }
                    finally
                    {
                        getLoadingViewModel(revm).IsLoading = false;
                    }
                });
            };
            var changeFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                if(changedField == sourceSolutionPropertyName)
                {
                    try
                    {
                        getLoadingViewModel(revm).IsLoading = true;
                        loadSolutionDetails(revm);
                        loadCurrentSolutionVersionDetails(revm);
                    }
                    catch (Exception)
                    {
                        getLoadingViewModel(revm).IsLoading = false;
                        throw;
                    }
                }
                switch (changedField)
                {
                    case nameof(ILoadSolutionForImport.TargetConnection):
                        {
                            try
                            {
                                getLoadingViewModel(revm).IsLoading = true;
                                loadSolutionDetails(revm);
                                loadCurrentSolutionVersionDetails(revm);
                            }
                            catch (Exception)
                            {
                                getLoadingViewModel(revm).IsLoading = false;
                                throw;
                            }
                            break;
                        }
                }
                if (changedField == importingManagedPropertyName
                    || changedField == sourceVersionPropertyName
                    || changedField == nameof(ILoadSolutionForImport.CurrentTargetVersion))
                {
                    if (revm.GetBooleanFieldFieldViewModel(importingManagedPropertyName).Value == true && revm.GetRecord().GetBoolField(nameof(ILoadSolutionForImport.IsInstallingNewerVersion)))
                    {
                        if (revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.InstallAsUpgrade)).Value != true)
                        {
                            revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.InstallAsUpgrade)).Value = true;
                        }
                    }
                    else
                    {
                        if (revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.InstallAsUpgrade)).Value == true)
                        {
                            revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.InstallAsUpgrade)).Value = false;
                        }
                    }
                }
            });
            solutionsImportModule.AddOnChangeFunction(changeFunction, requestType);
            var formLoadedFunction = new FormLoadedFunction((RecordEntryViewModelBase revm) =>
            {
                getLoadingViewModel(revm).IsLoading = true;
                loadSolutionDetails(revm);
                loadCurrentSolutionVersionDetails(revm);
            });
            solutionsImportModule.AddFormLoadedFunction(formLoadedFunction, requestType);
        }
    }
}
