using System;
using System.IO;
using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

namespace JosephM.Record.Sql
{
    [ServiceConnection(typeof(CsvRecordService))]
    public class CsvFileConnection : IValidatableObject
    {
        public CsvFileConnection()
        {

        }

        public CsvFileConnection(string fileName)
        {
            File = new FileReference(fileName);
        }

        [FileMask(FileMasks.CsvFile)]
        public FileReference File { get; set; }

        public override string ToString()
        {
            return Path.GetFileName(File.FileName) ?? "Error No Name " + base.ToString();
        }

        public IsValidResponse Validate()
        {
            try
            {
                var service = new CsvRecordService(this);
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