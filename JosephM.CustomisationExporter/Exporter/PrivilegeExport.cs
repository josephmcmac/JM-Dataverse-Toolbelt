using JosephM.Core.Attributes;

namespace JosephM.CustomisationExporter.Exporter
{
    public class PrivilegeExport
    {
        public PrivilegeExport(string roleName, int depth, string privilegeName, int accessRight, string entityType)
        {
            RoleName = roleName;
            switch(depth)
            {
                case 1 : Depth = "Basic (User)"; break;
                case 2: Depth = "Local (Business Unit)"; break;
                case 4: Depth = "Deep (Parent: Child)"; break;
                case 8: Depth = "Global (Organisation)"; break;
                default: Depth = depth.ToString(); break;
            }
            PrivilegeName = privilegeName;
            switch (accessRight)
            {
                case 0: AccessRight = "Yes"; break;
                case 1: AccessRight = "Read"; break;
                case 2: AccessRight = "Write"; break;
                case 4: AccessRight = "Append"; break;
                case 16: AccessRight = "Append To"; break;
                case 32: AccessRight = "Create"; break;
                case 65536: AccessRight = "Delete"; break;
                case 262144: AccessRight = "Share"; break;
                case 524288: AccessRight = "Assign"; break;
                default: AccessRight = accessRight.ToString(); break;
            }
            EntityType = entityType;
        }

        [DisplayOrder(10)]
        [GridWidth(250)]
        public string RoleName { get; set; }
        [DisplayOrder(20)]
        [GridWidth(350)]
        public string PrivilegeName { get; set; }
        [DisplayOrder(30)]
        [GridWidth(300)]
        public string EntityType { get; set; }
        [DisplayOrder(40)]
        [GridWidth(100)]
        public string AccessRight { get; set; }
        [DisplayOrder(50)]
        [GridWidth(155)]
        public string Depth { get; set; }
    }
}
