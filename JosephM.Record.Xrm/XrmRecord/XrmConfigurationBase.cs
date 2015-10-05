using JosephM.Xrm;
using Microsoft.Xrm.Sdk.Metadata;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmConfigurationBase
    {
        protected XrmService XrmService { get; set; }

        public XrmConfigurationBase(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        protected static bool GetIsCustomLabel(AssociatedMenuConfiguration menuConfiguration)
        {
            if (!menuConfiguration.Behavior.HasValue ||
                menuConfiguration.Behavior != AssociatedMenuBehavior.UseLabel)
                return false;
            else
                return true;
        }

        protected string GetCustomLabel(AssociatedMenuConfiguration menuConfiguration)
        {
            if (menuConfiguration.Behavior.HasValue &&
                menuConfiguration.Behavior == AssociatedMenuBehavior.UseLabel)
                return XrmService.GetLabelDisplay(menuConfiguration.Label);
            return null;
        }

        protected static int GetDisplayOrder(AssociatedMenuConfiguration menuConfiguration)
        {
            if (!menuConfiguration.Order.HasValue)
                return 0;
            else
                return menuConfiguration.Order.Value;
        }

        protected static bool GetIsDisplayRelated(AssociatedMenuConfiguration menuConfiguration)
        {
            if (!menuConfiguration.Behavior.HasValue ||
                menuConfiguration.Behavior == AssociatedMenuBehavior.DoNotDisplay)
                return false;
            else
                return true;
        }

    }
}