using System;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.DocumentWriter
{
    public class Document
    {
        private MigraDoc.DocumentObjectModel.Document ThisDocument { get; set; }

        public Document()
        {
            ThisDocument = new MigraDoc.DocumentObjectModel.Document();
        }

        public Section AddSection()
        {
            return new Section(ThisDocument.AddSection());
        }

        public string Save(Folder folder, string fileName, DocumentType documentType)
        {
            var theActualFileName = fileName;
            if (documentType == DocumentType.Pdf)
            {
                theActualFileName = theActualFileName.EndsWith(".pdf") ? theActualFileName : theActualFileName + ".pdf";
                SaveDocumentAsPdf(string.Format(@"{0}\{1}", folder, theActualFileName));
            }
            else if (documentType == DocumentType.Rtf)
            {
                theActualFileName = theActualFileName.EndsWith(".rtf") ? theActualFileName : theActualFileName + ".rtf";
                SaveDocumentAsRtf(folder.FolderPath, theActualFileName);
            }
            else
                throw new Exception(string.Format(
                    "There Was No Matching {0} In The {1} To Export To. The Type Was: {2}", typeof (DocumentType).Name,
                    typeof (RecordExtractRequest).Name, documentType));
            return theActualFileName;
        }

        private void SaveDocumentAsPdf(string fileName)
        {
            var renderer = new PdfDocumentRenderer(true);
            renderer.Document = ThisDocument;
            renderer.RenderDocument();
            renderer.Save(fileName.EndsWith(".pdf") ? fileName : fileName + ".pdf");
        }

        private void SaveDocumentAsRtf(string folder, string fileName)
        {
            var renderer = new RtfDocumentRenderer();
            FileUtility.CheckCreateFolder(folder);
            renderer.Render(ThisDocument, fileName.EndsWith(".rtf") ? fileName : fileName + ".rtf", folder);
        }
    }
}