$.MMS.TipoDevoluciones = function (mod) {

    mod = mod.toLowerCase();

    if (mod == "create" || mod == "edit" || mod == "delete")
        checkValidationSummaryErrors();


    if (mod == "create" || mod == "edit") {

    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
    }
}