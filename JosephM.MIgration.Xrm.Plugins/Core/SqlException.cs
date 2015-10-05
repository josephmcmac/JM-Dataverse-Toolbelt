#region

using System;

#endregion

namespace JosephM.MIgration.Xrm.Plugins.Core
{
    public class SqlException : Exception
    {
        public string Query { get; private set; }

        public SqlException(string query, Exception innerException)
            : base(string.Format("Error Executing Sql:\n{0}", query), innerException)
        {
            Query = query;
        }
    }
}