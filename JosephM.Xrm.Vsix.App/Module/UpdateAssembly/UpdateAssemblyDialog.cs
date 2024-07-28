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
using System.Reflection;

namespace JosephM.Xrm.Vsix.Module.UpdateAssembly
{
    [RequiresConnection]
    public class UpdateAssemblyDialog : DialogViewModel
    {
        public XrmRecordService Service { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        IVisualStudioService VisualStudioService { get; set; }

        public UpdateAssemblyDialog(IDialogController dialogController, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings)
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
                throw new Exception($"Update assembly cannot proceed due to a connection error\n\n{string.Join(",", verify.InvalidReasons)}");
            LoadingViewModel.IsLoading = true;
            var response = new UpdateAssemblyResponse();
            CompletionItem = response;
            try
            {
                var selectedProjectName = VisualStudioService.GetSelectedProjectName();
                var addIlMergePath = PackageSettings.AddIlMergePathForProject(selectedProjectName);
                var isPluginPackageProject = PackageSettings.IsPluginPackageProject(selectedProjectName);
                var assemblyFile = VisualStudioService.BuildSelectedProjectAndGetAssemblyName(addIlMergePath);
                if (string.IsNullOrWhiteSpace(assemblyFile))
                {
                    response.CompletionMessage = "Could Not Find Built Assembly. Check The Build Results";
                }
                else
                {
                    if (!isPluginPackageProject)
                    {
                        var fileInfo = new FileInfo(assemblyFile);
                        var assemblyName = fileInfo.Name.Substring(0,
                            fileInfo.Name.LastIndexOf(fileInfo.Extension, StringComparison.Ordinal));

                        var bytes = File.ReadAllBytes(assemblyFile);
                        var assemblyContent = Convert.ToBase64String(bytes);

                        var preAssembly = Service.GetFirst(Entities.pluginassembly, Fields.pluginassembly_.name, assemblyName);
                        if (preAssembly == null)
                        {
                            response.CompletionMessage = "There is no plugin assembly deployed in the dynamics instance with a matching name. Try the deploy assembly option to deploy a new plugin assembly, or rename the assembly to match an existing assembly deployed to the instance";
                        }
                        else
                        {
                            //okay first create/update the plugin assembly
                            var assemblyRecord = Service.NewRecord(Entities.pluginassembly);
                            assemblyRecord.Id = preAssembly.Id;
                            if (preAssembly.Id != null)
                                assemblyRecord.SetField(Fields.pluginassembly_.pluginassemblyid, preAssembly.Id, Service);
                            assemblyRecord.SetField(Fields.pluginassembly_.content, assemblyContent, Service);
                            var matchField = Fields.pluginassembly_.pluginassemblyid;

                            var assemblyLoadResponse = Service.LoadIntoCrm(new[] { assemblyRecord }, matchField);
                            if (assemblyLoadResponse.Errors.Any())
                            {
                                throw new Exception("Error Updating Assembly", assemblyLoadResponse.Errors.Values.First());
                            }
                            //add plugin assembly to the solution
                            var componentType = OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly;
                            var itemsToAdd = assemblyLoadResponse.Created.Union(assemblyLoadResponse.Updated);
                            if (PackageSettings.AddToSolution)
                                Service.AddSolutionComponents(PackageSettings.Solution.Id, componentType, itemsToAdd.Select(i => i.Id));

                            response.CompletionMessage = "Assembly Updated";
                        }
                    }
                    else
                    {
                        var packageId = VisualStudioService.GetSelectedProjectProperty("PackageId");
                        var packageOutputPath = VisualStudioService.GetSelectedProjectProperty("PackageOutputPath");
                        var packageVersion = VisualStudioService.GetSelectedProjectProperty("PackageVersion");
                        if(string.IsNullOrWhiteSpace(packageId))
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
                        var prePackage = Service.GetFirst(Entities.pluginpackage, Fields.pluginpackage_.uniquename, packageId);
                        if (prePackage == null)
                        {
                            response.CompletionMessage = "There is no plugin package deployed in the dynamics instance with a matching uniquename. Try the deploy option to deploy a new plugin package, or rename to match an existing plugin package deployed to the instance";
                        }
                        var nugetPackageFileName = Path.Combine(VisualStudioService.GetSelectedProjectDirectory(), packageOutputPath, $"{packageId}.{packageVersion}.nupkg");
                        var bytes = File.ReadAllBytes(nugetPackageFileName);
                        var packageContent = Convert.ToBase64String(bytes);

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
                        var componentType = 10030;
                        var itemsToAdd = packageLoadResponse.Created.Union(packageLoadResponse.Updated);
                        if (PackageSettings.AddToSolution)
                            Service.AddSolutionComponents(PackageSettings.Solution.Id, componentType, itemsToAdd.Select(i => i.Id));

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