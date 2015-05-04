using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace JosephM.Xrm.RecordExtract.DocumentWriter
{
    public class Table1Column : Table
    {
        public Table1Column(MigraDoc.DocumentObjectModel.Tables.Table table)
            : base(table)
        {
            var column = table.AddColumn("18cm");
            column.Format.Alignment = ParagraphAlignment.Left;
        }

        public void AddRow(string text)
        {
            var row = AddRowToTable();
            row.Cells[0].AddParagraph("" + text);
            row.Cells[0].Format.Font.Bold = false;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Top;
        }
    }
}