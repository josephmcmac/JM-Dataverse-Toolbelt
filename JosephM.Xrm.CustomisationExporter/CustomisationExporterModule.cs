using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Xrm.CustomisationExporter
{
    public class CustomisationExporterModule :
        ServiceRequestModule
            <CustomisationExporterDialog, CustomisationExporterService, CustomisationExporterRequest, CustomisationExporterResponse, CustomisationExporterResponseItem>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            ApplicationOptions.AddHelp("Export Customisations", "Customisation Exporter Help.htm");
        }
    }
}
