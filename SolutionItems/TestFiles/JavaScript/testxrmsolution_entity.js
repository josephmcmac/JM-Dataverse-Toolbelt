testxrmsolution_entity = new Object();

testxrmsolution_entity.RunOnLoad = function () {
    testXrmSolutionPageUtility.CommonForm(testxrmsolution_entity.RunOnChange, testxrmsolution_entity.RunOnSave);
}

testxrmsolution_entity.RunOnChange = function (fieldName) {
    switch (fieldName) {
        case "name":
            break;
    }
}

testxrmsolution_entity.RunOnSave = function () {
}