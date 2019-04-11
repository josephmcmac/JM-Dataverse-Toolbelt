using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
{
    public class ManagePluginTriggersService :
        ServiceBase<ManagePluginTriggersRequest, ManagePluginTriggersResponse, ManagePluginTriggersResponseitem>
    {
        public ManagePluginTriggersService(XrmRecordService service, XrmPackageSettings packageSettings)
        {
            Service = service;
            PackageSettings = packageSettings;
        }

        public XrmPackageSettings PackageSettings { get; set; }
        public XrmRecordService Service { get; set; }

        public override void ExecuteExtention(ManagePluginTriggersRequest request, ManagePluginTriggersResponse response, ServiceRequestController controller)
        {
            //delete any removed plugins
            var removedPlugins = request.GetSdkMessageStepsPre().Where(smsp => request.Triggers.All(pt => pt.Id != smsp.Id)).ToArray();
            var deletions = Service.DeleteInCrm(removedPlugins);
            response.AddResponseItems(deletions.Errors.Select(e => new ManagePluginTriggersResponseitem("Event Delete", e.Key.GetStringField(Fields.sdkmessageprocessingstep_.name), e.Value)));
            response.AddResponseItems(deletions.Deleted.Select(d => new ManagePluginTriggersResponseitem("Event Delete", d.GetStringField(Fields.sdkmessageprocessingstep_.name))));

            //load the filter entities which exist for each entity type, message combination
            var filters = request.Triggers.Select(t =>
            {
                var filter = new Filter();
                filter.AddCondition(Fields.sdkmessagefilter_.primaryobjecttypecode, ConditionType.Equal, t.RecordType == null ? "none" : t.RecordType.Key);
                filter.AddCondition(Fields.sdkmessagefilter_.sdkmessageid, ConditionType.Equal, t.Message.Id);
                return filter;
            }).ToArray();
            var pluginFilters = Service.RetrieveAllOrClauses(Entities.sdkmessagefilter, filters);

            //unload the triggers into an entity object referencing it in a dictionary
            var unloadedObjects = new Dictionary<IRecord, PluginTrigger>();
            foreach (var item in request.Triggers)
            {
                var matchingPluginFilters =
                    pluginFilters.Where(f => f.GetLookupId(Fields.sdkmessagefilter_.sdkmessageid) == item.Message.Id
                                             &&
                                             ((item.RecordType == null && f.GetStringField(Fields.sdkmessagefilter_.primaryobjecttypecode) == "none")
                                              ||
                                              (item.RecordType != null && f.GetStringField(Fields.sdkmessagefilter_.primaryobjecttypecode) == item.RecordType.Key)))
                                               .ToArray();
                var record = Service.NewRecord(Entities.sdkmessageprocessingstep);
                var name = string.Format("{0} {1} {2} {3} {4}", item.Plugin, item.RecordType?.Key ?? "none", item.Message, item.Stage, item.Mode).Left(Service.GetMaxLength(Fields.sdkmessageprocessingstep_.name, Entities.sdkmessageprocessingstep));
                try
                {
                    if (!matchingPluginFilters.Any())
                        response.AddResponseItem(new ManagePluginTriggersResponseitem("Warning", name, new NullReferenceException($"No Matching {Service.GetDisplayName(Entities.sdkmessagefilter)} Could Be Found For The Trigger Configuration")));
                        

                    record.SetField(Fields.sdkmessageprocessingstep_.name, name, Service);
                    record.SetField(Fields.sdkmessageprocessingstep_.rank, item.Rank, Service);
                    if (item.Stage != null)
                        record.SetField(Fields.sdkmessageprocessingstep_.stage, (int)item.Stage, Service);
                    if (item.Mode != null)
                        record.SetField(Fields.sdkmessageprocessingstep_.mode, (int)item.Mode, Service);
                    record.SetField(Fields.sdkmessageprocessingstep_.plugintypeid, item.Plugin, Service);
                    record.SetField(Fields.sdkmessageprocessingstep_.sdkmessagefilterid, !matchingPluginFilters.Any() ? null : matchingPluginFilters.First().ToLookup(), Service);
                    record.SetField(Fields.sdkmessageprocessingstep_.sdkmessageid, item.Message, Service);
                    record.SetField(Fields.sdkmessageprocessingstep_.impersonatinguserid, item.SpecificUserContext == null || item.SpecificUserContext.Id == null ? null : item.SpecificUserContext, Service);
                    if (item.Id != null)
                        record.SetField(Fields.sdkmessageprocessingstep_.sdkmessageprocessingstepid, item.Id, Service);
                    record.SetField(Fields.sdkmessageprocessingstep_.asyncautodelete, item.Mode == PluginTrigger.PluginMode.Asynch, Service);
                    record.SetField(Fields.sdkmessageprocessingstep_.filteringattributes, item.FilteringFields != null && item.FilteringFields.Any() ? string.Join(",", item.FilteringFields.OrderBy(r => r.Key).Select(r => r.Key)) : null, Service);
                    unloadedObjects.Add(record, item);
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new ManagePluginTriggersResponseitem("Unload Error", name, ex));
                }
            }

            //submit them to crm create/update
            var triggerLoads = Service.LoadIntoCrm(unloadedObjects.Keys,
                Fields.sdkmessageprocessingstep_.sdkmessageprocessingstepid);

            foreach(var e in triggerLoads.Errors)
            {
                response.AddResponseItem(new ManagePluginTriggersResponseitem(e.Key.Id != null ? "Event Update" : "Event Create", e.Key.GetStringField(Fields.sdkmessageprocessingstep_.name), e.Value));
            }
            response.AddResponseItems(triggerLoads.Created.Select(d => new ManagePluginTriggersResponseitem("Event Create", d.GetStringField(Fields.sdkmessageprocessingstep_.name))));
            response.AddResponseItems(triggerLoads.Updated.Select(d => new ManagePluginTriggersResponseitem("Event Update", d.GetStringField(Fields.sdkmessageprocessingstep_.name))));

            var updatesAndDeletes =
                unloadedObjects.Keys.Where(
                    r =>
                        new[] { "Update", "Delete" }.Contains(
                            r.GetLookupName(Fields.sdkmessageprocessingstep_.sdkmessageid)))
                            .ToArray();

            var solutionItemsToAdd = new List<string>();
            //update/delete pre-images
            var imagesToCreateOrUpdate = new List<IRecord>();
            var imagesToDelete = new List<IRecord>();
            foreach (var item in updatesAndDeletes
                .Where(i => !triggerLoads.Errors.Keys.Contains(i)))
            {
                var matchingPluginTrigger = unloadedObjects[item];
                //the plugin will only have an image if all fields, or there are specific fields selected
                var hasImage = matchingPluginTrigger.PreImageAllFields || (matchingPluginTrigger.PreImageFields != null && matchingPluginTrigger.PreImageFields.Any());
                if (!hasImage)
                {
                    //delete the existing image if is has been changed to not have an image
                    if (matchingPluginTrigger.PreImageId != null)
                    {
                        try
                        {
                            imagesToDelete.Add(Service.Get(Entities.sdkmessageprocessingstepimage, matchingPluginTrigger.PreImageId));
                            solutionItemsToAdd.Add(matchingPluginTrigger.Id);
                        }
                        catch (Exception ex)
                        {
                            response.AddResponseItem(new ManagePluginTriggersResponseitem("Pre Image Delete", matchingPluginTrigger.Id, ex));
                        }
                    }
                }
                else
                {
                    try
                    {
                        //set the details to create/update in the pre-image
                        var isUpdate = triggerLoads.Updated.Contains(item);
                        var imageRecord = Service.NewRecord(Entities.sdkmessageprocessingstepimage);
                        imageRecord.Id = matchingPluginTrigger.PreImageId;
                        if (matchingPluginTrigger.PreImageId != null)
                            imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepimageid, matchingPluginTrigger.PreImageId, Service);
                        if (matchingPluginTrigger.PreImageIdUnique != null)
                            imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepimageidunique, matchingPluginTrigger.PreImageIdUnique, Service);

                        imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.name, matchingPluginTrigger.PreImageName, Service);
                        imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.entityalias, matchingPluginTrigger.PreImageName, Service);
                        imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.messagepropertyname, "Target", Service);
                        imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, Service.ToLookup(item), Service);
                        imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.imagetype, OptionSets.SdkMessageProcessingStepImage.ImageType.PreImage, Service);
                        var attributesString = matchingPluginTrigger.PreImageAllFields || matchingPluginTrigger.PreImageFields == null || !matchingPluginTrigger.PreImageFields.Any()
                            ? null
                            : string.Join(",", matchingPluginTrigger.PreImageFields.Select(f => f.Key).OrderBy(s => s).ToArray());
                        imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.attributes, attributesString, Service);
                        imagesToCreateOrUpdate.Add(imageRecord);
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new ManagePluginTriggersResponseitem("Image Error", item.GetStringField(Fields.sdkmessageprocessingstep_.name), ex));
                    }
                }
            }

            //submit create/update/deletion of pre-images
            var imageLoads = Service.LoadIntoCrm(imagesToCreateOrUpdate,
                Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepimageidunique);
            foreach (var e in imageLoads.Errors)
            {
                response.AddResponseItem(new ManagePluginTriggersResponseitem(e.Key.Id != null ? "Pre Image Update" : "Pre Image Create", e.Key.GetLookupName(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid), e.Value));
            }
            response.AddResponseItems(imageLoads.Created.Select(d => new ManagePluginTriggersResponseitem("Pre Image Create", d.GetLookupName(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid))));
            response.AddResponseItems(imageLoads.Updated.Select(d => new ManagePluginTriggersResponseitem("Pre Image Update", d.GetLookupName(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid))));


            var imageDeletions = Service.DeleteInCrm(imagesToDelete);
            response.AddResponseItems(imageDeletions.Errors.Select(e => new ManagePluginTriggersResponseitem("Pre Image Delete", e.Key.GetLookupName(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid), e.Value)));

            //add plugin steps to the solution
            var componentType = OptionSets.SolutionComponent.ObjectTypeCode.SDKMessageProcessingStep;
            solutionItemsToAdd.AddRange(triggerLoads.Created.Union(triggerLoads.Updated).Select(r => r.Id).ToList());
            var imagesReferences = imageLoads.Created.Union(imageLoads.Updated)
                .Select(i => i.GetLookupId(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid))
                .Where(id => !string.IsNullOrWhiteSpace(id));
            solutionItemsToAdd.AddRange(imagesReferences);

            if (PackageSettings.AddToSolution)
                Service.AddSolutionComponents(PackageSettings.Solution.Id, componentType, solutionItemsToAdd);

            if (response.HasResponseItemError)
                response.Message = "There Were Errors Thrown Updating The Plugins";
            else if (!response.HasResponseItems)
                response.Message = "No Updates Were Identified";
            else
                response.Message = "Plugins Updated";
        }
    }
}