using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using $safeprojectname$.Xrm;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Schema;

namespace $safeprojectname$.Extentions
{
    public static class XrmServiceExtentions
    {
        /// <summary>
        /// Returns list of key values giving the types and field name parsed for the given string of field joins
        /// key = type, value = field
        /// </summary>
        /// <param name="xrmService"></param>
        /// <param name="fieldPath"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string,string>> GetTypeFieldPath(this XrmService xrmService, string fieldPath, string sourceType)
        {

            var list = new List<KeyValuePair<string, string>>();
            var splitOutFunction = fieldPath.Split(':');
            if (splitOutFunction.Count() > 1)
                fieldPath = splitOutFunction.ElementAt(1);
            var split = fieldPath.Split('.');
            var currentType = sourceType;
            list.Add(new KeyValuePair<string, string>(currentType, split.ElementAt(0).Split('|').First()));
            var i = 1;
            if (split.Length > 1)
            {
                foreach (var item in split.Skip(1).Take(split.Length - 1))
                {
                    var fieldName = item.Split('|').First();
                    if (split.ElementAt(i - 1).Contains("|"))
                    {
                        var targetType = split.ElementAt(i - 1).Split('|').Last();
                        list.Add(new KeyValuePair<string, string>(targetType, fieldName));
                        currentType = targetType;
                    }
                    else
                    {
                        var targetType = xrmService.GetLookupTargetEntity(list.ElementAt(i - 1).Value, currentType);
                        list.Add(new KeyValuePair<string, string>(targetType, fieldName));
                        currentType = targetType;
                    }
                    i++;
                }
            }
            return list;
        }

        /// <summary>
        /// Returns a query containing all the fields, and required joins for all the given fields
        /// field examples are "did_contactid.firstname" or "customerid|contact.lastname"
        public static QueryExpression BuildSourceQuery(this XrmService xrmService, string sourceType, IEnumerable<string> fields)
        {
            var query = XrmService.BuildQuery(sourceType, new string[0], null, null);
            foreach (var field in fields)
            {
                xrmService.AddRequiredQueryJoins(query, field);
            }
            return query;
        }

        public static void AddRequiredQueryJoins(this XrmService xrmService, QueryExpression query, string source)
        {
            var typeFieldPaths = xrmService.GetTypeFieldPath(source, query.EntityName);
            var splitOutFunction = source.Split(':');
            if (splitOutFunction.Count() > 1)
                source = splitOutFunction.ElementAt(1);
            var splitTokens = source.Split('.');
            if (typeFieldPaths.Count() == 1)
                query.ColumnSet.AddColumn(typeFieldPaths.First().Value);
            else
            {
                LinkEntity thisLink = null;

                for (var i = 0; i < typeFieldPaths.Count() - 1; i++)
                {
                    var lookupField = typeFieldPaths.ElementAt(i).Value;
                    var path = string.Join(".", splitTokens.Take(i + 1)).Replace("|","_");
                    if (i == 0)
                    {
                        var targetType = typeFieldPaths.ElementAt(i + 1).Key;
                        var matchingLinks = query.LinkEntities.Where(le => le.EntityAlias == path);

                        if (matchingLinks.Any())
                            thisLink = matchingLinks.First();
                        else
                        {
                            thisLink = query.AddLink(targetType, lookupField, xrmService.GetPrimaryKeyField(targetType), JoinOperator.LeftOuter);
                            thisLink.EntityAlias = path;
                            thisLink.Columns = xrmService.CreateColumnSet(new string[0]);
                        }
                    }
                    else
                    {
                        var targetType = xrmService.GetLookupTargetEntity(lookupField, thisLink.LinkToEntityName);
                        var matchingLinks = thisLink.LinkEntities.Where(le => le.EntityAlias == path);
                        if (matchingLinks.Any())
                            thisLink = matchingLinks.First();
                        else
                        {
                            thisLink = thisLink.AddLink(targetType, lookupField, xrmService.GetPrimaryKeyField(targetType), JoinOperator.LeftOuter);
                            thisLink.EntityAlias = path;
                            thisLink.Columns = xrmService.CreateColumnSet(new string[0]);

                        }

                    }
                }
                thisLink.Columns.AddColumn(typeFieldPaths.ElementAt(typeFieldPaths.Count() - 1).Value);
            }
        }


