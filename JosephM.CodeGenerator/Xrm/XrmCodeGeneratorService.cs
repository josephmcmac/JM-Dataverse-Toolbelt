using JosephM.CodeGenerator.Service;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.CodeGenerator.Xrm
{
    public class XrmCodeGeneratorService : CodeGeneratorService<XrmRecordService>
    {
        public XrmCodeGeneratorService(XrmRecordService service)
            : base(service)
        {
        }
    }
}