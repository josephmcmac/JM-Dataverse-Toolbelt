using System;

namespace JosephM.SolutionComponentExporter.Type
{
    public class SecurityRoleExport
    {
        public SecurityRoleExport(string name, string businessUnit, int userCount, string users, int teamCount, string teams, string modifiedBy, DateTime modifiedOn)
        {
            Name = name;
            BusinessUnit = businessUnit;
            UserCount = userCount;
            Users = users;
            TeamCount = teamCount;
            Teams = teams;
            ModifiedBy = modifiedBy;
            ModifiedOn = modifiedOn;
        }

        public string Name { get; set; }
        public string BusinessUnit { get; set; }
        public int UserCount { get; }
        public string Users { get; }
        public int TeamCount { get; }
        public string Teams { get; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}