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
            var query = XrmRecordService.GetViewAsQueryDefinition("50E599B0-341A-EA11-8410-06E6E6094FD1");
        }
    }
}