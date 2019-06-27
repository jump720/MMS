$.MMS.AsignarAnalista = function (mod) {
    mod = mod.toLowerCase();


    if (mod == "create") {
       // $("#TipoPaso").find("option").eq(0).remove();//Remueve primer option vacio
    } else if (mod == "delete") {
        $("#formAsignar input[type=text]").prop("readonly", true);
        $("#formAsignar input[type=checkbox]").prop("disabled", true);
        $("#formAsignar select").prop("disabled", true);
    }

    if (mod == "create" || mod == "edit" || mod == "delete") {

        if (mod == "delete") {
            validate = false;
            successMsg = "deleted";
            errorMsg = "deleting";
        }
        else {
            validate = true;
            successMsg = "saved";
            errorMsg = "saving";
        }


        $("#formAsignar").submit(function (e) {
            e.preventDefault();

            if (validate)
                if (!$(this).valid())
                    return;



            sLoading();

            $.post(this.action, validate ? $(this).serialize() : null)
                .done(function (result) {
                    if (result) {
                        hModal(getCurrentModalId());
                        msgSuccess(`Record ${successMsg}.`);
                    }
                    else
                        msgError(`Error ${errorMsg} the record.`);
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    msgError(errorThrown);
                })
                .always(function () {
                    hLoading();
                });

        });
    }

}
