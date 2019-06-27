$.MMS.Correos= function (mod, correoid) {
    mod = mod.toLowerCase();


    if (mod === "create" || mod === "edit" || mod === "delete") {
        var successMsg, errorMsg, validate;

        if (mod === "delete") {
            validate = false;
            successMsg = "deleted";
            errorMsg = "deleting";
        }
        else {
            validate = true;
            successMsg = "saved";
            errorMsg = "saving";
        }

        $("#frmCorreos").submit(function (e) {


            e.preventDefault();

            if (validate && !$(this).valid())
                return;

            //$("#presupuestovendedorano").prop("disabled", false);
            //$("#presupuestovendedormes").prop("disabled", false);
            //$("#clienteid").prop("disabled", false);
            //$("#centrocostoid").prop("disabled", false);
            //$("#presupuestogasto").prop("readonly", false);

            sLoading();
            $.post(this.action, validate ? $(this).serialize() : null)
                .done(function (result) {

                    if (result.Res) {
                        hModal(getCurrentModalId());
                        msgSuccess(`Record ${successMsg}.`);
                    }
                    else {
                        msgError(`Error ${errorMsg} the record.`);
                        msgError(result.Msg);
                    }


                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    msgError(jqXHR);
                    msgError(textStatus);


                })
                .always(function () {
                    hLoading();
                    //$("#PresupuestoVendedorAno").prop("disabled", true);
                    //$("#PresupuestoVendedorMes").prop("disabled", true);
                    //$("#ClienteID").prop("disabled", true);
                    //$("#CentroCostoID").prop("disabled", true);
                    //$("#PresupuestoGasto").prop("readonly", true);
                });
        });
    }

    if (mod === "create") {
        //$("#Nombre").focus();

    }
    else if (mod === "edit") {
        //$("#PresupuestoVendedorAno").prop("disabled", true);
        //$("#PresupuestoVendedorMes").prop("disabled", true);
        //$("#ClienteID").prop("disabled", true);
        //$("#CentroCostoID").prop("disabled", true);
        //$("#PresupuestoGasto").prop("readonly", true);
    }
    else if (mod === "details" || mod === "delete") {
        //$("#bodyForm input[type=text]").prop("readonly", true);
        //$("#bodyForm select").prop("disabled", true);
    }
};