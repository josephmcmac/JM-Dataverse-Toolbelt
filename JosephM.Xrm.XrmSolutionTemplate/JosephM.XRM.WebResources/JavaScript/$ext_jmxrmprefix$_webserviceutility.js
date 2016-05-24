/// <reference path="template_jquery.js" />

//Note object instantiated after function definition

if (typeof Xrm === 'undefined')
    Xrm = window.parent.Xrm;

$ext_jmobjprefix$ServiceUtility = function () {
    var that = this;
    this.FieldType =
    {
        Lookup: "EntityReference",
        Guid: "guid",
        Int: "int",
        Decimal: "decimal",
        Bool: "boolean",
        Money: "money",
        Date: "date",
        OptionSet: "OptionSetValue",
        String: "string"
    };
    this.FilterOperator =
    {
        Equal: "Equal",
        GreaterThan: "GreaterThan",
        Null: "Null",
        NotNull: "NotNull"
    };
    
    this.RetrieveMultipleAsync = function (entityType, fields, conditions, orders, asyncCallback, onError) {
        /// <summary>Calls Retrieve Multiple Method And Passes And Passes The Results In Array Of this.EntityObject Objects To asyncCallback Method Asynchronously </summary>
        /// <summary>Note Limitied To Maximum Records Returned In One Request By CRM </summary>
        /// <param name="entityType" type="String">Logical name of type of records to retrieve. use lowercase</param>
        /// <param name="fields" type="Array of String">Array of the the logical names of fields to include in the results. use lowercase</param>
        /// <param name="conditions" type="Array of FilterCondition">Use new this.FilterCondition method to create each condition in the array</param>
        /// <param name="orders" type="Array of OrderCondition">!! Not Implemented !!</param>
        /// <param name="asyncCallback" type="Function">Method to invoke asynchronously passing in the results as array of this.EntityObject objects the first argument</param>
        var executerequestxml = "";
        executerequestxml += "<request i:type='a:RetrieveMultipleRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>Query</b:key>";
        executerequestxml += "<b:value i:type='a:QueryExpression'>";
        executerequestxml += CreateColumnSetXml(fields);

        if (conditions == null) {
            executerequestxml += "<a:Criteria />";
        } else {
            if (conditions.length > 0) {
                executerequestxml += "<a:Criteria>";
                executerequestxml += "<a:Conditions>";

                for (var i = 0; i < conditions.length; i++) {
                    var condition = conditions[i];
                    if (condition instanceof Object) {
                        executerequestxml += CreateCondition(condition.field, condition.operator, condition.value, condition.type);
                    }
                }

                executerequestxml += "</a:Conditions>";
                executerequestxml += "<a:FilterOperator>And</a:FilterOperator>";
                executerequestxml += "<a:Filters />";
                executerequestxml += "</a:Criteria>";
            } else {
                executerequestxml += "<a:Criteria />";
            }
        }

        executerequestxml += "<a:EntityName>" + entityType + "</a:EntityName>";
        executerequestxml += "<a:LinkEntities />";
        executerequestxml += RetrieveMultipleMiscParamXml(orders);
        executerequestxml += "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>RetrieveMultiple</a:RequestName>";
        executerequestxml += "</request>";

        if (asyncCallback != null) {
            function processResponseData(data) {
                var jsobjects = ResponseToJsObjects(data);
                var value = jsobjects;
                asyncCallback(value);
            }
            ExecuteRequest(executerequestxml, processResponseData, onError);

        } else {
            return ExecuteRequest(executerequestxml, null);
        }
    };
    this.Create = function (entityType, fields, processResults, onError) {
        /// <summary>Calls Create Method And Passes The New Record Id To processResults Method Asynchronously </summary>
        /// <param name="entityType" type="String">Logical name of type of record to create</param>
        /// <param name="fields" type="Array of CreateEntityLookupField or CreateEntityField">Array of the the fields to set in the new record. Use new this.CreateEntityLookupField or this.CreateEntityField to create each field in the array</param>
        /// <param name="processResults" type="Method">If success calling create then method to invoke asynchronously passing in the new records id as the first argument</param>
        /// <param name="onError" type="Method">If error calling create then method to invoke asynchronously passing in error message as the first argument</param>
        function processResponseData(data) {
            var responseItems = ResponseResultByKeys(data);
            processResults(responseItems["id"]);
        }

        var executerequestxml = "";
        executerequestxml += "<request i:type='a:CreateRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>Target</b:key>";
        executerequestxml += "<b:value i:type='a:Entity'>";
        executerequestxml += "<a:Attributes>";
        if (fields != null) {
            for (var field in fields)
                executerequestxml = executerequestxml + CreateEntityFieldNode(fields[field]);
        }
        executerequestxml += "</a:Attributes>";
        executerequestxml += "<a:EntityState i:nil='true' />";
        executerequestxml += "<a:FormattedValues />";
        executerequestxml += "<a:Id>00000000-0000-0000-0000-000000000000</a:Id>";
        executerequestxml += "<a:LogicalName>" + entityType + "</a:LogicalName>";
        executerequestxml += "<a:RelatedEntities />";
        executerequestxml += "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>Create</a:RequestName>";
        executerequestxml += "</request>";

        ExecuteRequest(executerequestxml, processResponseData, onError);
    };
    this.CreateEntityField = function (fieldName, type, value) {
        /// <summary>Object function to encapsulate a fields properties</summary>
        /// <param name="fieldName" type="String">Logical name of the field. Use lowercase</param>
        /// <param name="type" type="FieldType">Use this.FieldType.* to set the type</param>
        /// <param name="value" >Value of the field</param>
        this.fieldName = fieldName;
        this.type = type;
        this.value = value;
    };
    this.CreateEntityLookupField = function (fieldName, lookuptype, lookupid) {
        /// <summary>Object function to encapsulate a lookup fields properties</summary>
        /// <param name="fieldName" type="String">Logical name of the field. Use lowercase</param>
        /// <param name="lookuptype" type="String">Type referenced by the lookup field</param>
        /// <param name="lookupid" type="String" >Id of the object referenced by the lookup field</param>
        this.fieldName = fieldName;
        this.type = that.FieldType.Lookup;
        this.value = new Object();
        this.value.id = lookupid;
        this.value.type = lookuptype;
    };
    this.SetState = function (entityType, id, state, status) {
        /// <summary>Calls SetState Method Asynchronously </summary>
        /// <param name="entityType" type="String">Logical name of type of record to set state. Use lowercase</param>
        /// <param name="id" type="String">Id of record to set state</param>
        /// <param name="state" type="int" >Key value of state to set. Generally 0 for active, 1 for inactive</param>
        /// <param name="status" type="int" >Key value of status option to set. -1 allows system to set</param>
        var executerequestxml = "";
        executerequestxml += "<request i:type='b:SetStateRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' xmlns:b='http://schemas.microsoft.com/crm/2011/Contracts'>";
        executerequestxml += "<a:Parameters xmlns:c='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<c:key>EntityMoniker</c:key>";
        executerequestxml += "<c:value i:type='a:EntityReference'>";
        executerequestxml += "<a:Id>" + id + "</a:Id>";
        executerequestxml += "<a:LogicalName>" + entityType + "</a:LogicalName>";
        executerequestxml += "<a:Name i:nil='true' />";
        executerequestxml += "</c:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<c:key>State</c:key>";
        executerequestxml += "<c:value i:type='a:OptionSetValue'>";
        executerequestxml += "<a:Value>" + state + "</a:Value>";
        executerequestxml += "</c:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        if (status != null) {
            executerequestxml += "<a:KeyValuePairOfstringanyType>";
            executerequestxml += "<c:key>Status</c:key>";
            executerequestxml += "<c:value i:type='a:OptionSetValue'>";
            executerequestxml += " <a:Value>" + status + "</a:Value>";
            executerequestxml += "</c:value>";
            executerequestxml += "</a:KeyValuePairOfstringanyType>";
        }
        executerequestxml += " </a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>SetState</a:RequestName>";
        executerequestxml += "</request>";

        function processResponseData() {
        }

        ExecuteRequest(executerequestxml, processResponseData);
    };
    this.RetrieveAsync = function (entityType, id, fields, processResults) {
        /// <summary>Retreives One Record Of Given Type Asynchronously As EntityObject object</summary>
        /// <param name="entityType" type="String">Logical name of type of record to get. Use lowercase</param>
        /// <param name="id" type="String">Id of record to get. If null then gets first record of type</param>
        /// <param name="fields" type="Array of String" >Logical names of fields to include in the record. Use lower case</param>
        /// <param name="processResults" type="Method" >Method to invoke asynchronously passing in the record as the first argument</param>
        function processResponseData(data) {
            var jsobjects = ResponseToJsObjects(data);
            var value = jsobjects.length > 0 ? jsobjects[0] : null;
            processResults(value);
        }

        var executerequestxml = "";
        executerequestxml += "<request i:type='a:RetrieveMultipleRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>Query</b:key>";
        executerequestxml += "<b:value i:type='a:QueryExpression'>";
        executerequestxml += CreateColumnSetXml(fields);

        if (id == null) {
            executerequestxml += "<a:Criteria />";
        } else {
            executerequestxml += "<a:Criteria>";
            executerequestxml += "<a:Conditions>";
            executerequestxml += CreateIdCondition(GetPrimaryKey(entityType), id);
            executerequestxml += "</a:Conditions>";
            executerequestxml += "<a:FilterOperator>And</a:FilterOperator>";
            executerequestxml += "<a:Filters />";
            executerequestxml += "</a:Criteria>";
        }

        executerequestxml += "<a:EntityName>" + entityType + "</a:EntityName>";
        executerequestxml += "<a:LinkEntities />";
        executerequestxml += RetrieveMultipleMiscParamXml();
        executerequestxml += "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>RetrieveMultiple</a:RequestName>";
        executerequestxml += "</request>";
        ExecuteRequest(executerequestxml, processResponseData);
    };
    this.LookupFieldAsync = function (entityType, id, field, processResults) {
        /// <summary>Retreives One Specific Field From Record Asynchronously</summary>
        /// <param name="entityType" type="String">Logical name of type of record to get field from. Use lowercase</param>
        /// <param name="id" type="String">Id of record to get field from</param>
        /// <param name="field" type="String" >Logical name of field to get. Use lower case</param>
        /// <param name="processResults" type="Method" >Method to invoke asynchronously passing in the field value as the first argument</param>
        function processResponseData(data) {
            var jsobjects = ResponseToJsObjects(data);
            var value = jsobjects.length > 0 ? jsobjects[0][field] : null;
            processResults(value);
        }
        var executerequestxml = "";
        executerequestxml += "<request i:type='a:RetrieveMultipleRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>Query</b:key>";
        executerequestxml += "<b:value i:type='a:QueryExpression'>";
        executerequestxml += CreateColumnSetXml(field);
        executerequestxml += "<a:Criteria>";
        executerequestxml += "<a:Conditions>";
        executerequestxml += CreateIdCondition(GetPrimaryKey(entityType), id);
        executerequestxml += "</a:Conditions>";
        executerequestxml += "<a:FilterOperator>And</a:FilterOperator>";
        executerequestxml += "<a:Filters />";
        executerequestxml += "</a:Criteria>";
        executerequestxml += "<a:EntityName>" + entityType + "</a:EntityName>";
        executerequestxml += "<a:LinkEntities />";
        executerequestxml += RetrieveMultipleMiscParamXml();
        executerequestxml += "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>RetrieveMultiple</a:RequestName>";
        executerequestxml += "</request>";
        ExecuteRequest(executerequestxml, processResponseData);
    };
    this.LookupLinkedRecordAsync = function (entityType, entityTypeFrom, lookupName, lookupId, fields, processResults) {
        /// <summary>Retreives One Specific Field From Linked Record Asynchronously</summary>
        /// <param name="entityType" type="String">Logical name of type of record to get field from. Use lowercase</param>
        /// <param name="entityTypeFrom" type="String">Logical name of type of record to link from. Use lowercase</param>
        /// <param name="lookupName" type="String">Logical name of lookup field which links entityTypeFrom to entityType. Use lowercase</param>
        /// <param name="lookupId" type="String">Id of record to link from</param>
        /// <param name="fields" type="Array of String" >Logical names of fields to include in the record. Use lower case</param>
        /// <param name="processResults" type="Method" >Method to invoke asynchronously passing in the field value as the first argument</param>
        function processResponseData(data) {
            var jsobjects = ResponseToJsObjects(data);
            var value = jsobjects.length > 0 ? jsobjects[0] : null;
            processResults(value);
        }

        var executerequestxml = "";
        executerequestxml += "<request i:type='a:RetrieveMultipleRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";

        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>Query</b:key>";
        executerequestxml += "<b:value i:type='a:QueryExpression'>";
        executerequestxml += CreateColumnSetXml(fields);
        executerequestxml += "<a:Criteria>";
        executerequestxml += "<a:Conditions />";
        executerequestxml += "<a:FilterOperator>And</a:FilterOperator>";
        executerequestxml += "<a:Filters />";
        executerequestxml += "</a:Criteria>";
        executerequestxml += "<a:EntityName>" + entityType + "</a:EntityName>";
        executerequestxml += "<a:LinkEntities>";
        executerequestxml += "<a:LinkEntity>";
        executerequestxml += "<a:Columns>";
        executerequestxml += "<a:AllColumns>false</a:AllColumns>";
        executerequestxml += "<a:Columns xmlns:c='http://schemas.microsoft.com/2003/10/Serialization/Arrays' />";
        executerequestxml += "</a:Columns>";
        executerequestxml += "<a:EntityAlias i:nil='true' />";
        executerequestxml += "<a:JoinOperator>Inner</a:JoinOperator>";
        executerequestxml += "<a:LinkCriteria>";
        executerequestxml += "<a:Conditions>";
        executerequestxml += CreateIdCondition(GetPrimaryKey(entityTypeFrom), lookupId);
        executerequestxml += "</a:Conditions>";
        executerequestxml += "<a:FilterOperator>And</a:FilterOperator>";
        executerequestxml += "<a:Filters />";
        executerequestxml += "</a:LinkCriteria>";
        executerequestxml += "<a:LinkEntities />";
        executerequestxml += "<a:LinkFromAttributeName>" + GetPrimaryKey(entityType) + "</a:LinkFromAttributeName>";
        executerequestxml += "<a:LinkFromEntityName>" + entityType + "</a:LinkFromEntityName>";
        executerequestxml += "<a:LinkToAttributeName>" + lookupName + "</a:LinkToAttributeName>";
        executerequestxml += "<a:LinkToEntityName>" + entityTypeFrom + "</a:LinkToEntityName>";
        executerequestxml += "</a:LinkEntity>";
        executerequestxml += "</a:LinkEntities>";
        executerequestxml += RetrieveMultipleMiscParamXml();
        executerequestxml += "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>RetrieveMultiple</a:RequestName>";
        executerequestxml += "</request>";
        ExecuteRequest(executerequestxml, processResponseData);
    };
    this.LookupLinkFieldAsync = function (entityType, entityTypeFrom, lookupName, lookupId, field, processResults) {
        /// <summary>Retreives One Specific Field From Linked Record Asynchronously</summary>
        /// <param name="entityType" type="String">Logical name of type of record to get field from. Use lowercase</param>
        /// <param name="entityTypeFrom" type="String">Logical name of type of record to link from. Use lowercase</param>
        /// <param name="lookupName" type="String">Logical name of lookup field which links entityTypeFrom to entityType. Use lowercase</param>
        /// <param name="lookupId" type="String">Id of record to link from</param>
        /// <param name="field" type="String" >Logical name of field to get. Use lower case</param>
        /// <param name="processResults" type="Method" >Method to invoke asynchronously passing in the field value as the first argument</param>
        function processResponseData(data) {
            var jsobjects = ResponseToJsObjects(data);
            var value = jsobjects.length > 0 ? jsobjects[0][field] : null;
            processResults(value);
        }

        var executerequestxml = "";
        executerequestxml += "<request i:type='a:RetrieveMultipleRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";

        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>Query</b:key>";
        executerequestxml += "<b:value i:type='a:QueryExpression'>";
        executerequestxml += CreateColumnSetXml(field);
        executerequestxml += "<a:Criteria>";
        executerequestxml += "<a:Conditions />";
        executerequestxml += "<a:FilterOperator>And</a:FilterOperator>";
        executerequestxml += "<a:Filters />";
        executerequestxml += "</a:Criteria>";
        executerequestxml += "<a:EntityName>" + entityType + "</a:EntityName>";
        executerequestxml += "<a:LinkEntities>";
        executerequestxml += "<a:LinkEntity>";
        executerequestxml += "<a:Columns>";
        executerequestxml += "<a:AllColumns>false</a:AllColumns>";
        executerequestxml += "<a:Columns xmlns:c='http://schemas.microsoft.com/2003/10/Serialization/Arrays' />";
        executerequestxml += "</a:Columns>";
        executerequestxml += "<a:EntityAlias i:nil='true' />";
        executerequestxml += "<a:JoinOperator>Inner</a:JoinOperator>";
        executerequestxml += "<a:LinkCriteria>";
        executerequestxml += "<a:Conditions>";
        executerequestxml += CreateIdCondition(GetPrimaryKey(entityTypeFrom), lookupId);
        executerequestxml += "</a:Conditions>";
        executerequestxml += "<a:FilterOperator>And</a:FilterOperator>";
        executerequestxml += "<a:Filters />";
        executerequestxml += "</a:LinkCriteria>";
        executerequestxml += "<a:LinkEntities />";
        executerequestxml += "<a:LinkFromAttributeName>" + GetPrimaryKey(entityType) + "</a:LinkFromAttributeName>";
        executerequestxml += "<a:LinkFromEntityName>" + entityType + "</a:LinkFromEntityName>";
        executerequestxml += "<a:LinkToAttributeName>" + lookupName + "</a:LinkToAttributeName>";
        executerequestxml += "<a:LinkToEntityName>" + entityTypeFrom + "</a:LinkToEntityName>";
        executerequestxml += "</a:LinkEntity>";
        executerequestxml += "</a:LinkEntities>";
        executerequestxml += RetrieveMultipleMiscParamXml();
        executerequestxml += "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>RetrieveMultiple</a:RequestName>";
        executerequestxml += "</request>";
        ExecuteRequest(executerequestxml, processResponseData);
    };
    this.GetLocalTime = function (utcdatetime, timeZoneInt, processResults) {
        /// <summary>Converts A UTC Date Value To A Target Time Zone Asynchronously</summary>
        /// <param name="utcdatetime" type="String">UTC Date Time value </param>
        /// <param name="timeZoneInt" type="Int">Integer representing the timezone in CRM</param>
        /// <param name="processResults" type="Method" >Method to invoke asynchronously passing in the converted Date value as the first argument</param>
        function processResponseData(data) {
            var responseItems = ResponseResultByKeys(data);
            processResults(responseItems["LocalTime"]);
        }

        var executerequestxml = "";
        executerequestxml += "<request i:type='c:LocalTimeFromUtcTimeRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' xmlns:c='http://schemas.microsoft.com/crm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";

        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>TimeZoneCode</b:key>";
        executerequestxml += "<b:value i:type='d:int' xmlns:d='http://www.w3.org/2001/XMLSchema'>" + ToJsonString(timeZoneInt) + "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>UtcTime</b:key>";
        executerequestxml += "<b:value i:type='d:dateTime' xmlns:d='http://www.w3.org/2001/XMLSchema'>" + ToJsonString(utcdatetime) + "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>LocalTimeFromUtcTime</a:RequestName>";
        executerequestxml += "</request>";
        // Capture the result
        ExecuteRequest(executerequestxml, processResponseData);
    };
    this.AssignRecordAsync = function (targetId, targetEntity, assigneeId, assigneeEntity, onSuccess) {
        /// <summary>Calls The Assign Method Asynchronously</summary>
        /// <param name="targetId" type="String">Logical name of type of record to assign. Use lowercase</param>
        /// <param name="targetEntity" type="String">Id of record to assign</param>
        /// <param name="assigneeId" type="String">Id of user or team to assign to</param>
        /// <param name="assigneeEntity" type="String">Type (team or systemuser) to assign to</param>
        /// <param name="processResults" type="Method" >If success calling Assign method to invoke asynchronously (no arguments)</param>
        var executerequestxml = "";
        executerequestxml += "<request i:type='b:AssignRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' xmlns:b='http://schemas.microsoft.com/crm/2011/Contracts'>";
        executerequestxml += "<a:Parameters xmlns:c='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<c:key>Target</c:key>";
        executerequestxml += "<c:value i:type='a:EntityReference'>";
        executerequestxml += "<a:Id>" + targetId + "</a:Id>";
        executerequestxml += "<a:LogicalName>" + targetEntity + "</a:LogicalName>";
        executerequestxml += "<a:Name i:nil='true' />";
        executerequestxml += "</c:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<c:key>Assignee</c:key>";
        executerequestxml += "<c:value i:type='a:EntityReference'>";
        executerequestxml += "<a:Id>" + assigneeId + "</a:Id>";
        executerequestxml += "<a:LogicalName>" + assigneeEntity + "</a:LogicalName>";
        executerequestxml += "<a:Name i:nil='true' />";
        executerequestxml += "</c:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>Assign</a:RequestName>";
        executerequestxml += "</request>";

        ExecuteRequest(executerequestxml, onSuccess);
    };
    this.ExecuteAction = function (actionName, actionArguments, processResults) {
        /// <summary>Invokes A CRM Process Of Type Action Asynchronously</summary>
        /// <param name="actionName" type="String">Logical name of action to invoke (e.g. new_actionname)</param>
        /// <param name="actionArguments" type="Array Of CreateEntityLookupField or CreateEntityField">Input arguments for the action. Use new this.CreateEntityLookupField or this.CreateEntityField to create each field in the array</param>
        /// <param name="processResults" type="Method">Id of user or team to assign to</param>
        function processResponseData(data) {
            var responseItems = ResponseResultByKeys(data);
            processResults(responseItems);
        }
        var executerequestxml = "";
        executerequestxml += "<request xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:c='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        var targetAdded = false;
        for (var i in actionArguments) {
            executerequestxml += CreateEntityFieldNode(actionArguments[i], "c");
            if (actionArguments[i].fieldName == "Target")
                targetAdded = true;
        }
        if (!targetAdded) {
            executerequestxml += "<a:KeyValuePairOfstringanyType>";
            executerequestxml += "<c:key>Target</c:key>";
            executerequestxml += "<c:value i:nil='true' />";
            executerequestxml += "</a:KeyValuePairOfstringanyType>";
        }
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>" + actionName + "</a:RequestName>";
        executerequestxml += "</request>";

        ExecuteRequest(executerequestxml, processResponseData);
    };
    function ResponseResultByKeys(response) {
        /// <summary>Converts an xml response containing a Results Node containing KeyValuePairOfstringanyType into a javascript object</summary>
        /// <param name="response" type="String">xml containing the response</param>
        /// <returns type="Object">Object containing a property for each key/value in the response</returns>
        CheckError(response);
        var results = new Object();
        var resultNodes = XrmFind(response, 'Results');
        if (resultNodes.length > 0) {
            var pairNodes = XrmFind(resultNodes[0], "KeyValuePairOfstringanyType");
            for (var i = 0; i < pairNodes.length; i++) {
                var fieldName = XrmInnerText(XrmElementsByTagName(pairNodes[i], 'key')[0]);
                var fieldValue = ParseJSField(XrmElementsByTagName(pairNodes[i], 'value')[0]);
                results[fieldName] = fieldValue;
            }
        }
        return results;
    }

    function CheckError(response) {
        /// <summary>Checks xml response has thrown an error (Fault)</summary>
        /// <param name="response" type="String">xml containing the response</param>
        var errorCount = XrmFind(response, 'Fault').length;
        if (errorCount != 0) {
            var msg = XrmInnerText(XrmFind(response, 'Text'));
            throw "" + msg;
        }
    }
    function ResponseToJsObjects(response) {
        /// <summary>Converts an xml response containing a list of Entities into javascript objects</summary>
        /// <param name="response" type="String">xml containing the response</param>
        /// <returns type="Array of EntityObject">Object containing a property for each entity in the response</returns>
        CheckError(response);

        var results = new Array();
        var entityNodes = XrmFind(response, 'Entity');
        for (var i = 0; i < entityNodes.length; i++) {
            var entity = new EntityObject();
            var attributeNodes = XrmFind(XrmFind(entityNodes[i], 'Attributes'), 'KeyValuePairOfstringanyType');
            for (var j = 0; j < attributeNodes.length; j++) {
                var fieldName = XrmInnerText(XrmElementsByTagName(attributeNodes[j], 'key')[0]);
                var fieldValue = ParseJSField(XrmElementsByTagName(attributeNodes[j], 'value')[0]);
                entity[fieldName] = fieldValue;
            }
            var formattedFields = new Object();
            var formattedNodes = XrmFind(XrmFind(entityNodes[i], 'FormattedValues'), 'KeyValuePairOfstringstring');
            for (var k = 0; k < formattedNodes.length; k++) {
                var formattedFieldName = XrmInnerText(XrmElementsByTagName(formattedNodes[k], 'key')[0]);
                var formattedFieldValue = XrmInnerText(XrmElementsByTagName(formattedNodes[k], 'value')[0]);
                formattedFields[formattedFieldName] = formattedFieldValue;
            }
            entity.formattedfieldsobject = formattedFields;
            results.push(entity);
        }
        //add a formattedvalues field

        return results;
    }
    function EntityObject() {
        /// <summary>Object to represent an entity instance</summary>
        /// <param name="response" type="String">xml containing the response</param>
        this.formattedfieldsobject = null;
        var that = this;
        this.setFormattedFields = function (formattedfields) {
            that.formattedfieldsobject = formattedfields;
        }
        this.getFormattedField = function (fieldName) {
            if (that.formattedfieldsobject[fieldName] != null)
                return that.formattedfieldsobject[fieldName];
            else
                return that[fieldName];
        }
    }
    //THIS METHOD CREATED BECAUSE IE AND OTHER BROWSERS DISPLAYED DIFFERENT BEHAVIOUR WHEN SELECTING SCHEMA PREFIXED ELEMENTS
    function XrmInnerText(node) {
        var text = node.innerText;
        if (text == null)
            text = node.textContent;
        if (node.innerHTML != null)
            text = node.innerHTML;
        return text;
    }
    //THIS METHOD CREATED BECAUSE IE AND OTHER BROWSERS DISPLAYED DIFFERENT BEHAVIOUR WHEN SELECTING SCHEMA PREFIXED ELEMENTS
    function XrmElementsByTagName(node, tag) {
        var elements = node.getElementsByTagName(tag);
        if (elements.length == 0)
            elements = node.getElementsByTagName('a:' + tag);
        if (elements.length == 0)
            elements = node.getElementsByTagName('b:' + tag);
        if (elements.length == 0)
            elements = node.getElementsByTagName('c:' + tag);
        return elements;
    };
    //THIS METHOD CREATED BECAUSE IE AND OTHER BROWSERS DISPLAYED DIFFERENT BEHAVIOUR WHEN SELECTING SCHEMA PREFIXED ELEMENTS
    function XrmFind(node, nodeName) {
        var findSelector = $(node).find(nodeName);
        if (findSelector.length == 0)
            findSelector = $(node).find('a\\:' + nodeName);
        if (findSelector.length == 0)
            findSelector = $(node).find('b\\:' + nodeName);
        return findSelector;
    }
    function ParseJSField(attributeNode) {
        /// <summary>Convert the xml for a field value into an appropriate javascript field value</summary>
        /// <param name="response" type="String">xml containing the attrobute value</param>
        var ctype = attributeNode.getAttribute('i:type');
        var fieldText = XrmInnerText(attributeNode);
        var fieldValue = XrmInnerText(attributeNode);
        if (attributeNode.parentNode != null
            && attributeNode.parentNode.attributes
            && attributeNode.parentNode.attributes.length > 0
            && attributeNode.parentNode.attributes[0].value == "a:OptionSetValue")
            ctype = "OptionSetValue";
        
        if (ctype != null) {
            if (ctype.indexOf("int") != -1)
                fieldValue = parseInt(fieldText);
            else if (ctype.indexOf("boolean") != -1)
                fieldValue = fieldValue == "true" ? true : false;
            else if (ctype.indexOf("Money") != -1) {
                fieldValue = parseFloat(fieldValue);
                if (fieldValue == NaN)
                    field = null;
            } else if (ctype.indexOf("dateTime") != -1)
                fieldValue = ParseDate(fieldText);
            else if (ctype.indexOf("EntityReference") != -1) {
                var lookup = new Object();
                var idNodeA = XrmElementsByTagName(attributeNode, 'Id');
                lookup.id = XrmInnerText(idNodeA[0]);
                var typeNodeA = XrmElementsByTagName(attributeNode, 'LogicalName');
                lookup.type = XrmInnerText(typeNodeA[0]);
                var nameNodeA = XrmElementsByTagName(attributeNode, 'Name');
                lookup.name = XrmInnerText(nameNodeA[0]);
                fieldValue = lookup;
            } else if (ctype.indexOf("OptionSetValue") != -1)
                fieldValue = parseInt(fieldText);
        }
        if (fieldValue === '')
            fieldValue = null;
        return fieldValue;
    }
    function HttpContainer(httpReq, onSuccess, onError) {
        this.OnSuccess = onSuccess;
        this.OnError = onError;
        this.HttpReq = httpReq;
        var that = this;

        this.OnStateChange = function () {
            if (that.HttpReq.readyState != 4) {
                return null;
            }

            if (that.HttpReq.status != 200) {
                var xhr = that.HttpReq;
                var errorMessage = "";
                if (xhr.responseText != null) {
                    var messageNodes = XrmFind(xhr.responseText, 'Message');
                    if (messageNodes.length > 0)
                        errorMessage = XrmInnerText(messageNodes[0]);
                    else
                        errorMessage = messageNodes;
                } else if (xhr.responseXML != null) {
                    var messageNodes = XrmFind(xhr.responseXML.xml, 'Message');
                    if (messageNodes.length > 0)
                        errorMessage = XrmInnerText(messageNodes[0]);
                    else
                        errorMessage = messageNodes;
                } else
                    errorMessage = "Unexpected error processing ajax request: " + error;
                if (that.OnError == null)
                    alert("Error: " + errorMessage);
                else
                    that.OnError(errorMessage);
            } else {
                that.OnSuccess(that.HttpReq.response);
            }
        }
    }
    function ExecuteRequest(requestXml, onSuccess, onError) {
        /// <summary>Asynchronously executes an OrganisationRequest through the CRM SOAP Web Service </summary>
        /// <param name="requestXml" type="String">xml representing the OrganisationRequest</param>
        /// <param name="onSuccess" type="Method">Function to execute if the request succeeds</param>
        /// <param name="onError" type="Method">Function to execute if an error is thrown. Passes in the error message string as the first argument</param>
        var urlPrefix = "";
        var currentUrl = window.location.href;
        if (currentUrl.indexOf("/" + Xrm.Page.context.getOrgUniqueName() + "/") != -1)
            urlPrefix = "/" + Xrm.Page.context.getOrgUniqueName();
        else
            urlPrefix = "";

        var request = "<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>";
        request += "<s:Body>";
        request += "<Execute xmlns='http://schemas.microsoft.com/xrm/2011/Contracts/Services' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>";
        request += requestXml;
        request += "</Execute>";
        request += "</s:Body>";
        request += "</s:Envelope>";

        var xmlhttp = new XMLHttpRequest();
        xmlhttp.open("POST", urlPrefix + "/XRMServices/2011/Organization.svc/web", true);
        xmlhttp.setRequestHeader("Accept", "application/xml, text/xml, */*");
        xmlhttp.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
        xmlhttp.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
        xmlhttp.onreadystatechange = new HttpContainer(xmlhttp, onSuccess, onError).OnStateChange;
        xmlhttp.send(request);
    }
    function ParseDate(xmlText) {
        var dateParts = xmlText.split("T")[0].split("-");
        var timeParts = xmlText.split("T")[1].split("+")[0].split(":");
        return new Date(dateParts[0], dateParts[1] - 1, dateParts[2], timeParts[0], timeParts[1], timeParts[2].substring(0, timeParts[2].length - 1));
    }
    function GetPrimaryKey(entity) {
        return entity + "id";
    }
    function ToJsonString(value) {

        function pad(n) {
            // Format integers to have at least two digits.
            return n < 10 ? '0' + n : n;
        }

        if (value instanceof Date)
            return value.getUTCFullYear() + '-' +
                pad(value.getUTCMonth() + 1) + '-' +
                pad(value.getUTCDate()) + 'T' +
                pad(value.getUTCHours()) + ':' +
                pad(value.getUTCMinutes()) + ':' +
                pad(value.getUTCSeconds()) + 'Z';
        else
            return "" + value;
    }
    function RetrieveMultipleMiscParamXml() {
        var xml = "";
        xml += "<a:Distinct>false</a:Distinct>";
        xml += "<a:Orders />";
        xml += "<a:PageInfo>";
        xml += "<a:Count>0</a:Count>";
        xml += "<a:PageNumber>0</a:PageNumber>";
        xml += "<a:PagingCookie i:nil='true' />";
        xml += "<a:ReturnTotalRecordCount>false</a:ReturnTotalRecordCount>";
        xml += "</a:PageInfo>";
        xml += "<a:NoLock>false</a:NoLock>";
        return xml;
    }
    function CreateColumnSetXml(fields) {
        var xml = "";
        xml += "<a:ColumnSet>";
        if (fields == null) {
            xml += "<a:AllColumns>true</a:AllColumns>";
            xml += "<a:Columns xmlns:c='http://schemas.microsoft.com/2003/10/Serialization/Arrays'>";
            xml += "</a:Columns>";
        } else {
            xml += "<a:AllColumns>false</a:AllColumns>";
            xml += "<a:Columns xmlns:c='http://schemas.microsoft.com/2003/10/Serialization/Arrays'>";
            if (fields instanceof Object) {
                for (var i = 0; i < fields.length; i++)
                    xml += "<c:string>" + fields[i] + "</c:string>";
            } else {
                var fieldsString = fields + "";
                var split = fieldsString.split(",");
                for (var j =0; j < split.length; j++)
                    xml += "<c:string>" + split[j] + "</c:string>";
            }
            xml += "</a:Columns>";
        }
        xml += "</a:ColumnSet>";
        return xml;
    }
    function CreateIdCondition(field, value) {
        var xml = "";
        xml += "<a:ConditionExpression>";
        xml += "<a:AttributeName>" + field + "</a:AttributeName>";
        xml += "<a:Operator>Equal</a:Operator>";
        xml += "<a:Values xmlns:c='http://schemas.microsoft.com/2003/10/Serialization/Arrays'>";
        xml += "<c:anyType i:type='d:guid' xmlns:d='http://schemas.microsoft.com/2003/10/Serialization/'>" + value + "</c:anyType>";
        xml += "</a:Values>";
        xml += "</a:ConditionExpression>";
        return xml;
    }
    function CreateCondition(field, operator, value, type) {
        if (type == that.FieldType.OptionSet)
            type = that.FieldType.Int;
        if (type == that.FieldType.Lookup)
            type = that.FieldType.Guid;
        var namespace;
        if (type == "guid")
            namespace = "http://schemas.microsoft.com/2003/10/Serialization/";
        else
            namespace = "http://www.w3.org/2001/XMLSchema";

        var xml = "";
        xml += "<a:ConditionExpression>";
        xml += "<a:AttributeName>" + field + "</a:AttributeName>";
        xml += "<a:Operator>" + operator + "</a:Operator>";
        xml += "<a:Values xmlns:c='http://schemas.microsoft.com/2003/10/Serialization/Arrays'>";
        if (value != null)
            xml += "<c:anyType i:type='d:" + type + "' xmlns:d='" + namespace + "'>" + value + "</c:anyType>";
        xml += "</a:Values>";
        xml += "</a:ConditionExpression>";
        return xml;
    }
    function CreateEntityFieldNode(field, keyValueKey) {
        if (keyValueKey == null)
            keyValueKey = "b";
        var xml = "";
        xml += "<a:KeyValuePairOfstringanyType>";
        xml += "<" + keyValueKey + ":key>" + field.fieldName + "</" + keyValueKey + ":key>";

        if (field.type == that.FieldType.Lookup) {
            xml += "<" + keyValueKey + ":value i:type='a:" + field.type + "'>";
            xml += "<a:Id>" + field.value.id + "</a:Id>";
            xml += "<a:LogicalName>" + field.value.type + "</a:LogicalName>";
            xml += "<a:Name i:nil='true' />";
            xml += "</" + keyValueKey + ":value>";
        } else if (field.type === that.FieldType.OptionSet) {
            xml += "<" + keyValueKey + ":value i:type='a:" + field.type + "'>";
            xml += "<a:Value>" + field.value + "</a:Value>";
            xml += "</" + keyValueKey + ":value>";
        } else {
            xml += "<" + keyValueKey + ":value i:type='f:" + field.type + "' xmlns:f='http://www.w3.org/2001/XMLSchema'>" + field.value + "</" + keyValueKey + ":value>";
        }

        xml += "</a:KeyValuePairOfstringanyType>";
        return xml;
    }
    function CreateOrder(field, descending) {
        var xml = "";
        xml += "<a:OrderExpression>";
        xml += "<a:AttributeName>" + field + "</a:AttributeName>";
        xml += "<a:OrderType>" + descending + "</a:OrderType>";
        xml += "</a:OrderExpression>";
        return xml;
    }
    this.FilterCondition = function (field, operator, value, type) {
        /// <summary>Creates a filter condition object for use in the RetrieveMultipleAsync method</summary>
        /// <param name="field" type="String">Name of the field for the condition. Use lowercase </param>
        /// <param name="operator" type="String">Operator for the condition. Use the FilterOperator constants</param>
        /// <param name="value" type="String">Value for the condition</param>
        /// <param name="type" type="String">The type of field (use the this.FieldType constants). Required for the service to form the correct xml for the clr object type</param>
        this.field = field;
        this.operator = operator;
        this.value = value;
        this.type = type;
    };
    this.OrderCondition = function (field, descending) {
        this.field = field;
        this.descending = descending;
    };
    this.Fetch = function (fetch, asyncCallback) {
        /// <summary>Calls Retreive Multiple For A Fetch Query</summary>
        /// <summary>Note Limitied To Maximum Records Returned In One Request By CRM </summary>
        /// <param name="fetch" type="String">The FetchXml query</param>
        /// <param name="asyncCallback" type="Method">Method to invoke asynchronously passing in the results as array of this.EntityObject objects the first argument</param>
        var executerequestxml = "";
        executerequestxml += "<request i:type='a:RetrieveMultipleRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>Query</b:key>";
        executerequestxml += "<b:value  i:type='a:FetchExpression'>";
        executerequestxml += "<a:Query>";
        executerequestxml += that.EncodeHTML(fetch);
        executerequestxml += "</a:Query>";
        executerequestxml += "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>RetrieveMultiple</a:RequestName>";
        executerequestxml += "</request>";

        function processResponseData(data) {
            var jsobjects = ResponseToJsObjects(data);
            asyncCallback(jsobjects);
        }

        ExecuteRequest(executerequestxml, processResponseData);
    };
    this.EncodeHTML = function (string) {
        if (string == null)
            return null;
        else
            return string.replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&apos;');
    };
    this.Update = function (entityType, id, fields, onSuccess) {
        /// <summary>Calls Update Method Asynchronously </summary>
        /// <param name="entityType" type="String">Logical name of type of record to update</param>
        /// <param name="id" type="String">Id of record to update</param>
        /// <param name="fields" type="Array of CreateEntityLookupField or CreateEntityField">Array of the the fields to set in the record. Use new this.CreateEntityLookupField or this.CreateEntityField to create each field in the array</param>
        /// <param name="onSuccess" type="Method">If success calling update then method to invoke asynchronously </param>
        var executerequestxml = "";
        executerequestxml += "<request i:type='a:UpdateRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' >";
        executerequestxml += "<a:Parameters xmlns:b='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>";
        executerequestxml += "<a:KeyValuePairOfstringanyType>";
        executerequestxml += "<b:key>Target</b:key>";
        executerequestxml += "<b:value i:type='a:Entity'>";
        executerequestxml += "<a:Attributes>";
        if (fields != null) {
            for (var field in fields)
                executerequestxml = executerequestxml + CreateEntityFieldNode(fields[field]);
        }
        executerequestxml += "</a:Attributes>";
        executerequestxml += "<a:EntityState i:nil='true' />";
        executerequestxml += "<a:FormattedValues />";
        executerequestxml += "<a:Id>" + id + "</a:Id>";
        executerequestxml += "<a:LogicalName>" + entityType + "</a:LogicalName>";
        executerequestxml += "<a:RelatedEntities />";
        executerequestxml += "</b:value>";
        executerequestxml += "</a:KeyValuePairOfstringanyType>";
        executerequestxml += "</a:Parameters>";
        executerequestxml += "<a:RequestId i:nil='true' />";
        executerequestxml += "<a:RequestName>Update</a:RequestName>";
        executerequestxml += "</request>";

        ExecuteRequest(executerequestxml, onSuccess);
    };
    this.GetFormRecordId = function () {
        return Xrm.Page.data.entity.getId();
    };
}

$ext_jminstprefix$ServiceUtility = new $ext_jmobjprefix$ServiceUtility();