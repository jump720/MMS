$.MMS.ChangePassword = function (mod) {
    mod = mod.toLowerCase();

    //$("#formChangePassword input[type=text]").prop("readonly", true);
    //$("#formChangePassword input[type=checkbox]").prop("disabled", true);


    if (mod == "create" || mod == "edit" || mod == "delete") {


        validate = true;
        successMsg = "Changed password";
        errorMsg = "Changing password";



        $("#formChangePassword").submit(function (e) {
            e.preventDefault();

            if (validate)
                if (!$(this).valid())
                    return;



            sLoading();

            $.post(this.action, validate ? $(this).serialize() : null)
                .done(function (result) {
                    if (result) {
                        hModal(getCurrentModalId());
                        msgSuccess(`Success, ${successMsg}.`);
                    }
                    else
                        msgError(`Error, ${errorMsg} `);
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
