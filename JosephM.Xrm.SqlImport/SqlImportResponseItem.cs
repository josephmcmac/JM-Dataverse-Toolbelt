using JosephM.Xrm.DataImportExport.Import;

namespace JosephM.Xrm.SqlImport
{
    public class SqlImportResponseItem : DataImportResponseItem
    {
        public SqlImportResponseItem(DataImportResponseItem item)
            : base(item.Entity, item.Field, item.Name, item.FieldValue, item.Message, item.Exception, rowNumber: item.RowNumber)
        {
        }
    }
}