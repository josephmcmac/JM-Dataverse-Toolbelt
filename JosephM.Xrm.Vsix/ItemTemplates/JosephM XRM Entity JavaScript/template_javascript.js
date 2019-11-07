$safeitemname$ = new function()
{
    var that = this;

    this.RunOnLoad = function(context) {
        that.context = context;
        that.$jminstprefix$PageUtility = new $jmobjprefix$PageUtility(context.getFormContext());
        that.$jminstprefix$PageUtility.CommonForm(that.RunOnChange, that.RunOnSave);
    };

    this.RunOnChange = function(fieldName) {
        switch (fieldName)
        {
            case "fieldname":
                break;
        }
    };

    this.RunOnSave = function() {
    };
}
();