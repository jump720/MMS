$.MMS.UsuarioFlujoPQRS = function (mod) {
    mod = mod.toLowerCase();

   

    tableUsuarios = $("#tableUsuarios").dataTable({
        scrollY: '55vh',
        scrollCollapse: true,
        searching: false,
        paging: false
    });

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

            var cbSelected = $('input[name*=check]:checked').length;
           
            if (cbSelected <= 0) {
                msgError(`Debe seleccionar al menos un usuario`);
                return;
            }
                

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