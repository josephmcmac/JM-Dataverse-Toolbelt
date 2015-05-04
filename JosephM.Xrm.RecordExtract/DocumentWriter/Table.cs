using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace JosephM.Xrm.RecordExtract.DocumentWriter
{
    public class Table
    {
        protected MigraDoc.DocumentObjectModel.Tables.Table ThisTable { get; set; }

        public Table(MigraDoc.DocumentObjectModel.Tables.Table table)
        {
            ThisTable = table;
            ThisTable.Style = "Table";
            ThisTable.Format.LeftIndent = ".2cm";
            ThisTable.Borders.Visible = true;
        }

        protected Row AddRowToTable()
        {
            var row = ThisTable.AddRow();
            row.Borders.Visible = false;
            row.Format.SpaceAfter = ".2cm";
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            return row;
        }
    }
}