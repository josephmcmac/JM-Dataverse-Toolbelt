namespace JosephM.CustomisationExporter.Exporter
{
    public class UserExport
    {
        public UserExport(string fullName, string status, int roleCount, string roles, int teamCount, string teams, int fieldSecurityCount, string fieldSecurities)
        {
            FullName = fullName;
            Status = status;
            RoleCount = roleCount;
            Roles = roles;
            TeamCount = teamCount;
            Teams = teams;
            FieldSecurityCount = fieldSecurityCount;
            FieldSecurities = fieldSecurities;
        }

        public string FullName { get; set; }
        public string Status { get; set; }
        public int RoleCount { get; }
        public string Roles { get; }
        public int TeamCount { get; }
        public string Teams { get; }
        public int FieldSecurityCount { get; }
        public string FieldSecurities { get; }
    }
}