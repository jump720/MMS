$.MMS.RememberUser = function (mod) {
    mod = mod.toLowerCase();

    //$("#formRememberUser input[type=text]").prop("readonly", true);
    //$("#formRememberUser input[type=checkbox]").prop("disabled", true);


    if (mod == "create" || mod == "edit" || mod == "delete") {


        validate = true;
        successMsg = "An email has been sent to your email with the new password";
        errorMsg = "User not found";



        $("#formRememberUser").submit(function (e) {
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
