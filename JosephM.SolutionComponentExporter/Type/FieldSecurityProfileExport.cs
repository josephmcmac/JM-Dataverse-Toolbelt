using System;

namespace JosephM.SolutionComponentExporter.Type
{
    public class FieldSecurityProfileExport
    {
        public FieldSecurityProfileExport(string name, int teamCount, string teams, int userCount, string users, string modifiedBy, DateTime modifiedOn)
        {
            Name = name;
            TeamCount = teamCount;
            Teams = teams;
            UserCount = userCount;
            Users = users;
            ModifiedBy = modifiedBy;
            ModifiedOn = modifiedOn;
        }

        public string Name { get; set; }
        public int TeamCount { get; set; }
        public string Teams { get; }
        public int UserCount { get; set; }
        public string Users { get; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}