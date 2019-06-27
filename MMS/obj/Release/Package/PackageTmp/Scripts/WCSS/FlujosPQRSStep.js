$.MMS.FlujosPQRSStep = function (mod, motivo, id) {
    mod = mod.toLowerCase();

    //

    //CKEDITOR.replace('Descripcion');
    //CKEDITOR.inline('Descripcion');
    //CKEDITOR.config.height = 300;

    //ClassicEditor
    //    .create(document.querySelector('#Descripcion'))
    //    .catch(error => {
    //        console.error(error);
    //    });

    $("#TipoPaso").select2({
        placeholder: "Search Tipo Paso",
        theme: "bootstrap",
        width: '100%'
    })

    $("#EnviaCorreoDestinatarios").select2({
        placeholder: "Select ",
        theme: "bootstrap",
        width: '100%'
    })

    if (mod == "create") {
        $("#TipoPaso").find("option").eq(0).remove();//Remueve primer option vacio
    } else if (mod == "delete") {
        $("#formStep input[type=text]").prop("readonly", true);
        $("#formStep input[type=checkbox]").prop("disabled", true);
        $("#formStep select").prop("disabled", true);
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


        $("#formStep").submit(function (e) {
            e.preventDefault();

            if (validate)
                if (!$(this).valid())
                    return;



            sLoading();
            if (mod == "delete") {
                $.post(this.action + "?MotivoPQRSId=" + motivo + "&Id=" + id)
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
            } else {
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
            }
        });
    }

}
