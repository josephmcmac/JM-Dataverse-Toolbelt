using JosephM.Core.Attributes;

namespace JosephM.Core.Sql
{
    public class SqlServerAndDbSettings : ISqlSettings
    {
        public string SqlServer { get; set; }

        public string Database { get; set; }

        public SqlServerAndDbSettings(string sqlServer, string database)
        {
            SqlServer = sqlServer;
            Database = database;
        }

        public SqlServerAndDbSettings()
        {

        }

        [Hidden]
        public string ConnectionString
        {
            get
            {
                return SqlProvider.GetConnectionString(SqlServer, Database);
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}].[{1}]", SqlServer, Database);
        }
    }
}


