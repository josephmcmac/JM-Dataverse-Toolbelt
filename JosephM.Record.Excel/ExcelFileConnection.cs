using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System;
using System.IO;

namespace JosephM.Record.Excel
{
    [ServiceConnection(typeof(ExcelRecordService))]
    public class ExcelFileConnection : IValidatableObject
    {
        public ExcelFileConnection()
        {

        }

        public ExcelFileConnection(string fileName)
        {
            File = new FileReference(fileName);
        }


        public ExcelFileConnection(FileReference fileReference)
        {
            File = fileReference;
        }

        [FileMask(FileMasks.ExcelFile)]
        public FileReference File { get; set; }

        public override string ToString()
        {
            return Path.GetFileName(File.FileName) ?? "Error No Name " + base.ToString();
        }

        public IsValidResponse Validate()
        {
            try
            {
                var service = new ExcelRecordService(this);
                return service.VerifyConnection();
            }
            catch (Exception ex)
            {
                var response = new IsValidResponse();
                response.AddInvalidReason("Error creating service: " + ex.DisplayString());
                return response;
            }

        }
    }
}