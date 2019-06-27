$.MMS.CausaPQRS = function (mod) {



    mod = mod.toLowerCase();

    $("#TipoPQRS").select2({
        placeholder: "Search PQRS",
        theme: "bootstrap"
    });

    if (mod == "create" || mod == "edit" || mod == "delete")
        checkValidationSummaryErrors();


    if (mod == "create" || mod == "edit") {

    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }
}