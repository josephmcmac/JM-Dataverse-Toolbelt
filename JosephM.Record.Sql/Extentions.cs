using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Record.Sql
{
    public static class Extentions
    {
        public static void ExecuteSql(this SqlRecordConnection connection, string sql)
        {
            var sqlService = new SqlRecordService(connection);
            sqlService.ExecuteSql(sql);
        }
    }
}
