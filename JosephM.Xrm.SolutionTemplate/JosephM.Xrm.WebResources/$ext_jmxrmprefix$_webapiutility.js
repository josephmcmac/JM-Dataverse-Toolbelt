
//Note object instantiated after function definition

if (typeof Xrm === 'undefined')
    Xrm = window.parent.Xrm;

$ext_jmobjprefix$WebApiUtility = function () {
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
        Equal: "eq",
    };

    this.FilterCondition = function (field, operator, value, type) {
        this.field = field;
        this.operator = operator;
        this.value = value;
        this.type = type;
    };

    this.OrderCondition = function (field, isDescending) {
        this.field = field;
        this.isDescending = isDescending == true;
    };

    function executeGet(queryString, successCallback, errorCallback)
    {
        var req = new XMLHttpRequest();

        req.open("Get", encodeURI(getWebAPIPath() + queryString), true);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.onreadystatechange = function () {
            if (this.readyState == 4 /* complete */) {
                req.onreadystatechange = null;
                if (this.status == 200 || this.status == 204) {
                    if (successCallback)
                        successCallback(JSON.parse(this.response));
                }
                else {
                    if (errorCallback)
                        errorCallback(errorHandler(this.response));
                }
            }
        };
        req.send();
    }

    function executePost(queryString, data, successCallback, errorCallback) {
        var req = new XMLHttpRequest();

        req.open("POST", encodeURI(getWebAPIPath() + queryString), true);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.onreadystatechange = function () {
            if (this.readyState == 4 /* complete */) {
                req.onreadystatechange = null;
                if (this.status == 200) {
                    if (successCallback)
                        successCallback(JSON.parse(this.response));
                }
                else {
                    if (errorCallback)
                        errorCallback(errorHandler(this.response));
                }
            }
        };
        req.send(data);
    }

    function createQueryString(fields, conditions, orders) {
        var params = new Array();
        if (conditions != null) {
            var conditionStrings = new Array();
            for (var i in conditions) {
                var thisOne = conditions[i];
                var valueString = "" + thisOne.value;
                if (thisOne.type == that.FieldType.Guid)
                    valueString = valueString.replace('{', '').replace('}', '');

                var thisString = thisOne.field + " " + thisOne.operator + " " + valueString;
                conditionStrings.push(thisString);
            }
            if (conditionStrings.length > 0) {
                params.push("$filter=" + conditionStrings.join(" and "));
            }
        }
        if (fields != null) {
            if (typeof fields === "string") {
                params.push("$select=" + fields);
            }
            else {
                var fieldStrings = new Array();
                for (var i in fields) {
                    var thisOne = fields[i];
                    fieldStrings.push(thisOne);
                }
                if (fieldStrings.length > 0) {
                    params.push("$select=" + fieldStrings.join(","));
                }
            }
        }
        if (orders != null) {
            if (typeof orders === "string") {
                params.push("$orderby=" + orders + " asc");
            }
            else {
                var orderStrings = new Array();
                for (var i in orders) {
                    var thisOne = orders[i].field + (orders[i].isDescending ? " desc" : " asc");
                    orderStrings.push(thisOne);
                }
                if (orderStrings.length > 0) {
                    params.push("$orderby=" + orderStrings.join(","));
                }
            }
        }
        var appendString = "";
        if (params.length > 0) {
            appendString = "?" + params.pop();
            while (params.length > 0)
                appendString = appendString + "&" + params.pop();
        }
        return appendString;
    }

    this.executeAction = function (actionName, arguments, successCallback, errorCallback) {
        executePost(actionName, JSON.stringify(arguments), function (result) { successCallback(result); }, errorCallback);
    };

    this.get = function (entitySetName, id, fields, successCallback, errorCallback) {
        id = id.replace('{', '').replace('}', '');
        executeGet(entitySetName + "(" + id + ")" + createQueryString(fields), successCallback, errorCallback);
    };

    this.getMultiple = function (entitySetName, fields, conditions, orders, successCallback, errorCallback) {
        executeGet(entitySetName + createQueryString(fields, conditions, orders), function (result) { successCallback(result.value); }, errorCallback);
    };

    //Internal supporting functions
    function getClientUrl() {
        //Get the organization URL
        if (typeof GetGlobalContext == "function" &&
            typeof GetGlobalContext().getClientUrl == "function") {
            return GetGlobalContext().getClientUrl();
        }
        else {
            //If GetGlobalContext is not defined check for Xrm.Page.context;
            if (typeof Xrm != "undefined" &&
                typeof Xrm.Page != "undefined" &&
                typeof Xrm.Page.context != "undefined" &&
                typeof Xrm.Page.context.getClientUrl == "function") {
                try {
                    return Xrm.Page.context.getClientUrl();
                } catch (e) {
                    throw new Error("Xrm.Page.context.getClientUrl is not available.");
                }
            }
            else { throw new Error("Context is not available."); }
        }
    }
    function getWebAPIPath() {
        return getClientUrl() + "/api/data/v8.1/";
    }

    // This function is called when an error callback parses the JSON response
    // It is a public function because the error callback occurs within the onreadystatechange 
    // event handler and an internal function would not be in scope.
    function errorHandler (resp) {
        try {
            return JSON.parse(resp).error;
        } catch (e) {
            return new Error("Unexpected Error")
        }
    }
}

$ext_jminstprefix$WebApiUtility = new $ext_jmobjprefix$WebApiUtility();