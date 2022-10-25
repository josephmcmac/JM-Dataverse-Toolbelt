﻿using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Record.Extentions;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using System;
using JosephM.Record.IService;

namespace JosephM.Deployment.SolutionsImport
{
    public static class ModuleExtensions
    {
        public static void AddSolutionDetailsFormEvent(this ModuleBase solutionsImportModule, Type requestType, string sourceSolutionPropertyName, Func<RecordEntryViewModelBase, SolutionZipMetadata> getSourceSolutionMetadata = null)
        {
            Action<RecordEntryViewModelBase> loadSolutionDetails = (revm) =>
            {
                if (getSourceSolutionMetadata != null)
                {
                    var solutionMetadata = getSourceSolutionMetadata(revm);
                    revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.IsManaged)).Value = solutionMetadata?.Managed;
                    revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.Version)).Value = solutionMetadata?.Version;
                    revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.UniqueName)).Value = solutionMetadata?.UniqueName;
                    revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.FriendlyName)).Value = solutionMetadata?.FriendlyName;
                }
            };
            Action<RecordEntryViewModelBase> loadCurrentSolutionVersionDetails = (revm) => {
                var connection = revm.GetRecord().GetField(nameof(ILoadSolutionForImport.TargetConnection)) as IXrmRecordConfiguration;
                var solutionuniqueName = revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.UniqueName)).Value;
                var solutionVersion = revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.UniqueName)).Value;

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
                        revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.IsCurrentlyInstalled)).Value = installedSolution != null;
                        revm.GetStringFieldFieldViewModel(nameof(ILoadSolutionForImport.CurrentVersion)).Value = installedSolution?.GetStringField(Fields.solution_.version);
                        revm.GetBooleanFieldFieldViewModel(nameof(ILoadSolutionForImport.CurrentVersionManaged)).Value = installedSolution == null
                            ? null
                            : (bool?)installedSolution.GetBoolField(Fields.solution_.ismanaged);
                    }
                    catch (Exception ex)
                    {
                        solutionsImportModule.ApplicationController.ThrowException(ex);
                    }
                    finally
                    {
                        revm.LoadingViewModel.IsLoading = false;
                    }
                });
            };
            var changeFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                if(changedField == sourceSolutionPropertyName)
                {
                    try
                    {
                        revm.LoadingViewModel.IsLoading = true;
                        loadSolutionDetails(revm);
                        loadCurrentSolutionVersionDetails(revm);
                    }
                    catch (Exception)
                    {
                        revm.LoadingViewModel.IsLoading = false;
                        throw;
                    }
                }
                switch (changedField)
                {
                    case nameof(ILoadSolutionForImport.TargetConnection):
                        {
                            try
                            {
                                revm.LoadingViewModel.IsLoading = true;
                                loadSolutionDetails(revm);
                                loadCurrentSolutionVersionDetails(revm);
                            }
                            catch (Exception)
                            {
                                revm.LoadingViewModel.IsLoading = false;
                                throw;
                            }
                            break;
                        }
                }
            });
            solutionsImportModule.AddOnChangeFunction(changeFunction, requestType);
            var formLoadedFunction = new FormLoadedFunction((RecordEntryViewModelBase revm) =>
            {
                loadSolutionDetails(revm);
            });
            solutionsImportModule.AddFormLoadedFunction(formLoadedFunction, requestType);
        }
    }
}
