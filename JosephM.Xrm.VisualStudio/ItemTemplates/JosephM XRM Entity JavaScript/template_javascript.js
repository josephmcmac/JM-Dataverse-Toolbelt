$safeitemname$ = new Object();

$safeitemname$.RunOnLoad = function () {
    $jminstprefix$PageUtility.CommonForm($safeitemname$.RunOnChange, $safeitemname$.RunOnSave);
}

$safeitemname$.RunOnChange = function (fieldName) {
    switch (fieldName) {
        case "name":
            break;
    }
}

$safeitemname$.RunOnSave = function () {
}