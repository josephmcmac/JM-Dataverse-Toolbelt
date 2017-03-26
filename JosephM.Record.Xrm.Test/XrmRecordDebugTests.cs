#region

using System;
using System.Linq;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Test;
using JosephM.Record.Extentions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace JosephM.Record.Xrm.Test
{
    [TestClass]
    public class XrmRecordDebugTests : XrmRecordTest
    {
        [TestMethod]
        public void XrmRecordDebug()
        {
            var config = XrmConfiguration;

            var views = XrmRecordService.GetFirst("sdkmessagefilter", "primaryobjecttypecode", "none");
        }
    }
}