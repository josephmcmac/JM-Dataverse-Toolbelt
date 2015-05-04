using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.CodeGenerator.Service
{
    public class XrmCodeGeneratorService : CodeGeneratorService<XrmRecordService>
    {
        public XrmCodeGeneratorService(XrmRecordService service)
            : base(service)
        {
        }
    }
}