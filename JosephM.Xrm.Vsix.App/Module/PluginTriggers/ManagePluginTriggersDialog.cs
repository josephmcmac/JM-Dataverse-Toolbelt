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

            var preImages = SdkMessageStepsPre.Any()
                ? XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessageprocessingstepimage, SdkMessageStepsPre.Select(m => new Condition(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, ConditionType.Equal, m.Id)))
                .Where(i => i.GetOptionKey(Fields.sdkmessageprocessingstepimage_.imagetype) == OptionSets.SdkMessageProcessingStepImage.ImageType.PreImage.ToString())
                .GroupBy(i => i.GetLookupField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid))
                .Where(g => g.Key != null && g.Key.Id != null)
                .ToDictionary(i => i.Key.Id, g => g.First())
                : new Dictionary<string, IRecord>();

            _entryObject = new PluginTriggers();
            var triggers = new List<PluginTrigger>();
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
                //load trigger details
                trigger.Id = item.Id;
                //for some unknown reason this field was setting the target type ot sdkmessage filter 
                //despite the target being plugin type so I had to implement this to correct the type 
                //the name is popuated after the loop
                trigger.Message = filter == null ? null : filter.GetLookupField(Fields.sdkmessagefilter_.sdkmessageid);
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
                    .Select(s => new RecordField(s, s))
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
                            .Select(s => new RecordField(s, s))
                            .ToArray();
                        trigger.PreImageName = preImage.GetStringField(Fields.sdkmessageprocessingstepimage_.entityalias);
                        trigger.PreImageId = preImage.Id;
                    }
                }
                triggers.Add(trigger);
            }
            //since I had to correct the target type for this fields lookup need to populate the name
            if (triggers.Any())
            {
                XrmRecordService.PopulateLookups(new Dictionary<string,List<Lookup>>()
                {
                    { Fields.sdkmessageprocessingstep_.plugintypeid, triggers.Select(t => t.Plugin).ToList() }
                }, null);
            }
            triggers = triggers
                .OrderBy(t => t.RecordType?.Value)
                .ThenBy(t => t.Message?.Name)
                .ThenByDescending(t => t.Stage)
                .ThenByDescending(t => t.Mode).ToList();
            EntryObject.Triggers = triggers;
            EntryObject.Assembly = assemblyRecord.ToLookup();
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

            //delete any removed plugins
            var removedPlugins = SdkMessageStepsPre.Where(smsp => EntryObject.Triggers.All(pt => pt.Id != smsp.Id)).ToArray();
            var deletions = XrmRecordService.DeleteInCrm(removedPlugins);
            foreach (var trigger in deletions.Errors)
            {
                var error = new PluginTriggerError()
                {
                    Type = "Deletion",
                    Name = trigger.Key.GetLookupName(Fields.sdkmessageprocessingstep_.sdkmessageprocessingstepid),
                    Exception = trigger.Value
                };
                responses.Add(error);
            }

            //load the filter entities which exist for each entity type, message combination
            var filters = EntryObject.Triggers.Select(t =>
            {
                var filter = new Filter();
                filter.AddCondition(Fields.sdkmessagefilter_.primaryobjecttypecode, ConditionType.Equal, t.RecordType == null ? "none" : t.RecordType.Key);
                filter.AddCondition(Fields.sdkmessagefilter_.sdkmessageid, ConditionType.Equal, t.Message.Id);
                return filter;
            }).ToArray();
            var pluginFilters = XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessagefilter, filters);

            //unload the triggers into an entity object referencing it in a dictionary
            var unloadedObjects = new Dictionary<IRecord, PluginTrigger>();
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
                record.SetField(Fields.sdkmessageprocessingstep_.impersonatinguserid, item.SpecificUserContext == null || item.SpecificUserContext.Id == null ? null : item.SpecificUserContext, XrmRecordService);
                if (item.Id != null)
                    record.SetField(Fields.sdkmessageprocessingstep_.sdkmessageprocessingstepid, item.Id, XrmRecordService);
                record.SetField(Fields.sdkmessageprocessingstep_.asyncautodelete, item.Mode == PluginTrigger.PluginMode.Asynchronous, XrmRecordService);
                record.SetField(Fields.sdkmessageprocessingstep_.filteringattributes, item.FilteringFields != null && item.FilteringFields.Any() ? string.Join(",", item.FilteringFields.OrderBy(r => r.Key).Select(r => r.Key)) : null, XrmRecordService);
                unloadedObjects.Add(record, item);
            }

            //submit them to crm create/update
            var triggerLoads = XrmRecordService.LoadIntoCrm(unloadedObjects.Keys,
                Fields.sdkmessageprocessingstep_.sdkmessageprocessingstepid);

            var updatesAndDeletes =
                unloadedObjects.Keys.Where(
                    r =>
                        new[] {"Update", "Delete"}.Contains(
                            r.GetLookupName(Fields.sdkmessageprocessingstep_.sdkmessageid)))
                            .ToArray();

            var solutionItemsToAdd = new List<string>();
            //update/delete pre-images
            var imagesToCreateOrUpdate = new List<IRecord>();
            var imagesToDelete = new List<IRecord>();
            foreach (var item in updatesAndDeletes
                .Where(i => !triggerLoads.Errors.Keys.Contains(CompletionItem)))
            {
                var matchingPluginTrigger = unloadedObjects[item];
                //the plugin will only have an image if all fields, or there are specific fields selected
                var hasImage = matchingPluginTrigger.PreImageAllFields || (matchingPluginTrigger.PreImageFields != null && matchingPluginTrigger.PreImageFields.Any());
                if (!hasImage)
                {
                    //delete the existing image if is has been changed to not have an image
                    if(matchingPluginTrigger.PreImageId != null)
                    {
                        try
                        {
                            imagesToDelete.Add(XrmRecordService.Get(Entities.sdkmessageprocessingstepimage, matchingPluginTrigger.PreImageId));
                            solutionItemsToAdd.Add(matchingPluginTrigger.Id);
                        }
                        catch (Exception ex)
                        {
                            var error = new PluginTriggerError()
                            {
                                Type = "Image Deletion",
                                Name = matchingPluginTrigger.PreImageId,
                                Exception = ex
                            };
                            responses.Add(error);
                        }
                    }
                }
                else
                {
                    //set the details to create/update in the pre-image
                    var isUpdate = triggerLoads.Updated.Contains(item);
                    var imageRecord = XrmRecordService.NewRecord(Entities.sdkmessageprocessingstepimage);
                    imageRecord.Id = matchingPluginTrigger.PreImageId;
                    if (matchingPluginTrigger.PreImageId != null)
                        imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepimageid, matchingPluginTrigger.PreImageId, XrmRecordService);
                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.name, matchingPluginTrigger.PreImageName, XrmRecordService);
                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.entityalias, matchingPluginTrigger.PreImageName, XrmRecordService);
                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.messagepropertyname, "Target", XrmRecordService);
                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, XrmRecordService.ToLookup(item), XrmRecordService);
                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.imagetype, OptionSets.SdkMessageProcessingStepImage.ImageType.PreImage, XrmRecordService);
                    var attributesString = matchingPluginTrigger.PreImageAllFields || matchingPluginTrigger.PreImageFields == null || !matchingPluginTrigger.PreImageFields.Any()
                        ? null
                        : string.Join(",", matchingPluginTrigger.PreImageFields.Select(f => f.Key).OrderBy(s => s).ToArray());
                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.attributes, attributesString, XrmRecordService);
                    imagesToCreateOrUpdate.Add(imageRecord);
                }
            }

            //submit create/update/deletion of pre-images
            var imageLoads = XrmRecordService.LoadIntoCrm(imagesToCreateOrUpdate,
                Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepimageid);
            var imageDeletions = XrmRecordService.DeleteInCrm(imagesToDelete);

            //add any errors to the response object
            foreach (var trigger in imageDeletions.Errors)
            {
                var error = new PluginTriggerError()
                {
                    Type = "Image Deletion",
                    Name = trigger.Key.GetLookupName(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid),
                    Exception = trigger.Value
                };
                responses.Add(error);
            }

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
            solutionItemsToAdd.AddRange(triggerLoads.Created.Union(triggerLoads.Updated).Select(r => r.Id).ToList());
            var imagesReferences = imageLoads.Created.Union(imageLoads.Updated)
                .Select(i => i.GetLookupId(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid))
                .Where(id => !string.IsNullOrWhiteSpace(id));
            solutionItemsToAdd.AddRange(imagesReferences);

            if (PackageSettings.AddToSolution)
                XrmRecordService.AddSolutionComponents(PackageSettings.Solution.Id, componentType, solutionItemsToAdd);

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