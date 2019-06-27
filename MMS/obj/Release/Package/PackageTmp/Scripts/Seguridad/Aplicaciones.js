$.MMS.Aplicaciones = function (mod) {
    mod = mod.toLowerCase();

    tableObjetos = $("#tableObjetos").dataTable({
        scrollY: '55vh',
        scrollCollapse: true,
        searching: false,
        paging: false
    });

    if (mod == "create" || mod == "edit" || mod == "delete")
        checkValidationSummaryErrors();

    if (mod == "create" || mod == "edit") {
       
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
    }

   

}