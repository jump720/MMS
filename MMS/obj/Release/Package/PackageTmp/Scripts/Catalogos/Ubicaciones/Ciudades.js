$.MMS.Ciudades = function (mod) {
    mod = mod.toLowerCase();

    $("#PaisID").select2({
        theme: "bootstrap",
        placeholder: "Search Pais"
    });
    $("#DepartamentoID").select2({
        theme: "bootstrap",
        placeholder: "Search Departamento"
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

        $("#frmCiudad").submit(function (e) {
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
        $("#CiudadID").focus();
    }
    else if (mod == "edit") {
        $("#CiudadDesc").focus();
        $("#cardForm input[id=CiudadID]").prop("readonly", true);
        $("#cardForm select").prop("disabled", true);
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }
};

function _DropDownInCascada() {

    //Si no se selecciona un país se deshabilitan los departamentos
    if ($("#PaisID").val() == '0' || $("#PaisID").val() == "") {
        document.getElementById("DepartamentoID").disabled = true;
        return;
    }
    var itemSeleccionado = $("#PaisID").val();
    var ddlModel = $("#DepartamentoID");
    
    $.ajax({
        cache: false,
        type: "GET",
        url: "/Ciudad/GetDropDownInCascada",
        data: { "id": itemSeleccionado },
        success: function (data) {
            document.getElementById("DepartamentoID").disabled = false;
            ddlModel.html('');
            ddlModel.append($("<option value=''>Seleccione una Opción</option>"));
            $.each(data, function (id, option) {

                ddlModel.append($('<option></option>').val(option.id).html(option.name));
            });
            if ($("#DepartamentoID").val() == null) {
                document.getElementById("DepartamentoID").disabled = true;
                ddlModel.append($("<option value='0'>Sin Items.</option>"));
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            Materialize.toast("Error al cargar los items!!", 5000);
        }
    });
};