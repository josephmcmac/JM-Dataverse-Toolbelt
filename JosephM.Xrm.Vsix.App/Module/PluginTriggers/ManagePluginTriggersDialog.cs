using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
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
    public class ManagePluginTriggersDialog : DialogViewModel
    {
        public XrmRecordService XrmRecordService { get; set; }
        public XrmPackageSettings PackageSettings { get; set; }
        IVisualStudioService VisualStudioService { get; set; }

        public ManagePluginTriggersDialog(IDialogController dialogController, IVisualStudioService visualStudioService, XrmRecordService xrmRecordService, XrmPackageSettings packageSettings)
            : base(dialogController)
        {
            PackageSettings = packageSettings;
            VisualStudioService = visualStudioService;
            XrmRecordService = xrmRecordService;

            var configEntryDialog = new ObjectGetEntryDialog(() => EntryObject, this, ApplicationController, XrmRecordService);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        private void Load(string assemblyName)
        {
            var assemblyRecord = XrmRecordService.GetFirst(Entities.pluginassembly, Fields.pluginassembly_.name,
                assemblyName);
            if (assemblyRecord == null)
                throw new NullReferenceException("Assembly Not Deployed");

            var pluginTypes = XrmRecordService.RetrieveAllAndClauses(Entities.plugintype,
                new[] {new Condition(Fields.plugintype_.pluginassemblyid, ConditionType.Equal, assemblyRecord.Id)});
            if (!pluginTypes.Any())
                throw new NullReferenceException("Not Plugin Types Deployed For Assembly");

            SdkMessageStepsPre = XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessageprocessingstep,
                pluginTypes.Select(
                    pt => new Condition(Fields.sdkmessageprocessingstep_.plugintypeid, ConditionType.Equal, pt.Id)));
            var sdkMessageStepsWithFilter = SdkMessageStepsPre
                .Where(sms => sms.GetField(Fields.sdkmessageprocessingstep_.sdkmessagefilterid) != null);

            var filters = !sdkMessageStepsWithFilter.Any()
                ? new IRecord[0]
                : XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessagefilter,
                    sdkMessageStepsWithFilter.Select(
                        sms =>
                            new Condition(Fields.sdkmessagefilter_.sdkmessagefilterid, ConditionType.Equal,
                                sms.GetLookupId(Fields.sdkmessageprocessingstep_.sdkmessagefilterid))));


            _entryObject = new PluginTriggers();
            var triggers = new List<PluginTrigger>();
            EntryObject.Triggers = triggers;
            EntryObject.Assembly = assemblyRecord.ToLookup();
            foreach (var item in SdkMessageStepsPre)
            {
                var filterId = item.GetLookupId(Fields.sdkmessageprocessingstep_.sdkmessagefilterid);
                var matchingFilters = filters.Where(f => f.Id == filterId);
                var filter = matchingFilters.Any() ? matchingFilters.First() : null;
                var recordType = filter == null ? null : filter.GetStringField(Fields.sdkmessagefilter_.primaryobjecttypecode);
                RecordType recordTypeObj = null;
                try
                {
                    recordTypeObj = new RecordType(recordType, XrmRecordService.GetDisplayName(recordType));
                }
                catch (Exception)
                {
                }

                var rank = item.GetIntegerField(Fields.sdkmessageprocessingstep_.rank);
                var name = item.GetStringField(Fields.sdkmessageprocessingstep_.name);
                var stage = item.GetOptionKey(Fields.sdkmessageprocessingstep_.stage);
                var mode = item.GetOptionKey(Fields.sdkmessageprocessingstep_.mode);
                var filteringAttributesString = item.GetStringField(Fields.sdkmessageprocessingstep_.filteringattributes);

                var trigger = new PluginTrigger();
                trigger.Id = item.Id;
                //for some unknown reason thi field was setting the target type ot sdkmessage filter 
                //despite the target being plugin type so I had to implement this to correct the type 
                //the name is popuated after the loop
                trigger.Message = filter == null ? null : filter.GetLookupField(Fields.sdkmessagefilter_.sdkmessageid);
                item.SetField(Fields.sdkmessageprocessingstep_.plugintypeid, new Lookup(Entities.plugintype, item.GetLookupId(Fields.sdkmessageprocessingstep_.plugintypeid), null), XrmRecordService);
                trigger.Plugin = item.GetLookupField(Fields.sdkmessageprocessingstep_.plugintypeid);
                trigger.RecordType = recordTypeObj;
                trigger.Stage = stage.ParseEnum<PluginTrigger.PluginStage>();
                trigger.Mode = mode.ParseEnum<PluginTrigger.PluginMode>();
                trigger.Rank = rank;
                trigger.FilteringFields = filteringAttributesString == null ? new RecordField[0]
                    : filteringAttributesString.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => new RecordField(s, s))
                    .ToArray();


                triggers.Add(trigger);
            }
            //since I had to correct the target type for this fieldsw lookup need to populate the name
            if (triggers.Any())
            {
                XrmRecordService.PopulateLookups(new Dictionary<string,List<Lookup>>()
                {
                    { Fields.sdkmessageprocessingstep_.plugintypeid, triggers.Select(t => t.Plugin).ToList() }
                }, null);
            }
        }

        public IEnumerable<IRecord> SdkMessageStepsPre { get; set; }

        private PluginTriggers _entryObject;

        public PluginTriggers EntryObject
        {
            get
            {
                if (_entryObject == null)
                {
                    Load(AssemblyName);
                }
                return _entryObject;
            }
        }

        public string AssemblyName { get; private set; }
        protected override void LoadDialogExtention()
        {
            AssemblyName = VisualStudioService.GetSelectedProjectAssemblyName();
            if (string.IsNullOrWhiteSpace(AssemblyName))
            {
                throw new NullReferenceException("Could Not Find Assembly Name");
            }
            else
                StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            var responses = new List<PluginTriggerError>();

            var removedPlugins = SdkMessageStepsPre.Where(smsp => EntryObject.Triggers.All(pt => pt.Id != smsp.Id)).ToArray();

            var deletions = XrmRecordService.DeleteInCrm(removedPlugins);
            foreach (var trigger in deletions.Errors)
            {
                var error = new PluginTriggerError()
                {
                    Type = "Deletion",
                    Name = trigger.Key.GetLookupName(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid),
                    Exception = trigger.Value
                };
                responses.Add(error);
            }

            var unloadedObjects = new List<IRecord>();

            var filters = EntryObject.Triggers.Select(t =>
            {
                var filter = new Filter();
                filter.AddCondition(Fields.sdkmessagefilter_.primaryobjecttypecode, ConditionType.Equal, t.RecordType == null ? "none" : t.RecordType.Key);
                filter.AddCondition(Fields.sdkmessagefilter_.sdkmessageid, ConditionType.Equal, t.Message.Id);
                return filter;
            }).ToArray();

            var pluginFilters = XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessagefilter, filters);

            foreach (var item in EntryObject.Triggers)
            {
                var matchingPluginFilters =
                    pluginFilters.Where(f => f.GetLookupId(Fields.sdkmessagefilter_.sdkmessageid) == item.Message.Id
                                             &&
                                             ((item.RecordType == null && f.GetStringField(Fields.sdkmessagefilter_.primaryobjecttypecode) == "none")
                                              ||
                                              (item.RecordType != null && f.GetStringField(Fields.sdkmessagefilter_.primaryobjecttypecode) == item.RecordType.Key)))
                                               .ToArray();
                var record = XrmRecordService.NewRecord(Entities.sdkmessageprocessingstep);
                var name = string.Format("{0} {1} {2} {3} {4}", item.Plugin, item.RecordType?.Key ?? "none", item.Message, item.Stage, item.Mode).Left(XrmRecordService.GetMaxLength(Fields.sdkmessageprocessingstep_.name, Entities.sdkmessageprocessingstep));
                record.SetField(Fields.sdkmessageprocessingstep_.name, name, XrmRecordService);
                record.SetField(Fields.sdkmessageprocessingstep_.rank, item.Rank, XrmRecordService);
                if(item.Stage != null)
                    record.SetField(Fields.sdkmessageprocessingstep_.stage, (int)item.Stage, XrmRecordService);
                if (item.Mode != null)
                    record.SetField(Fields.sdkmessageprocessingstep_.mode, (int)item.Mode, XrmRecordService);
                record.SetField(Fields.sdkmessageprocessingstep_.plugintypeid, item.Plugin, XrmRecordService);
                record.SetField(Fields.sdkmessageprocessingstep_.sdkmessagefilterid, !matchingPluginFilters.Any() ? null : matchingPluginFilters.First().ToLookup(), XrmRecordService);
                record.SetField(Fields.sdkmessageprocessingstep_.sdkmessageid, item.Message, XrmRecordService);
                if (item.Id != null)
                    record.SetField(Fields.sdkmessageprocessingstep_.sdkmessageprocessingstepid, item.Id, XrmRecordService);
                record.SetField(Fields.sdkmessageprocessingstep_.asyncautodelete, item.Mode == PluginTrigger.PluginMode.Asynchronous, XrmRecordService);
                record.SetField(Fields.sdkmessageprocessingstep_.filteringattributes, item.FilteringFields != null && item.FilteringFields.Any() ? string.Join(",", item.FilteringFields.OrderBy(r => r.Key).Select(r => r.Key)) : null, XrmRecordService);
                unloadedObjects.Add(record);
            }

            var triggerLoads = XrmRecordService.LoadIntoCrm(unloadedObjects,
                Fields.sdkmessageprocessingstep_.sdkmessageprocessingstepid);

            var updatesAndDeletes =
                unloadedObjects.Where(
                    r =>
                        new[] {"Update", "Delete"}.Contains(
                            r.GetLookupName(Fields.sdkmessageprocessingstep_.sdkmessageid)))
                            .ToArray();

            var images = new List<IRecord>();
            foreach (var item in updatesAndDeletes)
            {
                var imageRecord = XrmRecordService.NewRecord(Entities.sdkmessageprocessingstepimage);
                imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.name, "PreImage", XrmRecordService);
                imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.entityalias, "PreImage", XrmRecordService);
                imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.messagepropertyname, "Target", XrmRecordService);
                imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, XrmRecordService.ToLookup(item), XrmRecordService);
                imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.imagetype, OptionSets.SdkMessageProcessingStepImage.ImageType.PreImage, XrmRecordService);
                images.Add(imageRecord);
            }
            var imageLoads = XrmRecordService.LoadIntoCrm(images,
                Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid);

            foreach (var trigger in triggerLoads.Errors)
            {
                var error = new PluginTriggerError()
                {
                    Type = "Plugin Step",
                    Name = trigger.Key.GetStringField(Fields.sdkmessageprocessingstep_.name),
                    Exception = trigger.Value
                };
                responses.Add(error);
            }
            foreach (var trigger in imageLoads.Errors)
            {
                var error = new PluginTriggerError()
                {
                    Type = "Image",
                    Name = trigger.Key.GetLookupName(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid),
                    Exception = trigger.Value
                };
                responses.Add(error);
            }

            //add plugin steps to the solution
            var componentType = OptionSets.SolutionComponent.ObjectTypeCode.SDKMessageProcessingStep;
            var itemsToAdd = triggerLoads.Created.Union(triggerLoads.Updated);
            if (PackageSettings.AddToSolution)
                XrmRecordService.AddSolutionComponents(PackageSettings.Solution.Id, componentType, itemsToAdd);

            CompletionItem = new Completionresponse { Errors = responses };

            if (responses.Any())
                CompletionMessage = "There Were Errors Thrown Updating The Plugins";
            else
                CompletionMessage = "Plugins Updated";
        }

        public class Completionresponse
        {
            [Hidden]
            public bool HasResponses
            {
                get { return Errors != null && Errors.Any(); }
            }

            [PropertyInContextByPropertyValue(nameof(HasResponses), true)]
            public IEnumerable<PluginTriggerError> Errors { get; set; }
        }

        public class PluginTriggerError : ServiceResponseItem
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }
    }
}