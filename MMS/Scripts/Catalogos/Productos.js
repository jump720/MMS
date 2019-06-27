﻿$.MMS.Productos = function (mod) {
    mod = mod.toLowerCase();

    $("#TipoProductoID").select2({ minimumResultsForSearch: -1 });
    $("#TipoProductoID").find("option").eq(0).remove();//Remueve primer option vacio

    $('#ProductoPrecio').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    if (mod == "create" || mod == "edit" || mod == "delete") {
        var successMsg, errorMsg, validate;

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

        $("#frmProductos").submit(function (e) {
            e.preventDefault();

            if (validate && !$(this).valid())
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

    if (mod == "create") {
        $("#ProductoId").focus();
    }
    else if (mod == "edit") {
        $("#cardForm input[id=ProductoId]").prop("readonly", true);
        $("#ProductoDesc").focus();
    }
    else if (mod == "details" || mod == "delete") {
        $("#bodyForm input[type=text]").prop("readonly", true);
        $("#TipoProductoID").prop("disabled", true);
    }
};