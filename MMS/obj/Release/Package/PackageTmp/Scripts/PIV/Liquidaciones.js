$.MMS.Liquidaciones = function (mod) {
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

        $("#frmLiquidacion").submit(function (e) {
            e.preventDefault();

            if (validate) {
                if (!$(this).valid())
                    return;

                var fromMonth = $("#MonthFrom").val();
                var fromYear = $("#YearFrom").val();
                var fromDate = new Date(fromYear, parseInt(fromMonth) -1, 1);
                $("#FechaInicial").val(fromDate.toISOString());

                var tillMonth = $("#MonthTill").val();
                var tillYear = $("#YearTill").val();
                var tillDate = new Date(tillYear, tillMonth, 0);
                $("#FechaFinal").val(tillDate.toISOString());
            }

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

        $("#MonthFrom").rules("add", {
            required: true,
            messages: {
                required: "Required."
            }
        });

        $("#YearFrom").rules("add", {
            required: true,
            messages: {
                required: "Required."
            }
        });

        $("#MonthTill").rules("add", {
            required: true,
            min: function () {
                if (parseInt($("#YearTill").val()) > parseInt($("#YearFrom").val()))
                    return 0;
                else
                    return parseInt($("#MonthFrom").val());
            },
            messages: {
                required: "Required.",
                min: "Till Month mustn't be lower that Till Month"
    }
        });

        $("#YearTill").rules("add", {
            required: true,
            min: function () {
                return parseInt($("#YearFrom").val());
            },
            messages: {
                required: "Required.",
                min: "Till Year mustn't be lower that From Year"
            }
        });

        $("#Descripcion").focus();
    }
    else if (mod == "details" || mod == "delete") {
        $("#bodyForm input[type=text]").prop("readonly", true);
        $("#bodyForm select").prop("disabled", true);
    }
};