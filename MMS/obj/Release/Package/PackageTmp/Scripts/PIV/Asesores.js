$.MMS.Asesores = function (mod, cde) {
    mod = mod.toLowerCase();

    var select2Ciudad = function (data) {
        $("#CiudadId").select2({
            theme: "bootstrap",
            placeholder: "Select City",
            data: data
        });
    }

    var select2Departamento = function (data) {
        $('#DepartamentoId')
            .select2({
                theme: "bootstrap",
                placeholder: "Select State",
                data: data
            });
    }

    $("#PaisId").select2({
        theme: "bootstrap",
        placeholder: "Select Country"
    }).change(function () {
        if (!$(this).val())
            return;

        $.get("/api/Pais/Departamentos/" + $(this).val())
            .done(function (result) {
                if (result) {
                    result.map(function (item) {
                        item.id = item.Id;
                        item.text = item.Nombre;
                        return item;
                    });

                    $('#DepartamentoId').select2("destroy").find("option").remove();
                    select2Departamento(result);
                    $('#DepartamentoId').val("").change();

                    $('#CiudadId').select2("destroy").find("option").remove();
                    select2Ciudad(undefined);
                    $('#CiudadId').val("").change();
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });
    });

    $("#DepartamentoId").change(function () {
        if (!$(this).val() || !$("#PaisId").val())
            return;

        $.get("/api/Departamento/Ciudades/" + $(this).val() + "?paisId=" + $("#PaisId").val())
            .done(function (result) {
                if (result) {
                    result.map(function (item) {
                        item.id = item.Id;
                        item.text = item.Nombre;
                        return item;
                    });

                    $('#CiudadId').select2("destroy").find("option").remove();
                    select2Ciudad(result);
                    $('#CiudadId').val("").change();
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });
    });

    select2Departamento(undefined);
    select2Ciudad(undefined);
    $('#Meta').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    if (mod == "create") {
        $("#PaisId").val("").change();
    }

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

        $("#frmAsesor").submit(function (e) {
            e.preventDefault();

            if (validate)
                if (!$(this).valid())
                    return;

            replaceCommas("#Meta");

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

    var getCategoriaIcon = function (meta) {
        $.get("/api/ColeccionesPIV/GetCategoriaCDEIcon?meta=" + meta.replace(/\./g, ''))
            .done(function (result) {
                if (result)
                    $("#CategoriaCDEIcon").attr("src", `/Content/dist/images/levels/${result}.png`);
                else
                    $("#CategoriaCDEIcon").removeAttr("src");
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            })
            .always(function () {
                hLoading();
            });
    }

    if (mod == "create" || mod == "edit") {
        if (cde) {
            $("#Meta").change(function () {
                if (!$(this).valid()) {
                    $("#CategoriaCDEIcon").removeAttr("src");
                    return;
                }

                getCategoriaIcon(this.value);
            });

            if (mod == "edit")
                getCategoriaIcon($("#Meta").val());
        }

        $("#Cedula").focus();
    }
    else if (mod == "details" || mod == "delete") {
        $("#bodyForm input[type=text]").prop("readonly", true);
        $("#bodyForm select").prop("disabled", true);

        if (cde)
            getCategoriaIcon($("#Meta").val());
    }
};