$.MMS.Presupuesto = function (mod, presupuesto) {
    mod = mod.toLowerCase();


    $("#PresupuestoVendedorAno").select2({
        placeholder: "- Ano -",
        theme: "bootstrap"
    })

    $("#PresupuestoVendedorMes").select2({
        placeholder: "- Quarte -",
        theme: "bootstrap"
    })

    $("#CentroCostoID").select2({
        placeholder: "Centro de Custo",
        theme: "bootstrap"
    })

    $("#PlantaID").select2({
        placeholder: "Planta",
        theme: "bootstrap"
    })

    $("#CanalID").select2({
        placeholder: "Canal",
        theme: "bootstrap"
    })

    $('#PresupuestoValor').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    $('#PresupuestoGasto').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    //$('#SellOutYTD').inputmask("numeric", {
    //    radixPoint: ",",
    //    groupSeparator: ".",
    //    digits: 0,
    //    autoGroup: true,
    //    autoUnmask: true
    //});

    //$('#SellInYTD').inputmask("numeric", {
    //    radixPoint: ",",
    //    groupSeparator: ".",
    //    digits: 0,
    //    autoGroup: true,
    //    autoUnmask: true
    //});

    var template = function (data) {
        return '<div class="select2-result-repository clearfix" style="width: 50%">' +
            `<div><b>${data.ClienteID} - ${data.ClienteRazonSocial}</b></div>` +
            "<div>" +
            `<small>Zone: ${data.Zona}</small><br>` +
            `<small>Channel: ${data.Canal}</small><br>` +
            `<small>Seller: ${data.Vendedor}</small>` +
            "</div>" +
            '</div>';
    };


    var select2Options = {
        placeholder: "Search Customer",
        theme: "bootstrap",
        ajax: {
            type: "GET",
            url: function (params) {
                return "/api/Devoluciones/BuscarCliente?q=" + encodeURIComponent(params.term)
            },
            delay: 300,
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.ClienteRazonSocial,
                            id: item.ClienteID,
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

            return template(repo.data);
        },
        templateSelection: function (repo) {
            if (!repo.id)
                return repo.text;

            return template(repo.data);
        },
        escapeMarkup: function (m) { return m; }
    };


    //if (mod == 'create' && presupuesto !== null) {

    //    $.get("/api/Devoluciones/GetCliente/" + presupuesto.ClienteID)
    //        .done(function (result) {
    //            if (result) {
    //                var _clientes = [];
    //                _clientes.push({
    //                    text: result.ClienteRazonSocial,
    //                    id: result.ClienteID,
    //                    data: result
    //                });

    //                select2Options.data = _clientes;
                    

    //                $("#ClienteID").select2(select2Options);
    //            }
    //        })
    //        .fail(function (jqXHR, textStatus, errorThrown) {
    //            msgError(errorThrown);
    //        });
    //} else {
    //    $("#ClienteID").select2(select2Options);
    //}


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

        $("#frmPresupuesto").submit(function (e) {
            e.preventDefault();
            if (validate && !$(this).valid())
                return;

            $("#PresupuestoVendedorAno").prop("disabled", false);
            $("#PresupuestoVendedorMes").prop("disabled", false);
            $("#PlantaID").prop("disabled", false);
            $("#CentroCostoID").prop("disabled", false);
            $("#PresupuestoGasto").prop("readonly", false);

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
                    $("#PresupuestoVendedorAno").prop("disabled", true);
                    $("#PlantaID").prop("disabled", true);
                    $("#CentroCostoID").prop("disabled", true);
                    $("#PresupuestoGasto").prop("readonly", true);
                });
        });
    }

    if (mod === "create" ) {
        //$("#Nombre").focus();
       
    }

    else if (mod === "edit") {
        $("#PresupuestoVendedorAno").prop("disabled", true);
        $("#PlantaID").prop("disabled", false);
        $("#CentroCostoID").prop("disabled", true);
        $("#PresupuestoGasto").prop("readonly", true);
    }
    else if (mod === "details" || mod === "delete") {
        $("#bodyForm input[type=text]").prop("readonly", true);
        $("#bodyForm select").prop("disabled", true);
    }
};



//function _verDetallePresupuesto(filaActual, cliente, ano) {

