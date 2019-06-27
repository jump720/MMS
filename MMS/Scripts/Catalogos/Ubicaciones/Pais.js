$.MMS.Pais = function (mod) {
    mod = mod.toLowerCase();

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

        $("#frmPais").submit(function (e) {
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

    if (mod == "create" || mod == "edit") {
        $("#PaisID").focus();
    }
    else if (mod == "details" || mod == "delete") {
        $("#bodyForm input[type=text]").prop("readonly", true);
    }
};