$.MMS.Clientes = function (mod) {
    mod = mod.toLowerCase();

    $("#PaisID").select2({
        theme: "bootstrap",
        placeholder: "Search Pais"
    });

    $("#DepartamentoID").select2({
        theme: "bootstrap",
        placeholder: "Search Departamento"
    });

    $("#CiudadID").select2({
        theme: "bootstrap",
        placeholder: "Search Ciudad"
    });

    $("#VendedorId").select2({
        theme: "bootstrap",
        placeholder: "Search Vendedor"
    });

    $("#CanalID").select2({
        theme: "bootstrap",
        placeholder: "Search Canal"
    });

    $("#ColeccionPIVId").select2({
        theme: "bootstrap",
        placeholder: "Search Colección PIV"
    });

    $("#ClienteAprobacion").click(function () {
        if ($("#ClienteAprobacion").is(':checked')) {
            $("#ClienteAprobacion").val("true");
        } else {
            $("#ClienteAprobacion").val("false");
        }
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

        $("#frmCliente").submit(function (e) {
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
        $("#ClienteID").focus();
    }
    else if (mod == "edit") {
        $("#ClienteNit").focus();
        $("#cardForm input[id=ClienteID]").prop("readonly", true);
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }
};

function _DropDownInCascada(listas) {

    var splitReult = listas.split(";");
    var str1 = splitReult[0];
    var str2 = splitReult[1];
    var str3 = splitReult[2];
    //Variable para cargar el valor del país
    var idPais = "";

    //Si no se selecciona un país se deshabilitan los departamentos O
    //si no se selecciona un departamento se deshabilitan las ciudades
    if ($("#" + str1).val() == '0' || $("#" + str1).val() == "") {
        document.getElementById(str2).disabled = true;
        return;
    }
    //Para deshabilitar el campo de Ciudad al elegir el país
    if (str1 == "PaisID" && $("#CiudadID").val() != "") {
        $("#CiudadID").html('');
        document.getElementById("CiudadID").disabled = true;
    }
    if (str3 != null) {
        idPais = $("#" + str3).val();
    }
    var itemSeleccionado = $("#" + str1).val();
    var ddlModel = $("#" + str2);

    $.ajax({
        cache: false,
        type: "GET",
        url: "/Clientes/GetDropDownInCascada",
        data: { "id": itemSeleccionado, "tipo": str1, "idPais": idPais },
        success: function (data) {
            document.getElementById(str2).disabled = false;
            ddlModel.html('');
            ddlModel.append($("<option value=''>Seleccione una Opción</option>"));
            $.each(data, function (id, option) {

                ddlModel.append($('<option></option>').val(option.id).html(option.name));
            });
            if ($("#" + str2).val() == null) {
                document.getElementById(str2).disabled = true;
                ddlModel.append($("<option value='0'>Sin Items.</option>"));
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            Materialize.toast("Error al cargar los items!!", 5000);
        }
    });
};