//    $.ajax({
//        cache: false,
//        type: 'GET',
//        url: "/PresupuestoVendedor/obtenerMesesPresupuesto",
//        data: { cliente: cliente, ano: ano },
//        success: function (data) {
//            var tr = $(filaActual).closest('tr');
//            var row = $('#tablePresupuesto').DataTable().row(tr);
//            if (row.child.isShown()) {
//                // This row is already open - close it
//                row.child.hide();
//                tr.removeClass('shown');
//            }
//            else {
//                // Open this row
//                var resultStr = "";
//                $.each(data, function (id, option) {
//                    resultStr += '<tr>' +
//                                    '<td>' + 'Q' + option.mes + '</td>' +
//                                    '<td id="P' + option.mes + '" width="24%">' + option.valor + '</td>' +
//                                    '<td id="G' + option.mes + '" width="18%">' + option.gasto + '</td>' +
//                                '</tr>'
//                });
//                //resultStr = resultStr.replace("Q1", "Q1").replace("Q1", "Q1").replace("Q1", "Q1").replace("Q1", "Q1");
//                //Se construye la tabla con detalle
//                row.child('<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px; width:81%"><tr><th rowspan="13" style="width:14%; text-align:center">Q</th></tr>' + resultStr + '</table>').show();
//                tr.addClass('shown');
//                //Dar formato de números
//                for (i = 1; i <= 12; i++) {
//                    $('#P' + i).number(true, 0, ',', '.');
//                    $('#G' + i).number(true, 0, ',', '.');
//                }
//            }
//        },
//        error: function (xhr, ajaxOptions, thrownError) {
//            alert('Error al cargar presupuesto de los meses!!');
//        }

//    });


//    //$("#table").DataTable();


//};

//function _verDetalleVentas(filaActual, cliente, ano) {

//    $.ajax({
//        cache: false,
//        type: 'GET',
//        url: "/VentasxClientes/obtenerMesesVentas",
//        data: { cliente: cliente, ano: ano },
//        success: function (data) {
//            var tr = $(filaActual).closest('tr');
//            var row = $('#tablePresupuesto').DataTable().row(tr);
//            if (row.child.isShown()) {
//                // This row is already open - close it
//                row.child.hide();
//                tr.removeClass('shown');
//            }
//            else {
//                // Open this row
//                var resultStr = "";
//                $.each(data, function (id, option) {
//                    var mesStr = "";
//                    switch (option.mes) {
//                        case 1: mesStr = "Enero";
//                            break;
//                        case 2: mesStr = "Febrero";
//                            break;
//                        case 3: mesStr = "Marzo";
//                            break;
//                        case 4: mesStr = "Abril";
//                            break;
//                        case 5: mesStr = "Mayo";
//                            break;
//                        case 6: mesStr = "Junio";
//                            break;
//                        case 7: mesStr = "Julio";
//                            break;
//                        case 8: mesStr = "Agosto";
//                            break;
//                        case 9: mesStr = "Septiembre";
//                            break;
//                        case 10: mesStr = "Octubre";
//                            break;
//                        case 11: mesStr = "Noviembre";
//                            break;
//                        case 12: mesStr = "Diciembre";
//                            break;
//                    }


//                        resultStr += '<tr>' +
//                                        '<td>' + mesStr + '</td>' +
//                                        '<td id="VentaTD' + option.mes + '" width="35%">' + $.number(parseInt(option.valor), 0, ',', '.') + '</td>' +
//                                    '</tr>'



//                });
//                //resultStr = resultStr.replace("Mes1", "Enero").replace("Mes2", "Febrero").replace("Mes3", "Marzo").replace("Mes4", "Abril").replace("Mes5", "Mayo")
//                //.replace("Mes6", "Junio").replace("Mes7", "Julio").replace("Mes8", "Agosto").replace("Mes9", "Septiembre").replace("Mes10", "Octubre")
//                //    .replace("Mes11", "Noviembre").replace("Mes12", "Diciembre");


//                //Se construye la tabla con detalle
//                row.child('<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px; width:100%"><tr><th rowspan="13" style="width:20%; text-align:center">Meses</th></tr>' + resultStr + '</table>').show();
//                tr.addClass('shown');
//                //Dar formato de números
//                //for (i = 1; i <= 12; i++) {
//                //    $('#VentaTD' + i).number(true, 0, ',', '.');
//                //}
//            }
//        },
//        error: function (xhr, ajaxOptions, thrownError) {
//            alert('Error al cargar presupuesto de los meses!!');
//        }
//    });
//};