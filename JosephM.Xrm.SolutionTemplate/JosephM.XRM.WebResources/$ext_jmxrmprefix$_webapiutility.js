$ext_jmobjprefix$WebApiUtility = function () {

    this.ExecuteUnboundAction = function (actionName, arguments, successCallback, errorCallback) {
        executePost(actionName, JSON.stringify(arguments), successCallback, errorCallback);
    };

    this.ExecuteBoundAction = function (actionName, targetTypePlural, targetId, arguments, successCallback, errorCallback) {
        targetId = targetId.replace('{', '').replace('}', '');
        executePost(targetTypePlural + "(" + targetId + ")/Microsoft.Dynamics.CRM." + actionName, arguments == null ? null : JSON.stringify(arguments), function (result) { successCallback(result); }, errorCallback);
    };

    function executePost(queryString, data, successCallback, errorCallback) {
        var req = new XMLHttpRequest();

        req.open("POST", encodeURI(getWebAPIPath() + queryString), true);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Prefer", "return=representation");
        req.onreadystatechange = function () {
            if (this.readyState == 4 /* complete */) {
                req.onreadystatechange = null;
                if (this.status == 200 || this.status == 201) {
                    if (successCallback) {
                        successCallback(JSON.parse(this.response));
                    }
                }
                else if (this.status == 204) {
                    if (successCallback) {
                        successCallback();
                    }
                }
                else {
                    console.log("Error Calling Web API");
                    console.log(this.response);
                    if (errorCallback != null) {
                        errorCallback(this.response);
                    }
                }
            }
        };
        req.send(data);
    }

    function getWebAPIPath() {
        return getClientUrl() + "/api/data/v9.2/";
    }

    function getClientUrl() {
        return Xrm.Utility.getGlobalContext().getClientUrl();
    }
};