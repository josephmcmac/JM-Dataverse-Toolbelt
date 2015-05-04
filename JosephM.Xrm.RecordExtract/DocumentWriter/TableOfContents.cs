using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace JosephM.Xrm.RecordExtract.DocumentWriter
{
    public class TableOfContents : Table
    {
        public TableOfContents(MigraDoc.DocumentObjectModel.Tables.Table table)
            : base(table)
        {
            var labelColumn = table.AddColumn("18cm");
            labelColumn.Format.Alignment = ParagraphAlignment.Left;
        }

        public void AddRow(ContentBookmark bookmark)
        {
            AddRow(bookmark, 0);
        }

        public void AddRow(ContentBookmark bookmark, int level)
        {
            var row = AddRowToTable();
            var isbold = level == 0;
            var paragraph = row.Cells[0].AddParagraph();
            row.Cells[0].Format.Font.Bold = isbold;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Top;
            paragraph.Format.LeftIndent = "." + (level*2) + "cm";
            var hyperlink = paragraph.AddHyperlink(bookmark.Key, HyperlinkType.Bookmark);
            hyperlink.AddText(string.Format("{0}", bookmark.DisplayLabel));
            if (level < 3 && (bookmark.ChildBookmarks != null))
            {
                foreach (var child in bookmark.ChildBookmarks)
                {
                    AddRow(child, level + 1);
                }
            }
        }
    }
}