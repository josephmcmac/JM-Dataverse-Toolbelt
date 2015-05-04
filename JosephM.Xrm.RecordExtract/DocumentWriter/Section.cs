using System.Collections.Generic;
using MigraDoc.DocumentObjectModel;
using JosephM.Core.Extentions;

namespace JosephM.Xrm.RecordExtract.DocumentWriter
{
    public class Section
    {
        private MigraDoc.DocumentObjectModel.Section ThisSection { get; set; }

        public Section(MigraDoc.DocumentObjectModel.Section section)
        {
            ThisSection = section;
            ThisSection.PageSetup.LeftMargin = "2cm";
            ThisSection.PageSetup.RightMargin = "2cm";
        }

        public void AddHeading1(string heading)
        {
            var paragraph = ThisSection.AddParagraph();
            paragraph.Format.SpaceAfter = ".5cm";
            paragraph.Format.Font.Size = "14pt";
            paragraph.AddFormattedText(heading, TextFormat.Bold);
        }

        public ContentBookmark AddHeading1WithBookmark(string heading)
        {
            var paragraph = ThisSection.AddParagraph();
            paragraph.Format.SpaceAfter = ".5cm";
            paragraph.Format.Font.Size = "14pt";
            paragraph.AddFormattedText(heading, TextFormat.Bold);
            var bookmark = new ContentBookmark(heading);
            paragraph.AddBookmark(bookmark.Key);
            return bookmark;
        }

        public ContentBookmark AddBookmark(string displayLabel)
        {
            var bookmark = new ContentBookmark(displayLabel);
            ThisSection.AddParagraph().AddBookmark(bookmark.Key);
            return bookmark;
        }

        public void AddHeading2(string heading)
        {
            AddHeading2Internal(heading);
        }

        private Paragraph AddHeading2Internal(string heading)
        {
            var paragraph = ThisSection.AddParagraph();
            paragraph.Format.SpaceAfter = ".4cm";
            paragraph.Format.Font.Size = "12pt";
            paragraph.Format.LeftIndent = ".1cm";
            paragraph.AddFormattedText(heading, TextFormat.Bold);
            return paragraph;
        }

        public ContentBookmark AddHeading2WithBookmark(string heading)
        {
            var paragraph = AddHeading2Internal(heading);
            var bookmark = new ContentBookmark(heading);
            paragraph.AddBookmark(bookmark.Key);
            return bookmark;
        }

        private Paragraph AddHeading3Internal(string heading)
        {
            var paragraph = ThisSection.AddParagraph();
            paragraph.Format.SpaceAfter = ".4cm";
            paragraph.Format.Font.Size = "11pt";
            paragraph.Format.LeftIndent = ".2cm";
            paragraph.AddFormattedText(heading, TextFormat.Bold);
            return paragraph;
        }

        public ContentBookmark AddHeading3WithBookmark(string heading)
        {
            var paragraph = AddHeading3Internal(heading);
            var bookmark = new ContentBookmark(heading);
            paragraph.AddBookmark(bookmark.Key);
            return bookmark;
        }

        public static string CheckStripHtml(string field, string display)
        {
            if (field == "description")
                display = display.StripHtml();
            return display;
        }

        public Table2Column Add2ColumnTable()
        {
            var table = AddTable();
            return new Table2Column(table);
        }

        private MigraDoc.DocumentObjectModel.Tables.Table AddTable()
        {
            var table = ThisSection.AddTable();
            ThisSection.AddParagraph();
            return table;
        }

        public Table1Column Add1ColumnTable()
        {
            var table = AddTable();
            return new Table1Column(table);
        }

        public void AddTitle(string title)
        {
            var paragraph = ThisSection.AddParagraph();
            paragraph.Format.SpaceAfter = ".4cm";
            paragraph.Format.Font.Size = "16pt";
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.AddFormattedText(title, TextFormat.Bold);
        }

        public void AddTableOfContents(IEnumerable<ContentBookmark> bookmarks)
        {
            AddHeading1("Table Of Contents");
            var table = new TableOfContents(ThisSection.AddTable());
            foreach (var bookmark in bookmarks)
            {
                table.AddRow(bookmark);
            }
        }

        internal void AddParagraph(string text, bool bold)
        {
            var paragraph = ThisSection.AddParagraph();
            paragraph.Format.LeftIndent = ".2cm";
            paragraph.Format.SpaceAfter = ".2cm";
            paragraph.AddFormattedText(text, bold ? TextFormat.Bold : TextFormat.NotBold);
        }
    }
}