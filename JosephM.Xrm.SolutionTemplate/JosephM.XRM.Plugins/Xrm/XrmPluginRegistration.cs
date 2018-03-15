using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace $safeprojectname$.Xrm
{
    public abstract class XrmPluginRegistration : IPlugin
    {
        protected const string XRMRETRIEVEMULTIPLEFAKESCHEMANAME = "XRMRETRIEVEMULTIPLE";

        public void Execute(IServiceProvider serviceProvider)
        {
            string entityType;
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var message = context.MessageName;
            var isRelationship = message == PluginMessage.Associate || message == PluginMessage.Disassociate;
            entityType = GetTypeSchemaName(context, isRelationship);



            var plugin = CreateEntityPlugin(entityType, isRelationship);
            if (plugin != null)
            {
                plugin.ServiceProvider = serviceProvider;
                XrmPlugin.Go(plugin);
            }
        }

        private static string GetTypeSchemaName(IPluginExecutionContext context, bool isRelationship)
        {
            string entityType;
            if (isRelationship)
                entityType = ((Relationship)context.InputParameters["Relationship"]).SchemaName;
            else if (context.MessageName == PluginMessage.RetrieveMultiple)
                entityType = XRMRETRIEVEMULTIPLEFAKESCHEMANAME;
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                entityType = ((Entity)context.InputParameters["Target"]).LogicalName;
            else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                entityType = ((EntityReference)context.InputParameters["Target"]).LogicalName;
            else if (context.InputParameters.Contains("EntityMoniker") &&
                     context.InputParameters["EntityMoniker"] is EntityReference)
                entityType = ((EntityReference)context.InputParameters["EntityMoniker"]).LogicalName;
            else if (
                new[] { PluginMessage.AddMembers, PluginMessage.AddMember, PluginMessage.AddListMember }.Contains(
                    context.MessageName))
                entityType = "list";
            else if (context.InputParameters.Contains("LeadId") &&
                     context.InputParameters["LeadId"] is EntityReference)
                entityType = "lead";
            else if (context.InputParameters.Contains("EmailId"))
                entityType = "email";
            else if (context.MessageName == PluginMessage.Lose && context.InputParameters.Contains("OpportunityClose"))
                entityType = "opportunity";
            else if (context.MessageName == PluginMessage.Cancel)
                entityType = "salesorder";
            else if (context.MessageName == PluginMessage.Win || context.MessageName == PluginMessage.Lose)
                entityType = "opportunity";
            else if (context.MessageName == PluginMessage.Cancel)
                entityType = "salesorder";
            else
            {
                var args = "";
                args = args + "Message: " + context.MessageName;
                foreach (var item in context.InputParameters)
                {
                    if (args != "")
                        args = args + "\n" + item.Key + ": " + item.Value;
                    else
                        args = args + item.Key + ": " + item.Value;
                }
                throw new InvalidPluginExecutionException("Error Extracting Plugin Entity Type:\n" + args);
            }
            return entityType;
        }

        public abstract XrmPlugin CreateEntityPlugin(string entityType, bool isRelationship);
    }
}