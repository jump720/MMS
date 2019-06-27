$.MMS.Gastos = function (mod, gasto) {
    mod = mod.toLowerCase();

   


   

    $("#TipoGastoID").select2({
        placeholder: "- Select -",
        theme: "bootstrap"
    })

    $("#CentroCostoID").select2({
        placeholder: "- Select -",
        theme: "bootstrap"
    })



    if (mod == "create" || mod == "details")
        $("#GastoEstado").select2({
            placeholder: "- Select -",
            theme: "bootstrap"
        })


    templateItems = function (data) {
        return '<div class="select2-result-repository clearfix" style="width: 100%">' +
            `<div><b>${data.ProductoId} - ${data.ProductoDesc}</b></div>` +
            `<div></div>` +
            '</div>';
    };


    select2ItemsOptions = {
        placeholder: "Search Product",
        theme: "bootstrap",
        data: [],
        ajax: {
            type: "GET",
            url: function (params) {
                return "/api/Productos/BuscarProductoPOP?q=" + encodeURIComponent(params.term)
            },
            delay: 300,
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.ProductoDesc,
                            id: item.ProductoId,
                            data: item
                        }
                    }),
                    pagination: { more: (params.page * 30) < data.total_count }
                };
            }
        },
        minimumInputLength: 1,
        templateResult: function (repo) {

            if (repo.loading)
                return repo.text;

            return templateItems(repo.data);
        },
        templateSelection: function (repo) {

            if (!repo.id)
                return repo.text;

            return templateItems(repo.data);
        },
        escapeMarkup: function (m) { return m; }
    };

    if (gasto != null) {
        $.get("/api/Productos/GetProductoPOP/" + gasto.ProductoId)
           .done(function (result) {
               if (result) {
                   var _productos = [];
                   _productos.push({
                       text: result.ProductoDesc,
                       id: result.ProductoId,
                       data: result
                   });

                   select2ItemsOptions.data = _productos;
                   //$("#Devolucion_ClienteId").val(result.ClienteID).change();

                   $("#ProductoId").select2(select2ItemsOptions);
               }
           })
           .fail(function (jqXHR, textStatus, errorThrown) {
               msgError(errorThrown);
           });
    } else
        $("#ProductoId").select2(select2ItemsOptions);



    
    //$('#GastoValor').inputmask("decimal", {
    //    radixPoint: ",",
    //    groupSeparator: ".",
    //    digits: 2,
    //    autoGroup: true,
    //    autoUnmask: true
    //});


    $('#GastoTotal').inputmask("decimal", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 2,
        autoGroup: true,
        autoUnmask: true
    });
    
    $('#GastoValor').keyup(function () {
        CalcTotal();
    });

    $('#GastoCant').keyup(function () {
        CalcTotal();
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

        $("#frmGasto").submit(function (e) {


            e.preventDefault();

            if (validate && !$(this).valid())
                return;

            if (mod == "edit") {
                if (($('#GastoTotalOld').val() - $('#GastoTotal').val()) < 0 && (gasto.ActividadId != null && gasto.ActividadId != "")) {
                    msgError("Total cant´t be bigger than " + $('#GastoTotalOld').val());
                    return;

                }
            }
            $('#GastoValor').val($('#GastoValor').val().toString().replace(",", "."));

            $("#GastoId").prop("readonly", false);
            $("#TipoGastoID").prop("disabled", false);
            $("#CentroCostoID").prop("disabled", false);
            $("#ProductoId").prop("disabled", false);
            $("#ActividadId").prop("readonly", false);
            //$("#actividad_ActividadTitulo").prop("readonly", false);
            //$("#actividad_cliente_ClienteRazonSocial").prop("readonly", false);

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
                    $("#GastoId").prop("readonly", true);
                    $("#TipoGastoID").prop("disabled", true);
                    $("#CentroCostoID").prop("disabled", true);
                    $("#ProductoId").prop("disabled", true);
                    $("#ActividadId").prop("readonly", true);
                    //$("#actividad_ActividadTitulo").prop("readonly", true);
                    //$("#actividad_cliente_ClienteRazonSocial").prop("readonly", true);
                });
        });
    }


    $('#GastoTotal').prop("readonly", true);

    if (mod == "create") {
        //$("#Nombre").focus();
        $("#GastoEstado").find("option").eq(0).remove();//Remueve primer option vacio
       
    }
    else if (mod == "edit") {
        $('#GastoTotalOld').val($('#GastoValor').val() * $('#GastoCant').val());
        CalcTotal();
        $("#GastoId").prop("readonly", true);
        $("#TipoGastoID").prop("disabled", true);
        $("#CentroCostoID").prop("disabled", true);
        $("#ProductoId").prop("disabled", true);
        $("#ActividadId").prop("readonly", true);
        $("#ActividadTitulo").prop("readonly", true);
        $("#ClienteRazonSocial").prop("readonly", true);

      
    }
    else if (mod == "details" || mod == "delete") {
        CalcTotal();
        $("#bodyForm input[type=text]").prop("readonly", true);
        $("#bodyForm input[type=number]").prop("readonly", true);
        $("#bodyForm select").prop("disabled", true);
        $("#bodyForm textarea").prop("readonly", true);

    }
};

function CalcTotal() {
    var total = ($('#GastoValor').val().toString().replace(",", ".") * $('#GastoCant').val()).toFixed(2);
    $('#GastoTotal').val(total);
}