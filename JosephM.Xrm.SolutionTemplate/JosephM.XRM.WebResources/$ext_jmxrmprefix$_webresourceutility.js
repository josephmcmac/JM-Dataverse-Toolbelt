//Note object instantiated after function definition

if (typeof Xrm === 'undefined')
    Xrm = window.parent.Xrm;

$ext_jmobjprefix$WebResourceUtility = function () {
    var that = this;
    function getQueryArgValue(key, queryString) {
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

    this.getDataArgValue = function(key) {
        var queryString = window.location.search;
        var dataString = getQueryArgValue('data', queryString);
        if (dataString != null)
            return getQueryArgValue(key, decodeURIComponent(dataString));
        return null;
    }

    this.getId = function (){
        var idString = getQueryArgValue("id", window.location.search);
        if (idString != null) {
            idString = idString.replace('%7b', '').replace('%7d', '').replace('%7B', '').replace('%7D', '');
        }
        return idString;
    }

    this.getType = function () {
        return getQueryArgValue("typename", window.location.search);
    }

    this.getParameterByName = function(name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    };

    function getEntity() {
        if (window.parent != null
            && window.parent.Xrm != null
            && window.parent.Xrm.Page != null
            && window.parent.Xrm.Page.data != null
            && window.parent.Xrm.Page.data.entity != null)
            return window.parent.Xrm.Page.data.entity;
        if (Xrm != null
            && Xrm.Page != null
            && Xrm.Page.data != null
            && Xrm.Page.data.entity != null)
            return Xrm.Page.data.entity;
        return null;
    }

    this.getParentFormId = function () {
        var entity = getEntity();
        if (entity != null) {
            var id = entity.getId();
            if (id != null)
                id = id.replace('{', '').replace('}', '');
            return id;
        }
        return null;
    }

    this.getParentFormEntityType = function () {
        var entity = getEntity();
        if (entity != null && entity.getEntityName != null) {
            return entity.getEntityName();
        }
        return null;
    }

    function getControl(controlName) {
        if (window.parent != null
            && window.parent.Xrm != null
            && window.parent.Xrm.Page != null
            && window.parent.Xrm.Page.getControl != null
            && window.parent.Xrm.Page.getControl(controlName) != null)
            return window.parent.Xrm.Page.getControl(controlName);
        if (Xrm != null
            && Xrm.Page != null
            && Xrm.Page.getControl != null
            && Xrm.Page.getControl(controlName) != null)
            return Xrm.Page.getControl(controlName);
        return null;
    }

    this.refreshGridOnParentForm = function (gridName) {
        var control = getControl(gridName);
        if (control != null)
            control.refresh();
    }

    function getAttribute(fieldName) {
        if (window.parent != null
            && window.parent.Xrm != null
            && window.parent.Xrm.Page != null
            && window.parent.Xrm.Page.getAttribute != null
            && window.parent.Xrm.Page.getAttribute(fieldName) != null)
            return window.parent.Xrm.Page.getAttribute(fieldName);
        if (Xrm != null
            && Xrm.Page != null
            && Xrm.Page.getAttribute != null
            && Xrm.Page.getAttribute(fieldName) != null)
            return Xrm.Page.getAttribute(fieldName);
        return null;
    }

    this.GetFieldValue = function (fieldName) {
        var attribute = getAttribute(fieldName);
        if (attribute != null)
            return attribute.getValue();
        return null;
    };

    this.ParentFormPopupMessage = function (message, onCloseCallback) {
        if (onCloseCallback == null)
            onCloseCallback = function () { };
        if (window.parent != null
            && window.parent.Xrm != null
            && window.parent.Xrm.Utility != null
            && window.parent.Xrm.Utility.alertDialog != null)
            window.parent.Xrm.Utility.alertDialog(message, onCloseCallback);
        else if (Xrm != null
            && Xrm.Utility != null
            && Xrm.Utility.alertDialog != null)
            Xrm.Utility.alertDialog(message, onCloseCallback);
        else {
            alert(message);
            onCloseCallback();
        }
    };
}

$ext_jminstprefix$WebResourceUtility = new $ext_jmobjprefix$WebResourceUtility();