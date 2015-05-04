using System.ComponentModel;

namespace JosephM.Xrm.RecordExtract.DocumentWriter
{
    public enum DocumentType
    {
        [Description("Microsoft Word Format (RTF)")] Rtf,
        [Description("Portable Document Format (PDF)")] Pdf
    }
}