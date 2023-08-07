using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.SolutionComponentExporter.Type;
using JosephM.Spreadsheet;
using JosephM.Xrm.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JosephM.SolutionComponentExporter
{
    public class SolutionComponentExporterService :
        ServiceBase<SolutionComponentExporterRequest, SolutionComponentExporterResponse, SolutionComponentExporterResponseItem>
    {
        public SolutionComponentExporterService(XrmRecordService service)
        {
            Service = service;
        }

        private XrmRecordService Service { get; set; }

        public override void ExecuteExtention(SolutionComponentExporterRequest request,
            SolutionComponentExporterResponse response,
            ServiceRequestController controller)
        {
            response.Folder = request.SaveToFolder.FolderPath;

            var componentList = Process(request, response, controller.Controller);

            response.Folder = request.SaveToFolder.FolderPath;

            var excelFileName = "Solution Component Export " + DateTime.Now.ToFileTime() + ".xlsx";
            ExcelUtility.CreateXlsx(request.SaveToFolder.FolderPath, excelFileName, new Dictionary<string, IEnumerable>
            {
                { "Components", componentList }
            });
            response.ExcelFileName = excelFileName;

            response.Message = "The Export is Complete";
        }

        private IEnumerable<SolutionComponentOutput> Process(SolutionComponentExporterRequest request, SolutionComponentExporterResponse response,
            LogController controller)
        {
            var allComponents = new List<SolutionComponentOutput>();

            var solutionId = request.Solution.Id;
            controller.LogLiteral("Loading Solution Components");
            var solutionComponents = Service.RetrieveAllAndClauses(Entities.solutioncomponent, new[]
            {
                new Condition(Fields.solutioncomponent_.solutionid, ConditionType.Equal, solutionId)
            });
            controller.LogLiteral("Loading Metadata");
            var entitySolutionComponentIds = solutionComponents
                .Where(sc => sc.GetOptionKey(Fields.solutioncomponent_.componenttype) == OptionSets.Shared.ComponentType.Entity.ToString())
                .Select(sc => sc.GetField(Fields.solutioncomponent_.objectid)?.ToString())
                .ToArray();
            var recordTypesMetadata = Service
                .GetAllRecordTypes()
                .Select(r => Service.GetRecordTypeMetadata(r))
                .Where(m => entitySolutionComponentIds.Contains(m.MetadataId))
                .ToArray();
            var fieldMetadata = recordTypesMetadata
                .Select(rt => Service.GetFieldMetadata(rt.SchemaName))
                .SelectMany(fm => fm)
                .ToDictionary(fm => fm.MetadataId, fm => fm);
            var optionSetSolutionComponentIds = solutionComponents
                .Where(sc => sc.GetOptionKey(Fields.solutioncomponent_.componenttype) == OptionSets.Shared.ComponentType.OptionSet.ToString())
                .Select(sc => sc.GetField(Fields.solutioncomponent_.objectid)?.ToString())
                .ToArray();
            var optionSetMetadata = optionSetSolutionComponentIds.Any()
                ? Service
                .GetSharedPicklists()
                .Where(m => optionSetSolutionComponentIds.Contains(m.MetadataId))
                .ToArray()
                : new IPicklistSet[0];
            var countToDo = solutionComponents.Count();
            var countDone = 0;
            foreach (var solutionComponent in solutionComponents)
            {
                countDone++;
                controller.UpdateProgress(countDone, countToDo, "Processing Solution Components");
                var solutionComponentOutput = new SolutionComponentOutput
                {
                    ComponentTypeKey = solutionComponent.GetOptionKey(Fields.solutioncomponent_.componenttype),
                    ComponentId = solutionComponent.GetField(Fields.solutioncomponent_.objectid)?.ToString(),
                    ComponentType = Service.GetPicklistLabel(Fields.solutioncomponent_.componenttype, Entities.solutioncomponent, solutionComponent.GetOptionKey(Fields.solutioncomponent_.componenttype))
                };
                allComponents.Add(solutionComponentOutput);
                if (int.TryParse(solutionComponentOutput.ComponentTypeKey, out int componentTypeInt))
                {
                    switch(componentTypeInt)
                    {
                        case 80:
                            {
                                solutionComponentOutput.ComponentType = "Model Driven App";
                                var appModule = Service.Get(Entities.appmodule, solutionComponentOutput.ComponentId, new[] { Fields.appmodule_.name });
                                solutionComponentOutput.ComponentDisplayName = appModule.GetStringField(Fields.appmodule_.name);
                                break;
                            }
                        case OptionSets.Shared.ComponentType.Attribute:
                            {
                                if(fieldMetadata.ContainsKey(solutionComponentOutput.ComponentId))
                                {
                                    solutionComponentOutput.ComponentDisplayName = fieldMetadata[solutionComponentOutput.ComponentId].DisplayName;
                                    solutionComponentOutput.TableDisplayName = Service.GetDisplayName(fieldMetadata[solutionComponentOutput.ComponentId].RecordType);
                                }
                                break;
                            }
                        case OptionSets.Shared.ComponentType.Entity:
                            {
                                var matchingMetadata = recordTypesMetadata
                                    .Where(rtm => rtm.MetadataId == solutionComponentOutput.ComponentId)
                                    .ToArray();
                                if (matchingMetadata.Any())
                                {
                                    solutionComponentOutput.ComponentDisplayName = matchingMetadata.First().DisplayName;
                                }
                                break;
                            }
                        case OptionSets.Shared.ComponentType.EntityRelationship:
                            {
                                var rootComponentId = solutionComponent.GetIdField(Fields.solutioncomponent_.rootsolutioncomponentid);
                                if(rootComponentId != null)
                                {
                                    var matchingRootComponents = solutionComponents
                                        .Where(sc => sc.Id == rootComponentId && sc.GetOptionKey(Fields.solutioncomponent_.componenttype) == OptionSets.Shared.ComponentType.Entity.ToString())
                                        .ToArray();
                                    if(matchingRootComponents.Any())
                                    {
                                        var matchingMetadata = recordTypesMetadata
                                            .Where(rtm => rtm.MetadataId == matchingRootComponents.First().GetIdField(Fields.solutioncomponent_.objectid))
                                            .ToArray();
                                        var objectMetadataId = solutionComponent.GetIdField(Fields.solutioncomponent_.objectid);
                                        if (matchingMetadata.Any())
                                        {
                                            var entityMetadata = matchingMetadata.First();
                                            solutionComponentOutput.TableDisplayName = entityMetadata.DisplayName;
                                            var matchingOneToManyRelationship = Service
                                                .GetOneToManyRelationships(entityMetadata.SchemaName, onlyValidForAdvancedFind: false)
                                                .Where(r => r.MetadataId == objectMetadataId)
                                                .ToArray();
                                            if(matchingOneToManyRelationship.Any())
                                            {
                                                solutionComponentOutput.ComponentType = "One to Many Relatipnship";
                                                solutionComponentOutput.ComponentDisplayName = matchingOneToManyRelationship.First().SchemaName;
                                            }
                                            else
                                            {
                                                var matchingManyToOneRelationship = Service
                                                    .GetManyToOneRelationships(entityMetadata.SchemaName, onlyValidForAdvancedFind: false)
                                                    .Where(r => r.MetadataId == objectMetadataId)
                                                    .ToArray();
                                                if (matchingManyToOneRelationship.Any())
                                                {
                                                    solutionComponentOutput.ComponentType = "Many to One Relatipnship";
                                                    solutionComponentOutput.ComponentDisplayName = matchingManyToOneRelationship.First().SchemaName;
                                                }
                                                else
                                                {
                                                    var matchingManyToManyRelationship = Service
                                                        .GetManyToManyRelationships(entityMetadata.SchemaName)
                                                        .Where(r => r.MetadataId == objectMetadataId)
                                                        .ToArray();
                                                    if (matchingManyToManyRelationship.Any())
                                                    {
                                                        solutionComponentOutput.ComponentType = "Many to Many Relatipnship";
                                                        solutionComponentOutput.ComponentDisplayName = matchingManyToManyRelationship.First().SchemaName;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case OptionSets.Shared.ComponentType.OptionSet:
                            {
                                var matchingMetadata = optionSetMetadata
                                     .Where(rtm => rtm.MetadataId == solutionComponentOutput.ComponentId)
                                     .ToArray();
                                if (matchingMetadata.Any())
                                {
                                    solutionComponentOutput.ComponentDisplayName = matchingMetadata.First().DisplayName;
                                }
                                break;
                            }
                        case OptionSets.Shared.ComponentType.PluginAssembly:
                            {
                                var pluginAssembly = Service.Get(Entities.pluginassembly, solutionComponentOutput.ComponentId, new[] { Fields.pluginassembly_.name });
                                solutionComponentOutput.ComponentDisplayName = pluginAssembly.GetStringField(Fields.pluginassembly_.name);
                                break;
                            }
                        case OptionSets.Shared.ComponentType.SavedQuery:
                            {
                                solutionComponentOutput.ComponentType = "System View";
                                var savedQuery = Service.Get(Entities.savedquery, solutionComponentOutput.ComponentId, new[] { Fields.savedquery_.name, Fields.savedquery_.returnedtypecode });
                                solutionComponentOutput.ComponentDisplayName = savedQuery.GetStringField(Fields.savedquery_.name);
                                if (savedQuery.GetField(Fields.savedquery_.returnedtypecode) is string)
                                {
                                    solutionComponentOutput.TableDisplayName = Service.GetDisplayName(savedQuery.GetStringField(Fields.savedquery_.returnedtypecode));
                                }
                                break;
                            }
                        case OptionSets.Shared.ComponentType.Role:
                            {
                                var role = Service.Get(Entities.role, solutionComponentOutput.ComponentId, new[] { Fields.role_.name });
                                solutionComponentOutput.ComponentDisplayName = role.GetStringField(Fields.role_.name);
                                break;
                            }
                        case OptionSets.Shared.ComponentType.SiteMap:
                            {
                                var sitemap = Service.Get(Entities.sitemap, solutionComponentOutput.ComponentId, new[] { Fields.sitemap_.sitemapname });
                                solutionComponentOutput.ComponentDisplayName = sitemap.GetStringField(Fields.sitemap_.sitemapname);
                                break;
                            }
                        case OptionSets.Shared.ComponentType.SystemForm:
                            {
                                var systemForm = Service.Get(Entities.systemform, solutionComponentOutput.ComponentId, new[] { Fields.systemform_.name, Fields.systemform_.objecttypecode, Fields.systemform_.type });
                                solutionComponentOutput.ComponentType = Service.GetPicklistLabel(Fields.systemform_.type, Entities.systemform, systemForm.GetOptionKey(Fields.systemform_.type));
                                if(solutionComponentOutput.ComponentType == "Main")
                                {
                                    solutionComponentOutput.ComponentType = "Form";
                                }
                                solutionComponentOutput.ComponentDisplayName = systemForm.GetStringField(Fields.systemform_.name);
                                if (systemForm.GetField(Fields.systemform_.objecttypecode) is string objectTypeCode
                                    && objectTypeCode != "none")
                                {
                                    solutionComponentOutput.TableDisplayName = Service.GetDisplayName(objectTypeCode);
                                }
                                break;
                            }
                        case OptionSets.Shared.ComponentType.WebResource:
                            {
                                var webResource = Service.Get(Entities.webresource, solutionComponentOutput.ComponentId, new[] { Fields.webresource_.name });
                                solutionComponentOutput.ComponentDisplayName = webResource.GetStringField(Fields.webresource_.name);
                                break;
                            }
                        case OptionSets.Shared.ComponentType.Workflow:
                            {
                                var workflow = Service.Get(Entities.workflow, solutionComponentOutput.ComponentId, new[] { Fields.workflow_.name, Fields.workflow_.primaryentity, Fields.workflow_.category });
                                solutionComponentOutput.ComponentType = Service.GetPicklistLabel(Fields.workflow_.category, Entities.workflow, workflow.GetOptionKey(Fields.workflow_.category));
                                solutionComponentOutput.ComponentDisplayName = workflow.GetStringField(Fields.systemform_.name);
                                if (workflow.GetField(Fields.workflow_.primaryentity) is string objectTypeCode
                                    && objectTypeCode != "none")
                                {
                                    solutionComponentOutput.TableDisplayName = Service.GetDisplayName(objectTypeCode);
                                }
                                break;
                            }
                    }
                }
            }
            return allComponents
                .OrderBy(c => c.ComponentType)
                .ThenBy(c => c.ComponentDisplayName)
                .ToArray();
        }
    }
}