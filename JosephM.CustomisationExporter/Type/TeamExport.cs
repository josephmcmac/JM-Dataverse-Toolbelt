namespace JosephM.CustomisationExporter.Type
{
    public class TeamExport
    {
        public TeamExport(string name, string teamType, int userCount, string users, int roleCount, string roles, int fieldSecurityCount, string fieldSecurities)
        {
            Name = name;
            TeamType = teamType;
            UserCount = userCount;
            Users = users;
            RoleCount = roleCount;
            Roles = roles;
            FieldSecurityCount = fieldSecurityCount;
            FieldSecurities = fieldSecurities;
        }

        public string Name { get; set; }
        public string TeamType { get; set; }
        public int UserCount { get; }
        public string Users { get; }
        public int RoleCount { get; }
        public string Roles { get; }
        public int FieldSecurityCount { get; }
        public string FieldSecurities { get; }
    }
}