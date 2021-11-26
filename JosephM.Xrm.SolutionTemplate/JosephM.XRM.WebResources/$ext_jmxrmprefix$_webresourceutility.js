CCCWebResourceUtility = function () {

    var that = this;
    function getQueryParameterValue(key, queryString) {
        if (queryString == null)
            return null;
        if (queryString == "")
            return null;
        if (queryString.indexOf("?") == 0)
            queryString = queryString.substr(1);
        var splitQuery = queryString.split("&");
        if (splitQuery.length > 0) {
            for (var i in splitQuery) {
                var splitarg = splitQuery[i].split("=");
                if (splitarg[0] == key && splitarg.length > 0) {
                    return splitarg[1];
                }
            }
        }
        return null;
    }

    this.GetDataParameterValue = function (key) {
        var queryString = window.location.search;
        var dataString = getQueryParameterValue('data', queryString);
        if (dataString != null)
            return getQueryParameterValue(key, decodeURIComponent(dataString));
        return null;
    };

    this.GetType = function () {
        return getQueryParameterValue("typename", window.location.search);
    };

    this.GetId = function () {
        return getQueryParameterValue("id", window.location.search);
    };
};