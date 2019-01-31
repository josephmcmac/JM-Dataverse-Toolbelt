using JosephM.Deployment.DataImport;

namespace JosephM.Deployment.ImportSql
{
    public class ImportSqlResponseItem : DataImportResponseItem
    {
        public ImportSqlResponseItem(DataImportResponseItem item)
            : base(item.Entity, item.Field, item.Name, item.FieldValue, item.Message, item.Exception, rowNumber: item.RowNumber)
        {
        }
    }
}