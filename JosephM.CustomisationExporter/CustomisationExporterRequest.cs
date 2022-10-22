using System.Collections.Generic;
using System.Linq;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

namespace JosephM.CustomisationExporter
{
    [Instruction("An Excel file will be generated with sheets for each export option")]
    [AllowSaveAndLoad]
    [DisplayName("Customisation Export")]
    [Group(Sections.Folder, Group.DisplayLayoutEnum.VerticalCentered, order: 10, displayLabel: false)]
    [Group(Sections.TableAndFieldOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, selectAll: true)]
    [Group(Sections.CustomisationOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30, selectAll: true)]
    [Group(Sections.UsersAndRoleOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40, selectAll: true)]
    public class CustomisationExporterRequest : ServiceRequestBase, IValidatableObject
    {
        [GridWidth(300)]
        [DisplayOrder(10)]
        [Group(Sections.Folder)]
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [GridWidth(110)]
        [DisplayOrder(100)]
        [Group(Sections.TableAndFieldOptions)]
        public bool Entities { get; set; }

        [GridWidth(110)]
        [DisplayOrder(110)]
        [Group(Sections.TableAndFieldOptions)]
        public bool Fields { get; set; }

        [GridWidth(110)]
        [DisplayOrder(120)]
        [Group(Sections.TableAndFieldOptions)]
        public bool FieldOptionSets { get; set; }

        [GridWidth(110)]
        [DisplayOrder(130)]
        [Group(Sections.TableAndFieldOptions)]
        public bool SharedOptionSets { get; set; }

        [GridWidth(110)]
        [DisplayOrder(200)]
        [Group(Sections.TableAndFieldOptions)]
        public bool Relationships { get; set; }

        [DisplayName("Duplicate N:N Relationship Sides")]
        [GridWidth(110)]
        [DisplayOrder(210)]
        [Group(Sections.TableAndFieldOptions)]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool DuplicateManyToManyRelationshipSides { get; set; }

        [GridWidth(110)]
        [DisplayOrder(220)]
        [Group(Sections.TableAndFieldOptions)]
        [DisplayName("Include 1:N Relationships")]
        [PropertyInContextByPropertyValue("Relationships", true)]
        public bool IncludeOneToManyRelationships { get; set; }

        [GridWidth(110)]
        [DisplayOrder(180)]
        [Group(Sections.CustomisationOptions)]
        public bool FormsAndDashboards { get; set; }

        [GridWidth(110)]
        [DisplayOrder(175)]
        [Group(Sections.CustomisationOptions)]
        public bool Reports { get; set; }

        [GridWidth(110)]
        [DisplayOrder(140)]
        [Group(Sections.CustomisationOptions)]
        public bool Solutions { get; set; }

        [GridWidth(110)]
        [DisplayOrder(150)]
        [Group(Sections.CustomisationOptions)]
        public bool PluginAssemblies { get; set; }

        [GridWidth(110)]
        [DisplayOrder(151)]
        [Group(Sections.CustomisationOptions)]
        public bool PluginTriggers { get; set; }

        [GridWidth(110)]
        [DisplayOrder(175)]
        [Group(Sections.CustomisationOptions)]
        public bool WebResources { get; set; }

        [GridWidth(110)]
        [DisplayOrder(145)]
        [Group(Sections.CustomisationOptions)]
        public bool Workflows { get; set; }

        [GridWidth(110)]
        [DisplayOrder(160)]
        [Group(Sections.UsersAndRoleOptions)]
        public bool FieldSecurityProfiles { get; set; }

        [GridWidth(110)]
        [DisplayOrder(155)]
        [Group(Sections.UsersAndRoleOptions)]
        public bool SecurityRoles { get; set; }

        [GridWidth(110)]
        [DisplayOrder(156)]
        [Group(Sections.UsersAndRoleOptions)]
        public bool SecurityRolesPrivileges { get; set; }

        [GridWidth(110)]
        [DisplayOrder(170)]
        [Group(Sections.UsersAndRoleOptions)]
        public bool Teams { get; set; }

        [GridWidth(110)]
        [DisplayOrder(165)]
        [Group(Sections.UsersAndRoleOptions)]
        public bool Users { get; set; }

        public IsValidResponse Validate()
        {
            //lets just ensure at leats one valid oiton is selected
            var validProperties = new[] { nameof(Entities), nameof(Fields), nameof(FieldOptionSets), nameof(SharedOptionSets), nameof(Relationships), nameof(Solutions), nameof(Workflows), nameof(PluginAssemblies), nameof(SecurityRoles), nameof(FieldSecurityProfiles), nameof(Users), nameof(Teams), nameof(Reports), nameof(WebResources), nameof(PluginTriggers), nameof(FormsAndDashboards), nameof(SecurityRolesPrivileges) };
            var isOneSelected = validProperties.Any(p => (bool)this.GetPropertyValue(p));
            var isValidResponse = new IsValidResponse();
            if(!isOneSelected)
            {
                var thisType = GetType();
                isValidResponse.AddInvalidReason($"At Least One Of {validProperties.Select(p => thisType.GetProperty(p).GetDisplayName()).JoinGrammarAnd()} Is Required To Be Selected");
            }
            return isValidResponse;
        }

        private static class Sections
        {
            public const string Folder = "Select folder to save to";
            public const string TableAndFieldOptions = "Table and field options";
            public const string CustomisationOptions = "Customisation options";
            public const string UsersAndRoleOptions = "User, team & role options";
        }
    }
}