$.MMS.CoreValues = function (mod) {
    mod = mod.toLowerCase();

    if (mod == "create" || mod == "edit" || mod == "delete")
        checkValidationSummaryErrors();

    if (mod == "create" || mod == "edit")
        $("#Nombre").focus();
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm textarea").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
    }
};