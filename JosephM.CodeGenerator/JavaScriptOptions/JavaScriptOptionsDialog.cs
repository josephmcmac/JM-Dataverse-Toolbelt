﻿using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    public class JavaScriptOptionsDialog :
        ServiceRequestDialog<JavaScriptOptionsService, JavaScriptOptionsRequest, JavaScriptOptionsResponse, ServiceResponseItem>
    {
        public JavaScriptOptionsDialog(JavaScriptOptionsService service, IDialogController dialogController, XrmRecordService xrmRecordService)
            : base(service, dialogController, xrmRecordService)
        {
        }
    }
}