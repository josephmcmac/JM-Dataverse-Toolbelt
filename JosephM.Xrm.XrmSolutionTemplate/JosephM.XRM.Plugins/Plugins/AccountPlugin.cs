using $safeprojectname$.Xrm;
using Schema;

namespace $safeprojectname$.Plugins
{
    public class AccountPlugin : TemplateEntityPluginBase
    {
        public override void GoExtention()
        {
            SamplePluginMethod();
        }


        /// <summary>
        /// Map state and opportunity topic into a new opportunity based on linked lead fields
        /// </summary>
        private void SamplePluginMethod()
        {
            if (IsMessage(PluginMessage.Create) && IsStage(PluginStage.PreOperationEvent))
            {
                if (FieldChanging(Fields.account_.name))
                {
                    TemplateService.SampleServiceMethod();
                    Trace(string.Format("The organisation name is {0}", TemplateSettings.OrganisationName));
                    Trace(string.Format("The new accounts name is {0}", GetStringField(Fields.account_.name)));
                }
            }
        }
    }
}