$.MMS.Items = function (mod) {
    mod = mod.toLowerCase();

    $("#MarcaId").select2({
        theme: "bootstrap",
        placeholder: "Search Brand"
    });

    $('#PrecioSugerido').inputmask("numeric", {
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

        $("#frmItems").submit(function (e) {
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
        $("#Codigo").focus();
    }
    else if (mod == "edit") {
        $("#Categoria").focus();
        $("#cardForm input[id=Codigo]").prop("readonly", true);
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=number]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }
};