using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
{
    [RequiresConnection]
    public partial class ManagePluginTriggersDialog : ServiceRequestDialog<ManagePluginTriggersService, ManagePluginTriggersRequest, ManagePluginTriggersResponse, ManagePluginTriggersResponseitem>
    {
        public XrmRecordService XrmRecordService { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        IVisualStudioService VisualStudioService { get; set; }

        public ManagePluginTriggersDialog(ManagePluginTriggersService service, IDialogController dialogController, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings)
            : base(service, dialogController, lookupService: xrmRecordService, nextButtonLabel: "Save")
        {
            PackageSettings = packageSettings;
            VisualStudioService = visualStudioService;
            XrmRecordService = xrmRecordService;
        }

        protected override void LoadDialogExtention()
        {
            //hijack the load method so that we can prepopulate
            //the entered request with various details
            LoadAssemblyDetails();

            StartNextAction();
        }

        private void LoadAssemblyDetails()
        {
            LoadingViewModel.LoadingMessage = "Loading Assembly";

            Request.AssemblyName = VisualStudioService.GetSelectedProjectAssemblyName();

            if (string.IsNullOrWhiteSpace(Request.AssemblyName))
            {
                throw new NullReferenceException("Could Not Find Assembly Name");
            }

            var assemblyRecord = XrmRecordService.GetFirst(Entities.pluginassembly, Fields.pluginassembly_.name,
                Request.AssemblyName);
            if (assemblyRecord == null)
                throw new NullReferenceException("There is no plugin assembly deployed in the dynamics instance with a matching name. Try the deploy assembly option to deploy a new plugin assembly, or rename the assembly to match an existing assembly deployed to the instance");

            LoadingViewModel.LoadingMessage = "Loading Plugin Types";

            var pluginTypes = XrmRecordService.RetrieveAllAndClauses(Entities.plugintype,
                new[] { new Condition(Fields.plugintype_.pluginassemblyid, ConditionType.Equal, assemblyRecord.Id) });
            if (!pluginTypes.Any())
                throw new NullReferenceException("There No Plugin Types Deployed In This Assembly So No Triggers Can Be Created For Them");

            LoadingViewModel.LoadingMessage = "Loading Plugin Triggers";

            Request.SetSdkMessageStepsPre(XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessageprocessingstep,
                pluginTypes.Select(
                    pt => new Condition(Fields.sdkmessageprocessingstep_.plugintypeid, ConditionType.Equal, pt.Id))));
            var sdkMessageStepsWithFilter = Request.GetSdkMessageStepsPre()
                .Where(sms => sms.GetField(Fields.sdkmessageprocessingstep_.sdkmessagefilterid) != null);

            LoadingViewModel.LoadingMessage = "Loading Plugin Filters";

            var filters = !sdkMessageStepsWithFilter.Any()
                ? new IRecord[0]
                : XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessagefilter,
                    sdkMessageStepsWithFilter.Select(
                        sms =>
                            new Condition(Fields.sdkmessagefilter_.sdkmessagefilterid, ConditionType.Equal,
                                sms.GetLookupId(Fields.sdkmessageprocessingstep_.sdkmessagefilterid))));

            LoadingViewModel.LoadingMessage = "Loading Plugin Images";

            var preImages = Request.GetSdkMessageStepsPre().Any()
                ? XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessageprocessingstepimage, Request.GetSdkMessageStepsPre().Select(m => new Condition(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, ConditionType.Equal, m.Id)))
                .Where(i => i.GetOptionKey(Fields.sdkmessageprocessingstepimage_.imagetype) == OptionSets.SdkMessageProcessingStepImage.ImageType.PreImage.ToString())
                .GroupBy(i => i.GetLookupField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid))
                .Where(g => g.Key != null && g.Key.Id != null)
                .ToDictionary(i => i.Key.Id, g => g.First())
                : new Dictionary<string, IRecord>();

            var triggers = new List<PluginTrigger>();
            foreach (var item in Request.GetSdkMessageStepsPre())
            {
                var filterId = item.GetLookupId(Fields.sdkmessageprocessingstep_.sdkmessagefilterid);
                var matchingFilters = filters.Where(f => f.Id == filterId);
                var filter = matchingFilters.Any() ? matchingFilters.First() : null;
                var recordType = filter == null ? null : filter.GetStringField(Fields.sdkmessagefilter_.primaryobjecttypecode);
                RecordType recordTypeObj = null;
                if (recordType != null)
                {
                    LoadingViewModel.LoadingMessage = $"Loading {XrmRecordService.GetDisplayName(recordType)} Type";
                    try
                    {

                        recordTypeObj = new RecordType(recordType, XrmRecordService.GetDisplayName(recordType));
                    }
                    catch (Exception)
                    {
                    }
                }

                var rank = item.GetIntegerField(Fields.sdkmessageprocessingstep_.rank);
                var name = item.GetStringField(Fields.sdkmessageprocessingstep_.name);
                var stage = item.GetOptionKey(Fields.sdkmessageprocessingstep_.stage);
                var mode = item.GetOptionKey(Fields.sdkmessageprocessingstep_.mode);
                var filteringAttributesString = item.GetStringField(Fields.sdkmessageprocessingstep_.filteringattributes);

                var trigger = new PluginTrigger();
                //load trigger details
                trigger.Id = item.Id;
                //for some unknown reason this field was setting the target type ot sdkmessage filter 
                //despite the target being plugin type so I had to implement this to correct the type 
                //the name is popuated after the loop
                trigger.Message = item.GetField(Fields.sdkmessageprocessingstep_.sdkmessageid) as Lookup;// filter == null ? null : filter.GetLookupField(Fields.sdkmessagefilter_.sdkmessageid);
                item.SetField(Fields.sdkmessageprocessingstep_.plugintypeid, new Lookup(Entities.plugintype, item.GetLookupId(Fields.sdkmessageprocessingstep_.plugintypeid), null), XrmRecordService);
                trigger.Plugin = item.GetLookupField(Fields.sdkmessageprocessingstep_.plugintypeid);
                trigger.RecordType = recordTypeObj;
                trigger.Stage = stage.ParseEnum<PluginTrigger.PluginStage>();
                trigger.Mode = mode.ParseEnum<PluginTrigger.PluginMode>();
                trigger.Rank = rank;
                trigger.FilteringFields = filteringAttributesString == null
                    ? new RecordField[0]
                    : filteringAttributesString.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => new RecordField(s, XrmRecordService.GetFieldLabel(s, recordType)))
                    .ToArray();
                trigger.SpecificUserContext = item.GetLookupField(Fields.sdkmessageprocessingstep_.impersonatinguserid);
                //load image details if there is one
                if (trigger.Id != null)
                {
                    if (!preImages.ContainsKey(item.Id))
                    {
                        trigger.PreImageAllFields = false;
                    }
                    else
                    {
                        var preImage = preImages[item.Id];
                        var attributes = preImage.GetStringField(Fields.sdkmessageprocessingstepimage_.attributes);
                        trigger.PreImageAllFields = string.IsNullOrWhiteSpace(attributes);
                        trigger.PreImageFields = attributes == null
                            ? new RecordField[0]
                            : attributes
                            .Split(',')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => new RecordField(s, XrmRecordService.GetFieldLabel(s, recordType)))
                            .ToArray();
                        trigger.PreImageName = preImage.GetStringField(Fields.sdkmessageprocessingstepimage_.entityalias);
                        trigger.PreImageId = preImage.Id;
                        trigger.PreImageIdUnique = preImage.GetField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepimageidunique)?.ToString();
                    }
                }
                triggers.Add(trigger);
            }
            LoadingViewModel.LoadingMessage = "Loading Plugin Names";
            //since I had to correct the target type for this fields lookup need to populate the name
            if (triggers.Any())
            {
                XrmRecordService.PopulateLookups(new Dictionary<string, List<Lookup>>()
                {
                    { Fields.sdkmessageprocessingstep_.plugintypeid, triggers.Select(t => t.Plugin).ToList() }
                }, null);
            }
            triggers = triggers
                .OrderBy(t => t.RecordType?.Value)
                .ThenBy(t => t.Message?.Name)
                .ThenByDescending(t => t.Stage)
                .ThenByDescending(t => t.Mode).ToList();
            Request.Triggers = triggers;
            Request.Assembly = assemblyRecord.ToLookup();

            LoadingViewModel.LoadingMessage = "Loading Plugins Into Grid";
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            if (Request.Triggers != null)
            {
                addProperty($"Trigger Count", Request.Triggers.Count().ToString());
                addProperty($"Trigger With Field Filters Count", Request.Triggers.Count(t => (t.FilteringFields?.Count() ?? 0) > 0).ToString());
                addProperty($"Trigger With PreImage Fields Count", Request.Triggers.Count(t => (t.PreImageFields?.Count() ?? 0) > 0).ToString());
                addProperty($"Trigger With Specific User Context Count", Request.Triggers.Count(t => t.SpecificUserContext != null).ToString());
                foreach (var messageGroup in Request.Triggers.Where(t => t.Message != null).GroupBy(t => t.Message.Name))
                {
                    addProperty($"Trigger {messageGroup.Key} Message Count", messageGroup.Count().ToString());
                }
            }
            return dictionary;
        }
    }
}