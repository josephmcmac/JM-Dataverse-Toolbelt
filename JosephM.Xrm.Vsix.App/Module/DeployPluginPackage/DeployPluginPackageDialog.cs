using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.App.Extensions;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployPluginPackage
{
    [RequiresConnection]
    public class DeployPluginPackageDialog : DialogViewModel
    {
        public XrmRecordService Service { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        IVisualStudioService VisualStudioService { get; set; }

        public DeployPluginPackageDialog(IDialogController dialogController, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings)
            : base(dialogController)
        {
            VisualStudioService = visualStudioService;
            Service = xrmRecordService;
            PackageSettings = packageSettings;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            var verify = Service.VerifyConnection();
            if (!verify.IsValid)
                throw new Exception($"Update plugin package cannot proceed due to a connection error\n\n{string.Join(",", verify.InvalidReasons)}");
            LoadingViewModel.IsLoading = true;
            var response = new DeployPluginPackageResponse();
            CompletionItem = response;
            try
            {
                var selectedProjectName = VisualStudioService.GetSelectedProjectName();
                var assemblyFile = VisualStudioService.BuildSelectedProjectAndGetAssemblyName(false);
                if (string.IsNullOrWhiteSpace(assemblyFile))
                {
                    response.CompletionMessage = "Could Not Find Built Assembly. Check The Build Results";
                }
                else
                {
                    var packageId = VisualStudioService.GetSelectedProjectProperty("PackageId");
                    var packageOutputPath = VisualStudioService.GetSelectedProjectProperty("PackageOutputPath");
                    var packageVersion = VisualStudioService.GetSelectedProjectProperty("PackageVersion");
                    if (string.IsNullOrWhiteSpace(packageId))
                    {
                        throw new NullReferenceException("PackageId Property Not Set In Project");
                    }
                    if (string.IsNullOrWhiteSpace(packageOutputPath))
                    {
                        throw new NullReferenceException("PackageOutputPath Property Not Set In Project");
                    }
                    if (string.IsNullOrWhiteSpace(packageVersion))
                    {
                        throw new NullReferenceException("PackageVersion Property Not Set In Project");
                    }

                    var nugetPackageFileName = Path.Combine(VisualStudioService.GetSelectedProjectDirectory(), packageOutputPath, $"{packageId}.{packageVersion}.nupkg");
                    var bytes = File.ReadAllBytes(nugetPackageFileName);
                    var packageContent = Convert.ToBase64String(bytes);

                    var prePackage = Service.GetFirst(Entities.pluginpackage, Fields.pluginpackage_.uniquename, packageId);
                    if (prePackage == null)
                    {
                        var packageRecord = Service.NewRecord(Entities.pluginpackage);
                        packageRecord.SetField(Fields.pluginpackage_.name, packageId, Service);
                        packageRecord.SetField(Fields.pluginpackage_.uniquename, packageId, Service);
                        packageRecord.SetField("version", packageVersion, Service);
                        packageRecord.SetField("content", packageContent, Service);
                        packageRecord.Id = Service.Create(packageRecord);

                        //add plugin assembly to the solution
                        var pluginPackageComponentType = 10029;
                        if (PackageSettings.AddToSolution)
                            Service.AddSolutionComponents(PackageSettings.Solution.Id, pluginPackageComponentType, new[] { packageRecord.Id });

                        response.CompletionMessage = "Package Created";
                    }
                    else
                    {
                        var packageRecord = Service.NewRecord(Entities.pluginpackage);
                        packageRecord.Id = prePackage.Id;
                        if (prePackage.Id != null)
                            packageRecord.SetField(Fields.pluginpackage_.pluginpackageid, prePackage.Id, Service);
                        packageRecord.SetField("content", packageContent, Service);
                        var matchField = Fields.pluginpackage_.pluginpackageid;

                        var packageLoadResponse = Service.LoadIntoCrm(new[] { packageRecord }, matchField);
                        if (packageLoadResponse.Errors.Any())
                        {
                            throw new Exception("Error Updating Plugin Package", packageLoadResponse.Errors.Values.First());
                        }
                        //add plugin assembly to the solution
                        var pluginPackageComponentType = 10029;
                        var itemsToAdd = packageLoadResponse.Created.Union(packageLoadResponse.Updated);
                        if (PackageSettings.AddToSolution)
                            Service.AddSolutionComponents(PackageSettings.Solution.Id, pluginPackageComponentType, itemsToAdd.Select(i => i.Id));

                        response.CompletionMessage = "Package Updated";
                    }
                }
            }
            catch(Exception ex)
            {
                response.CompletionMessage = ex.XrmDisplayString();
            }
            finally
            {
                LoadingViewModel.IsLoading = false;
            }
        }
    }
}