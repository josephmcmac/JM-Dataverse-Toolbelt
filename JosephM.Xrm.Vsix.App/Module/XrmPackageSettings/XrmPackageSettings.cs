﻿using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Attributes;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Crud.Validations;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    [Instruction("These settings are specific for this solution and will be stored in a file in a 'SolutionItems' folder")]
    [Group(Sections.Solution, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.ConnectionInstances, Group.DisplayLayoutEnum.VerticalCentered, order: 30)]
    [Group(Sections.ProjectSolutionOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40)]
    [Group(Sections.OtherSolutionOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 50, displayLabel: false)]
    [Group(Sections.SchemaGenerationFilter, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 60)]
    public class XrmPackageSettings : ISavedXrmConnections
    {
        [DisplayOrder(10)]
        [MyDescription("Select if deployed items will be added to a solution")]
        [Group(Sections.Solution)]
        public bool AddToSolution { get; set; }

        [DisplayOrder(20)]
        [MyDescription("Select the solution to add deployed inems into")]
        [Group(Sections.Solution)]
        [RequiredProperty]
        [ReferencedType(Entities.solution)]
        [UsePicklist(Fields.solution_.uniquename)]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupCondition(Fields.solution_.uniquename, ConditionType.NotEqual, "default")]
        [PropertyInContextByPropertyValue(nameof(AddToSolution), true)]
        public Lookup Solution { get; set; }

        [DisplayOrder(110)]
        [MyDescription("String used to prefix the name of classes and objects generated by project and item templates")]
        [Group(Sections.Solution)]
        [RequiredProperty]
        [ClassPrefixValidation]
        [DisplayName("Class Prefix")]
        public string SolutionObjectPrefix { get; set; }

        [DisplayOrder(120)]
        [MyDescription("String used to prefix the name of web resource files generated by project and item templates")]
        [Group(Sections.Solution)]
        [RequiredProperty]
        [PrefixValidation]
        [DisplayName("Customisation Prefix")]
        public string SolutionDynamicsCrmPrefix { get; set; }

        [Hidden]
        public string SolutionObjectInstancePrefix
        {
            get
            {
                if (string.IsNullOrEmpty(SolutionObjectPrefix))
                    return SolutionObjectPrefix;
                if (char.IsLower(SolutionObjectPrefix[0]))
                    return "" + char.ToUpper(SolutionObjectPrefix[0]) + (SolutionObjectPrefix.Length == 1 ? "" : SolutionObjectPrefix.Substring(1));
                if (char.IsUpper(SolutionObjectPrefix[0]))
                    return "" + char.ToLower(SolutionObjectPrefix[0]) + (SolutionObjectPrefix.Length == 1 ? "" : SolutionObjectPrefix.Substring(1));
                return SolutionObjectPrefix;
            }
        }

        [DisplayOrder(310)]
        [Group(Sections.ProjectSolutionOptions)]
        [MyDescription("Specify if you want to only display plugin assembly options for specific projects")]
        public IEnumerable<PluginProject> PluginProjects { get; set; }

        [DisplayOrder(320)]
        [Group(Sections.OtherSolutionOptions)]
        [MyDescription("Specify if you want to only display deploy web resource options for specific projects")]
        public IEnumerable<WebResourceProject> WebResourceProjects { get; set; }

        [DisplayOrder(330)]
        [Group(Sections.OtherSolutionOptions)]
        [MyDescription("Specify if you want to only display plugin package options for specific projects")]
        public IEnumerable<PluginPackageProject> PluginPackageProjects { get; set; }

        [DisplayName("Type Filter for Schema Generation (optional)")]
        [DisplayOrder(350)]
        [Group(Sections.SchemaGenerationFilter)]
        [MyDescription("Specify if you want to limit types for the Refresh Schema feature")]
        public IEnumerable<RecordTypeSetting> TypesForSchemaGeneration { get; set; }

        [DisplayOrder(210)]
        [FormEntry]
        [Group(Sections.ConnectionInstances)]
        public IEnumerable<SavedXrmRecordConfiguration> Connections { get; set; }

        public bool AddIlMergePathForProject(string projectName)
        {
            return PluginProjects != null
                && PluginProjects.Any(pp => pp.ProjectName?.ToLower() == projectName?.ToLower())
                && PluginProjects.First(pp => pp.ProjectName?.ToLower() == projectName?.ToLower()).UsesILMergeMsBuildTask;
        }

        public bool IsPluginPackageProject(string projectName)
        {
            return PluginPackageProjects != null
                && PluginPackageProjects.Any(pp => pp.ProjectName?.ToLower() == projectName?.ToLower());
        }

        [DoNotAllowGridOpen]
        public class PluginProject
        {
            public PluginProject()
            {

            }

            public PluginProject(string projectName)
            {
                ProjectName = projectName;
            }

            [GridWidth(330)]
            [RequiredProperty]
            public string ProjectName { get; set; }

            [GridWidth(160)]
            [RequiredProperty]
            [MyDescription("Prevent deploy assembly button from displaying in right-click menu")]
            public bool DisableAssemblyDeploy { get; set; }

            [GridWidth(170)]
            [RequiredProperty]
            [MyDescription("Prevent update assembly button from displaying in right-click menu")]
            public bool DisableAssemblyUpdate { get; set; }

            [GridWidth(180)]
            [RequiredProperty]
            [DisplayName("Uses ILMerge-MSBuild-Task")]
            [MyDescription("ILMerge-MSBuild-Task output will be used for this project if this flag is checked, but is only supported for the standard ILMerge output path and where an ILMergeConfig.json file is used to limit the dlls included (rather than the default CopyAll)")]
            public bool UsesILMergeMsBuildTask { get; set; }
        }

        [DoNotAllowGridOpen]
        public class PluginPackageProject
        {
            public PluginPackageProject()
            {

            }

            public PluginPackageProject(string projectName)
            {
                ProjectName = projectName;
            }

            [GridWidth(330)]
            [RequiredProperty]
            public string ProjectName { get; set; }
        }

        [DoNotAllowGridOpen]
        public class WebResourceProject
        {
            public WebResourceProject()
            {

            }

            public WebResourceProject(string projectName)
            {
                ProjectName = projectName;
            }

            [GridWidth(330)]
            [RequiredProperty]
            public string ProjectName { get; set; }
        }

        private static class Sections
        {
            public const string Solution = "Active Dev Solution";
            public const string ConnectionInstances = "Instance Connections";
            public const string ProjectSolutionOptions = "Project type options - use these to filter menu options within projects. If you want to hide menu options for all projects just insert a fake project name";
            public const string OtherSolutionOptions = "Other Solution Options";
            public const string SchemaGenerationFilter = "Type Filter for Schema Generation (optional)";
        }
    }
}