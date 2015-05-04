using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using JosephM.Core.Extentions;

namespace JosephM.Xrm.RecordExtract.DocumentWriter
{
    public class Table2Column : Table
    {
        public Table2Column(MigraDoc.DocumentObjectModel.Tables.Table table)
            : base(table)
        {
            var column = ThisTable.AddColumn("5cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            column = table.AddColumn("13cm");
            column.Format.Alignment = ParagraphAlignment.Left;
        }

        public void AddFieldToTable(string label, string display)
        {
            if (!label.IsNullOrWhiteSpace() && !display.IsNullOrWhiteSpace())
            {
                var row = AddRowToTable();
                row.Cells[0].AddParagraph("" + label);
                row.Cells[0].Format.Font.Bold = false;
                row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                row.Cells[0].VerticalAlignment = VerticalAlignment.Top;
                row.Cells[1].AddParagraph("" + display);
                row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            }
        }
    }
}