        public static string GetDisplayLabel(this XrmService xrmService, Entity targetObject, string token)
        {
            var fieldPaths = xrmService.GetTypeFieldPath(token, targetObject.LogicalName);
            var thisFieldType = fieldPaths.Last().Key;
            var thisFieldName = fieldPaths.Last().Value;
            var displayString = xrmService.GetFieldLabel(thisFieldName, thisFieldType);
            return displayString;
        }
        public static string GetDisplayString(this XrmService xrmService, Entity targetObject, string token, bool isHtml = false)
        {
            var fieldPaths = xrmService.GetTypeFieldPath(token, targetObject.LogicalName);
            var thisFieldType = fieldPaths.Last().Key;
            var thisFieldName = fieldPaths.Last().Value;
            string func = null;
            var getFieldString = token.Replace("|", "_");
            var splitFunc = getFieldString.Split(':');
            if (splitFunc.Count() > 1)
            {
                func = splitFunc.First();
                getFieldString = splitFunc.ElementAt(1);
            }
            var displayString = xrmService.GetFieldAsDisplayString(thisFieldType, thisFieldName, targetObject.GetFieldValue(getFieldString), isHtml: isHtml, func: func);
            return displayString;
        }

        public static string GenerateEmailContent(this XrmService xrmService, string emailTemplateResourceName, string emailTemplateTargetType, Guid emailTemplateTargetId, string crmurl = null, string appendAppIdToCrmUrl = null)
        {
            string activityDescription = null;
            var targetTokens = new List<string>();
            var staticTokens = new Dictionary<string, List<string>>();
            var ifTokens = new List<string>();
            var staticIdentifier = "STATIC|";
            var ifIdentifier = "IF|";
            var endifIdentifier = "ENDIF";

            if (emailTemplateResourceName != null)
            {
                var resource = xrmService.GetFirst(Entities.webresource, Fields.webresource_.name, emailTemplateResourceName);
                if (resource == null)
                    throw new NullReferenceException(string.Format("Could Not Find {0} With {1} '{2}'", xrmService.GetEntityLabel(Entities.webresource), xrmService.GetFieldLabel(Fields.webresource_.name, Entities.webresource), emailTemplateResourceName));
                var encoded = resource.GetStringField(Fields.webresource_.content);
                byte[] binary = Convert.FromBase64String(encoded);
                activityDescription = UnicodeEncoding.UTF8.GetString(binary);

                if (appendAppIdToCrmUrl != null)
                {
                    var baseUrlPart = "crminstanceurl]/main.aspx?";
                    activityDescription = activityDescription.Replace(baseUrlPart, baseUrlPart + "appid=" + appendAppIdToCrmUrl + "&");
                }

                //parse out all tokens inside [] chars to replace in the email

                var i = 0;
                while (i < activityDescription.Length)
                {
                    if (activityDescription[i] == '[')
                    {
                        var startIndex = i;
                        while (i < activityDescription.Length)
                        {
                            if (activityDescription[i] == ']')
                            {
                                var endIndex = i;
                                var token = activityDescription.Substring(startIndex + 1, endIndex - startIndex - 1);

                                if (token.ToUpper().StartsWith(ifIdentifier) || token.ToUpper().StartsWith(endifIdentifier))
                                {
                                    ifTokens.Add(token);
                                }
                                else if (token.ToUpper().StartsWith(staticIdentifier))
                                {
                                    token = token.Substring(staticIdentifier.Length);
                                    var split = token.Split('.');
                                    if (split.Count() != 2)
                                        throw new Exception(string.Format("The static token {0} is not formatted as expected. It should be of the form type.field", token));
                                    var staticType = split.First();
                                    var staticField = split.ElementAt(1);
                                    if (!staticTokens.ContainsKey(staticType))
                                        staticTokens.Add(staticType, new List<string>());
                                    staticTokens[staticType].Add(staticField);
                                }

                                else
                                    targetTokens.Add(token);
                                break;
                            }
                            i++;
                        }
                    }
                    else
                        i++;
                }
            }

            //query to get all the fields for replacing tokens
            var query = xrmService.BuildSourceQuery(emailTemplateTargetType, targetTokens);
            query.Criteria.AddCondition(new ConditionExpression(xrmService.GetPrimaryKeyField(emailTemplateTargetType), ConditionOperator.Equal, emailTemplateTargetId));
            var targetObject = xrmService.RetrieveFirst(query);

            //process all the ifs (clear where not)
            while (ifTokens.Any())
            {
                var endIfTokenStackCount = 0;
                var removeAll = false;
                var token = ifTokens.First();
                if (token.ToUpper() != endifIdentifier)
                {
                    var tokenIndex = activityDescription.IndexOf(token);
                    var indexOf = token.IndexOf("|");
                    if (indexOf > -1)
                    {
                        var field = token.Substring(indexOf + 1);
                        var fieldValue = targetObject.GetFieldValue(field);
                        var endIfTokenStack = 1;
                        var remainingTokens = ifTokens.Skip(1).ToList();
                        while (true && remainingTokens.Any())
                        {
                            if (remainingTokens.First().ToUpper() == endifIdentifier)
                            {
                                endIfTokenStack--;
                                endIfTokenStackCount++;
                            }
                            else
                            {
                                endIfTokenStack++;
                            }
                            remainingTokens.RemoveAt(0);
                            if (endIfTokenStack == 0)
                                break;
                        }
                        //okay so starting at the current index need to find the end if
                        //and remove the content or the tokens
                        var currentStack = endIfTokenStackCount;
                        var currentIndex = activityDescription.IndexOf(token);
                        while (currentStack > 0)
                        {
                            var endIfIndex = activityDescription.IndexOf(endifIdentifier, currentIndex, StringComparison.OrdinalIgnoreCase);
                            if (endIfIndex > -1)
                            {
                                currentIndex = endIfIndex;
                                currentStack--;
                            }
                            else
                                break;
                        }
                        removeAll = fieldValue == null;
                        if (removeAll)
                        {
                            var startRemove = tokenIndex - 1;
                            var endRemove = currentIndex + endifIdentifier.Length + 1;
                            activityDescription = activityDescription.Substring(0, startRemove) + activityDescription.Substring(endRemove);
                        }
                        else
                        {
                            var startRemove = tokenIndex - 1;
                            var endRemove = currentIndex - 1;
                            activityDescription = activityDescription.Substring(0, startRemove)
                                + activityDescription.Substring(startRemove + token.Length + 2, endRemove - startRemove - token.Length - 2)
                                + activityDescription.Substring(endRemove + endifIdentifier.Length + 2);
                        }
                    }
                }
                ifTokens.RemoveRange(0, endIfTokenStackCount > 0 ? endIfTokenStackCount * 2 : 1);
            }

            //replace all the tokens
            foreach (var token in targetTokens)
            {
                var sourceType = emailTemplateTargetType;
                string displayString = xrmService.GetDisplayString(targetObject, token, isHtml: true);
                activityDescription = activityDescription.Replace("[" + token + "]", displayString);
            }

            foreach (var staticTargetTokens in staticTokens)
            {
                var staticType = staticTargetTokens.Key;
                var staticFields = staticTargetTokens.Value;

                //query to get all the fields for replacing tokens
                var staticQuery = xrmService.BuildSourceQuery(staticType, staticFields);
                var staticTarget = xrmService.RetrieveFirst(staticQuery);

                //replace all the tokens
                foreach (var staticField in staticFields)
                {
                    string staticFunc = null;
                    activityDescription = activityDescription.Replace("[STATIC|" + string.Format("{0}.{1}", staticType, staticField) + "]", xrmService.GetFieldAsDisplayString(staticType, staticField, staticTarget.GetFieldValue(staticField), isHtml: true, func: staticFunc));
                }
            }
            return activityDescription;
        }
    }
}
