using JosephM.Core;

namespace JosephM.Record.Sql
{
    public class SqlRecordConfiguration
    {
        public string SqlServer { get; set; }
        public string DataBase { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string GetConnectionString()
        {
            return
                "Provider=sqloledb;Data Source={0};Initial Catalog={1};User Id={2};Password={3};".FormatString(
                    SqlServer, DataBase, UserName,
                    Password);
        }
    }
}