$.MMS.ResetPassword = function (mod) {
    mod = mod.toLowerCase();

    $("#formResetPassword input[type=text]").prop("readonly", true);
    $("#formResetPassword input[type=checkbox]").prop("disabled", true);


    if (mod == "create" || mod == "edit" || mod == "delete") {


        validate = true;
        successMsg = "Reset Password";
        errorMsg = "Resetting Password";



        $("#formResetPassword").submit(function (e) {
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
                        msgError(`Error ${errorMsg} the user.`);
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
