using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.InstanceComparer
{
    public static class Extensions
    {
        public static IEnumerable<AppComponent> GetAppComponents(this IRecordService recordService, string appName)
        {
            var appComponents = new List<AppComponent>();

            AddSitemap(recordService, appName, appComponents);
            AddComponents("Business Process Flow", Entities.workflow, Fields.workflow_.name, Fields.workflow_.primaryentity, OptionSets.AppModuleComponent.ObjectTypeCode.BusinessProcessFlows, appName, appComponents, recordService);
            AddEntities(recordService, appName, appComponents);
            AddComponents("Chart", Entities.savedqueryvisualization, Fields.savedqueryvisualization_.name, Fields.savedqueryvisualization_.primaryentitytypecode, OptionSets.AppModuleComponent.ObjectTypeCode.Charts, appName, appComponents, recordService);
            AddComponents((r) => new string[] {
                OptionSets.SystemForm.FormType.Dashboard.ToString(),
                OptionSets.SystemForm.FormType.InteractionCentricDashboard.ToString() }.Contains(r.GetOptionKey(Fields.systemform_.type))
            ? "Dashboard"
            : "Form", Entities.systemform, Fields.systemform_.name, Fields.systemform_.objecttypecode, OptionSets.AppModuleComponent.ObjectTypeCode.Forms, appName, appComponents, recordService);
            AddComponents("View", Entities.savedquery, Fields.savedquery_.name, Fields.savedquery_.returnedtypecode, OptionSets.AppModuleComponent.ObjectTypeCode.Views, appName, appComponents, recordService);



            return appComponents;
        }

        private static void AddSitemap(IRecordService recordService, string appName, List<AppComponent> appComponents)
        {
            var query = new QueryDefinition(Entities.appmodulecomponent);
            query.RootFilter.AddCondition(Fields.appmodulecomponent_.componenttype, ConditionType.Equal, OptionSets.AppModuleComponent.ObjectTypeCode.Sitemap);
            var siteMapJoin = new Join(Fields.appmodulecomponent_.objectid, Entities.sitemap, Fields.sitemap_.sitemapid);
            siteMapJoin.Fields = new[] { Fields.sitemap_.sitemapxml };
            siteMapJoin.Alias = "SM";
            query.Joins.Add(siteMapJoin);
            var appJoin = new Join(Fields.appmodulecomponent_.appmoduleidunique, Entities.appmodule, Fields.appmodule_.appmoduleidunique);
            appJoin.RootFilter.Conditions.Add(new Condition(Fields.appmodule_.name, ConditionType.Equal, appName));
            query.Joins.Add(appJoin);
            var siteMaps = recordService.RetreiveAll(query);
            foreach (var siteMap in siteMaps)
            {
                appComponents.Add(new AppComponent("Sitemap", null, siteMapXml: siteMap.GetStringField("SM." + Fields.sitemap_.sitemapxml)));
            }
        }

        private static void AddEntities(IRecordService recordService, string appName, List<AppComponent> appComponents)
        {
            //Entities cannot use generic query so match on metadata id
            var entityComponents = GetComponents(OptionSets.AppModuleComponent.ObjectTypeCode.Entities, appName, recordService);
            if (entityComponents.Any())
            {
                var typeMetadata = recordService
                        .GetAllRecordTypes()
                        .Select(rt => recordService.GetRecordTypeMetadata(rt))
                        .GroupBy(mt => mt.MetadataId)
                        .ToDictionary(g => g.Key, g => g.ToArray().First());
                foreach (var entity in entityComponents)
                {
                    if (typeMetadata.ContainsKey(entity.GetStringField(Fields.appmodulecomponent_.objectid)))
                    {
                        appComponents.Add(new AppComponent("Entity", typeMetadata[entity.GetStringField(Fields.appmodulecomponent_.objectid)].SchemaName));
                    }
                    else
                    {
                        throw new Exception($"Couldnt Find Record Type With Metadata ID {entity.GetStringField(Fields.appmodulecomponent_.objectid)}");
                    }
                }
            }
        }

        private static IEnumerable<IRecord> GetComponents(int componentType, string appName, IRecordService recordService)
        {
            var query = new QueryDefinition(Entities.appmodulecomponent);
            query.RootFilter.Conditions.Add(new Condition(Fields.appmodulecomponent_.componenttype, ConditionType.Equal, componentType));
            var appJoin = new Join(Fields.appmodulecomponent_.appmoduleidunique, Entities.appmodule, Fields.appmodule_.appmoduleidunique);
            appJoin.RootFilter.Conditions.Add(new Condition(Fields.appmodule_.name, ConditionType.Equal, appName));
            query.Joins.Add(appJoin);
            return recordService.RetreiveAll(query);
        }

        private static void AddComponents(string displayType, string recordType, string matchField, string recordTypeField, int componentType, string appName, List<AppComponent> appComponents, IRecordService recordService)
        {
            AddComponents((r) => displayType, recordType, matchField, recordTypeField, componentType, appName, appComponents, recordService);
        }

        private static void AddComponents(Func<IRecord, string> getDisplayType, string recordType, string matchField, string recordTypeField, int componentType, string appName, List<AppComponent> appComponents, IRecordService recordService)
        {
            var query = new QueryDefinition(recordType);
            var componentJoin = new Join(recordService.GetPrimaryKey(recordType), Entities.appmodulecomponent, Fields.appmodulecomponent_.objectid);
            componentJoin.RootFilter.Conditions.Add(new Condition(Fields.appmodulecomponent_.componenttype, ConditionType.Equal, componentType));
            query.Joins.Add(componentJoin);
            var appJoin = new Join(Fields.appmodulecomponent_.appmoduleidunique, Entities.appmodule, Fields.appmodule_.appmoduleidunique);
            appJoin.RootFilter.Conditions.Add(new Condition(Fields.appmodule_.name, ConditionType.Equal, appName));
            componentJoin.Joins.Add(appJoin);
            var theseOnes = recordService.RetreiveAll(query);

            appComponents.AddRange(theseOnes.Select(e => new AppComponent(getDisplayType(e), e.GetStringField(matchField), e.GetStringField(recordTypeField))).ToArray());
        }

        public class AppComponent
        {
            public AppComponent(string type, string name, string relatedRecordType = null, string siteMapXml = null)
            {
                Type = type;
                Name = name;
                RelatedRecordType = relatedRecordType;
                SiteMapXml = siteMapXml;
            }

            public string Name { get; set; }
            public string RelatedRecordType { get; }
            public string SiteMapXml { get; }
            public string Type { get; set; }

            public string CompareString { get { return $"{Type}{((RelatedRecordType ?? "none") == "none" ? "" : ($" - {RelatedRecordType}"))}{(Name == null ? "" : ($" - {Name}"))}"; } }
        }
    }
}
