using System.Data.OleDb;

namespace JosephM.Core.Sql
{
    public class SqlTransaction : ITransaction
    {
        public OleDbTransaction Transaction { get; set; }

        public SqlTransaction(OleDbTransaction transaction)
        {
            Transaction = transaction;
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }

        public void Commit()
        {
            Transaction.Commit();
        }
    }
}
