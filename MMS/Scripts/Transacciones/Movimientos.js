/* Insert = 1,
Update = 2,
Delete = 3,
Display = 4*/
var TableMovProductos;
var indexTable = 0;
var form;
var ConfigTipoMovEntrada = "";
var ConfigTipoMovSalida = "";
var ConfigTipoMovAjEntrada = "";
var ConfigTipoMovAjSalida = "";

$(document).ready(function () {
    checkValidationSummaryErrors();

    /*Configuración DataTable*/
    TableMovProductos = $('#TableMovProductos').DataTable({
        dom: "<'row'<'col-sm-12'tr>>",
        "scrollY": "50vh",
        "scrollCollapse": true,
        "bSort": false,
        "iDisplayLength": -1,
        oLanguage: {
            sLengthMenu: "Mostrando _MENU_ ",
            sZeroRecords: "No se encontraron registros",
            sInfo: "Mostrando _START_ - _END_ de _TOTAL_ registros",
            sInfoEmpty: "Mostrando 0 - 0 de 0 registros",
            sInfoFiltered: "(Filtrando de _MAX_ registros totales)",
            oPaginate: { sPrevious: "Anterior", sNext: "Siguiente", sFirst: "Primera", sLast: "Ultima" },
            sEmptyTable: "No hay registros",
            sSearch: "Buscar:"
        }
    });

    TableMovProductos.column(6).visible(false);//Oculta Columna de Entrada
    TableMovProductos.column(7).visible(false);//Oculta columna de Disponible de la entrada
    TableMovProductos.column(8).visible(false);//Oculta columna de Movimientolinea

    if (TrnMode == 1 || TrnMode == 2) {
        $.validator.addMethod("cantidad", function (value, element) {
            var estado = $("select[name*='MovimientoEstado'][data-index*='" + $(element).data("index") + "']").val();
            var prod = $("input[name*='ProductoId'][data-index*='" + $(element).data("index") + "']").val();
            //alert("cantidad");
            if (prod == "" || estado == 4) {

            } else {
                return ($(element).val() === "") ? false : true;
            }
            return true;
        }, "Campo Cantidad es requerida");

        $.validator.addMethod("valor", function (value, element) {
            var estado = $("select[name*='MovimientoEstado'][data-index*='" + $(element).data("index") + "']").val();
            var prod = $("input[name*='ProductoId'][data-index*='" + $(element).data("index") + "']").val();
            if (prod == "" || estado == 4) {

            } else {
                //console.log(element + "-" + value);
                return (value == "") ? false : (value == "0") ? false : true;
            }
            return true;
        }, "Campo Valor es requerida");

        $.validator.addMethod("Producto", function (value, element) {
            var estado = $("select[name*='MovimientoEstado'][data-index*='" + $(element).data("index") + "']").val();
            //console.log(element + "-" + value);
            if (value != "" && estado != 4) {
                return $(element).data("valid");
            } else {
                if ($(element).data("mode") == "UPD") {
                    if (!$(element).data("valid") || value == "") {
                        return false;
                    } else {
                        return true;
                    }

                } else {
                    return true;
                }
            }
        }, "Producto No existe o Vacio");

        $.validator.addMethod("entrada", function (value, element) {
            var estado = $("select[name*='MovimientoEstado'][data-index*='" + $(element).data("index") + "']").val();
            var prod = $("input[name*='ProductoId'][data-index*='" + $(element).data("index") + "']").val();
            var tMovimiento = $("#TipoMovimientoID").val();
            if (prod == "" || estado == 4) {

            } else {
                if (tMovimiento == ConfigTipoMovAjSalida || tMovimiento == ConfigTipoMovSalida) {
                    return ($(element).val() == "") ? false : true;
                } else {

                }
            }
            return true;
        }, "Mov. Entrada es requerida");

        $.validator.addMethod("tablaVacia", function (value, element) {
            var nroLineas = 0;
            $("#TableMovProductos tbody input[name*='ProductoId']").each(function () {
                var producto = $(this).val();
                var estado = $("select[name*='MovimientoEstado'][data-index*='" + $(this).data("index") + "']").val();
                //Valida grilla vacia
                if (producto != "" && estado != 4)
                    nroLineas++;
            });

            if (nroLineas == 0)
                return false;
            else
                return true;
        }, "Se debe ingresar al menos un producto");

        $.validator.addMethod("productoRepetido", function (value, element) {
            var nroRepetidos = 0;
            var inputsProductos = parseInt($("#nroTableMovProductos").val());
            //var inputsProductos = $("#TableMovProductos tbody input[name*='ProductoId']");
            $.validator.messages.productoRepetido = "";
            for (var x = 0; x <= inputsProductos; x++) {//inputsProductos.length
                var productoX = $("#ProductoId" + (x + 1)).val();
                var estadoX = $("#MovimientoEstado" + (x + 1)).val();
                //console.log(estadoX);
                if (productoX != "" && estadoX != 4 && estadoX != null && productoX != "undefined" && productoX != null) {
                    for (var y = x + 1; y <= inputsProductos; y++) {//inputsProductos.length
                        var productoY = $("#ProductoId" + (y + 1)).val();
                        var estadoY = $("#MovimientoEstado" + (y + 1)).val();
                        if (productoX == productoY && estadoY != 4) {
                            nroRepetidos++;
                            $.validator.messages.productoRepetido += "Producto: " + productoX + " esta repetido Lineas(" + (x + 1) + "," + (y + 1) + ")\n";
                        }//if (productoX == productoY) {
                    }//if (productoX != "") {
                }//if (productoX != "") {
            }//for (var x = 0; x <= inputsProductos.length; x++) {

            if (nroRepetidos > 0) {//Numero de repetidos mayor a  0
                return false;
            } else {
                return true;
            }
        }, $.validator.messages.productoRepetido);

        $("#formMov").validate({
            onfocusout: false,
            onkeyup: false,
        });

        $("#MovimientoId").rules("add", {
            tablaVacia: true,
            productoRepetido: true
        });

        $("#TipoMovimientoID").rules("add", {
            required: true,
        })

        $("#formMov").submit(function (e) {
            e.preventDefault();

            if (!$(this).valid()) return;

            $("input[name*=MovimientoLinea]").each(function () {
                $(this).removeAttr("readonly");
            });
            $("input[name*=MovimientoIDEntrada]").each(function () {
                $(this).removeAttr("readonly");
            });
            $("input[name*=MovimientoValor]").each(function () {
                $(this).removeAttr("readonly");
            });
            TableMovProductos.column(6).visible(true);
            TableMovProductos.column(8).visible(true);

            $.ajax({
                url: this.action,
                type: this.method,
                data: $(this).serialize(),
                dataType: 'json',
                success: function (result) {
                    if (result.validate) {
                        msgSuccess(result.mensaje);
                        setTimeout(function () {
                            $("#btnCancelMov").click();
                        });
                    } else {
                        if (result.inventario) {
                            msgError(result.mensaje);
                        } else {
                            msgInfo(result.mensaje, 4000);
                            actualizaRuleDisponible();
                        }
                    }
                    configTableTipoMov($("#TipoMovimientoID").val());
                },
                beforeSend: function () {
                    sLoading();
                },
                complete: function () {
                    hLoading();
                },
                error: function (xhr, status) {
                    msgError(status);
                    hLoading();

                    //deja el formulario configurado dependiendo del tipo de movimiento 
                    configTableTipoMov($("#TipoMovimientoID").val());
                }
            });

            return false;
        });

    }//if (TrnMode != 3)
    document.onkeydown = function (e) {
        KeyDownField(e);
    }

    $('#TotVrl').number(true, 2, ',', '.');
    $('#TotCant').val(0);
    $('#TotVrl').val(0);

    // $('#TotCant').number(true, 2);

    $("#TipoMovimientoID").change(function () {
        configTableTipoMov($(this).val());
    })

    ListaTipoMovimiento();
    if (TrnMode == 1) {
        $("#MovimientoId").val(0);
        addRowTableMov();
    }
    else if (TrnMode == 2) {
        datosGrid(dataList);
        sumaValores();
    }
    else if (TrnMode == 3 || TrnMode == 4) {
        datosGrid(dataList);
        sumaValores();
    }

    updateMaterialTextFields();
    $("#TipoMovimientoID").change();
});

function KeyDownField(e) {
    name = e.srcElement.id;
    if (e.ctrlKey == true && e.keyCode == 32) {
        // acción para ctrl + space y evitar que ejecute la acción propia del navegador               
        if (TrnMode != 4) addRowTableMov();
        return false;
    }
}
function promptProductos(i) {
    var tMovimiento = $("#TipoMovimientoID").val();

    if (tMovimiento != "") {
        if (tMovimiento != ConfigTipoMovAjSalida && tMovimiento != ConfigTipoMovSalida) {//&& tMovimiento != ConfigTipoMovSalida
            //prompt de productos
            var estado = $("#MovimientoEstado" + i).val();
            if (estado != 4 && estado != 3) {
                indexTable = i;
                sLoading();
                $.ajax({
                    url: '/productos/_productos',
                    data: {},
                    contentType: 'application/html; charset=utf-8',
                    type: 'GET',
                    dataType: 'html'
                })
                    .success(function (result) {
                        $('#productosModal .modal-body').html(result);
                    })
                    .error(function (xhr, status) {
                        alert(xhr.responseText);
                    })
                    .complete(function (xhr, status) {
                        if (status == "success") {
                            $('#productosModal').modal();
                            hLoading();
                        }
                    });
            }
        } else {
            //prompt de productos con inventario
            var estado = $("#MovimientoEstado" + i).val();

            if ((estado != 4 && estado != 3 && TrnMode != 4)) {
                indexTable = i;
                sLoading();
                $.ajax({
                    url: '/Movimientos/_ListaProductosDisponible',
                    data: {},
                    contentType: 'application/html; charset=utf-8',
                    type: 'GET',
                    dataType: 'html'
                })
                    .success(function (result) {
                        $('#productosModal .modal-body').html(result);
                    })
                    .error(function (xhr, status) {
                        alert(xhr.responseText);
                    })
                    .complete(function (xhr, status) {
                        if (status == "success") {
                            $('#productosModal').modal();
                            hLoading();
                        }
                    });
            }
        }
    } else {
        msgAlert("Primero debe seleccionar un tipo de movimiento")
    }
    $("#ProductoId" + i).blur();
}

function selectProductos(id, desc) {
    $("#ProductoId" + indexTable).val(id);
    $("#ProductoDesc" + indexTable).val(desc);
    $('#productosModal').modal('hide');
    $("#ProductoId" + indexTable).blur();
}

function consultaProducto(o, i) {
    var tMovimiento = $("#TipoMovimientoID").val();
    //var productoID
    var id = $(o).val();

    if (tMovimiento != "" && id != "") {
        var estado = $("select[name*='MovimientoEstado'][data-index*='" + $(o).data("index") + "']").val();
        if (estado != 4) {

            $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']").data("valid", true).removeClass("red-text");
            $("input[name*='ProductoDesc'][data-index*='" + $(o).data("index") + "']").removeClass("red-text");

            if (id != "") {
                $.ajax({
                    url: '../productos/infoProducto',
                    data: { ProductoId: id },
                    contentType: 'application/html; charset=utf-8',
                    type: 'GET',
                    dataType: 'json'
                })
                    .success(function (result) {
                        if (result.ProductoId != null) {
                            $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']").val(result.ProductoId);
                            $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']").data("valid", true);
                            $("input[name*='ProductoDesc'][data-index*='" + $(o).data("index") + "']").val(result.ProductoDesc);
                            $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']").valid();
                            //$("#ProductoId" + indexTable).val(result.ProductoId);
                            //$("#ProductoId" + indexTable).data("valid", true);
                            //$("#ProductoDesc" + indexTable).val(result.ProductoDesc).removeClass("red-text");
                        } else {
                            $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']").data("valid", false);
                            $("input[name*='ProductoDesc'][data-index*='" + $(o).data("index") + "']").val("Producto " + id + " no encontrado").addClass("red-text");
                        }
                    })
                    .error(function (xhr, status) {
                        $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']").data("valid", false);
                        $("input[name*='ProductoDesc'][data-index*='" + $(o).data("index") + "']").val("");
                        msgError(status);
                    });

            } else {
                $("input[name*='ProductoDesc'][data-index*='" + $(o).data("index") + "']").val("");
                //$("#ProductoDesc" + indexTable).val("").removeClass("red-text");
            }//if (id != "")
        }

        if (tMovimiento == ConfigTipoMovAjSalida || tMovimiento == ConfigTipoMovSalida) {
            //prompt de productos con inventario
            var movEntrada = $("#MovimientoIDEntrada" + i).val();
            if (movEntrada == "") {
                promptProductos(i);
            }
        }
    } else {
        if (id != "") {
            msgAlert("Primero debe seleccionar un tipo de movimiento");
        }
    }
}

function addRowTableMov(datarow) {

    indx = (parseInt($("#nroTableMovProductos").val()) + 1).toString();
    $("#nroTableMovProductos").val(indx)
    var tMovimiento = $("#TipoMovimientoID").val();
    var cell0 = "";

    if (TrnMode == 3 || TrnMode == 4) {
        if (tMovimiento == ConfigTipoMovEntrada || tMovimiento == ConfigTipoMovAjEntrada) {
            cell0 = (`<div class="text-center m-t-5">
                        <a id='BtnDetalle1' href="javascript:void(0)" class="btn-table bg-blue btn-sm btn-sm-circle waves-circle waves-effect waves-float"
                            onclick="promptDetalleSalidas(this);" aria-hidden="true" data-index="index1" data-toggle="tooltip" data-container="body" data-placement="top" title="Ver detalle de salidas">
                            <i class="material-icons">list</i>
                        </a>
                    </div>`).replaceAll('1', indx);
        } else {
            cell0 = "";
        }
    } else {
        cell0 = (`<div class="text-center m-t-5">
                    <a id="BtnEliminar1" href="javascript:void(0)" class="btn-table bg-red btn-sm btn-sm-circle waves-circle waves-effect waves-float"
                        onclick="EliminaLinea(this);" aria-hidden="true" data-index="index1">
                        <i class="material-icons">delete</i>
                    </a>
                </div>`).replaceAll('1', indx);
    }

    var cell1 = (`<div class="form-group form-float">
                    <div class="input-group">
                        <div class="form-line">
                            <input type="text" id="ProductoId1" name="ProductoId1" value="" data-index="index1" data-toggle="tooltip" data-container="body" data-placement="top" title="Doble clic para buscar productos"
                                    ondblclick="promptProductos(1);" class="form-control" onblur="consultaProducto(this,1);" />
                        </div>
                    </div>
                    <span class="field-validation-error" data-valmsg-for="ProductoId1" data-valmsg-replace="true">
                        <label for="ProductoId1"></label>
                    </span>
                </div>`).replaceAll('1', indx);

    var cell2 = (`<div class="form-group form-float">
                    <div class="form-line">
                        <input type="text" id="ProductoDesc1" class="form-control" name="ProductoDesc1" value="" disabled="disabled" data-index="index1" />
                    </div>
                </div>`).replaceAll('1', indx);

    var cell3 = `<div class="form-group form-float"><div class="input-group"><div class="form-line">`;
    if (tMovimiento == ConfigTipoMovAjSalida || tMovimiento == ConfigTipoMovSalida) {
        cell3 += (`<input type='text' id='MovimientoValor1' class="form-control" name='MovimientoValor1' value='' data-index='index1' readonly='readonly' />`).replaceAll('1', indx);
    } else {
        cell3 += (`<input type='text' id='MovimientoValor1' class="form-control" name='MovimientoValor1' value='' data-index='index1' />`).replaceAll('1', indx);
    }
    cell3 += `</div></div><span class="field-validation-error" data-valmsg-for="MovimientoValor1" data-valmsg-replace="true">
                <label for="MovimientoValor1"></label>
            </span>`;
    cell3 += `</div>`;

    var cell4 = (`<div class="form-group form-float">
                    <div class="input-group">
                        <div class="form-line">
                            <input type="number" id="MovimientoCantidad1" class="form-control full-width" name="MovimientoCantidad1" value="" data-index="index1" />
                        </div>
                    </div>
                    <span class="field-validation-error" data-valmsg-for="MovimientoCantidad1" data-valmsg-replace="true">
                        <label for="MovimientoCantidad1"></label>
                    </span>
                </div>`).replaceAll('1', indx);

    var cell5 = ("<select id='MovimientoEstado1' class='form-control full-width' name='MovimientoEstado1' data-index='index1'></select>").replaceAll('1', indx);

    var cell6 = (`<div class="form-group form-float">
                    <div class="form-line">
                        <input type="text" id="MovimientoIDEntrada1" class="form-control" name="MovimientoIDEntrada1" value="" data-index="index1" readonly="readonly" />
                    </div>
                </div>`).replaceAll('1', indx);

    var cell7 = (`<div class="form-group form-float">
                    <div class="form-line">
                        <input type="text" id="MovimientoEntradaDisp1" class="form-control" name="MovimientoEntradaDisp1" value="" data-index="index1" disabled="disabled" />
                    </div>
                </div>`).replaceAll('1', indx);

    var cell8 = (`<div class="form-group form-float">
                    <div class="form-line">
                        <input type="text" id="MovimientoLinea1" class="form-control" name="MovimientoLinea1" value="1" data-index="index1" readonly="readonly" />
                    </div>
                </div>`).replaceAll('1', indx);

    TableMovProductos.column(8).visible(true); //Oculta columna de Movimientolinea

    TableMovProductos.row.add([cell0, cell1, cell2, cell3, cell4, cell5, cell6, cell7, cell8]).draw(false);

    if (TrnMode != 3 && TrnMode != 4) {
        $('input[name*="ProductoId' + indx + '"]').rules("add", {
            Producto: true
        });

        $('input[name*="MovimientoCantidad' + indx + '"]').rules("add", {
            cantidad: true
        });

        $('input[name*="MovimientoValor' + indx + '"]').rules("add", {
            valor: true
        });

        var movEntraCol = $('input[name*="MovimientoIDEntrada' + indx + '"]');
        if (movEntraCol.length) {
            movEntraCol.rules("add", {
                entrada: true
            });
        }
    }

    $('[data-toggle="tooltip"]').tooltip();

    //Agregar los options a los select de la grilla
    addSelectEstado($("#MovimientoEstado" + indx));
    if (tMovimiento == ConfigTipoMovAjSalida || tMovimiento == ConfigTipoMovSalida) {
        $("select[name*='MovimientoEstado'] option").each(function () {
            if ($(this).val() == 3) {
                $(this).removeAttr("disabled");
            }
        });
    }

    $('input[name*="' + ("ProductoId1").replaceAll('1', indx) + '"]').focus();

    /*Formato numero jQuery.number*/
    $('input[name*="' + ("MovimientoValor1").replaceAll('1', indx) + '"]').number(true, 2, ',', '.');
    //$('input[name*="' + ("MovimientoCantidad1").split("1").join(idx) + '"]').number(true, 2);
    /*Formato numero jQuery.number*/

    /*Eventos JS campos*/
    $('input[name*="' + ("MovimientoCantidad1").replaceAll('1', indx) + '"]').blur(function () {
        sumaValores();
    });
    $('input[name*="' + ("MovimientoValor1").replaceAll('1', indx) + '"]').blur(function () {
        sumaValores();
    });
    /*Eventos JS campos*/

    if (datarow != null && (TrnMode == 2 || TrnMode == 3 || TrnMode == 4)) {
        $('input[name*="' + ("MovimientoValor1").replaceAll('1', indx) + '"]').val(datarow.MovimientoValor);
        $('input[name*="' + ("MovimientoCantidad1").replaceAll('1', indx) + '"]').val(datarow.MovimientoCantidad);
        $('input[name*="' + ("MovimientoIDEntrada1").replaceAll('1', indx) + '"]').val(datarow.MovimientoIDEntrada);
        $('input[name*="' + ("MovimientoLinea1").replaceAll('1', indx) + '"]').val(datarow.MovimientoLinea);
        $('input[name*="' + ("ProductoId1").replaceAll('1', indx) + '"]').val(datarow.ProductoId);
        $('input[name*="' + ("MovimientoEntradaDisp1").replaceAll('1', indx) + '"]').val(datarow.MovimientoDisponible);
        $('select[name*="' + ("MovimientoEstado1").replaceAll('1', indx) + '"]').val(datarow.MovimientoEstado);
        $('input[name*="' + ("ProductoId1").replaceAll('1', indx) + '"]').blur();

        $('input[name*="' + ("ProductoId1").replaceAll('1', indx) + '"]').data("linea", datarow.MovimientoLinea);

        //data html para saber si es update
        $('input[name*="' + ("ProductoId1").replaceAll('1', indx) + '"]').data("mode", "UPD");

        if (datarow.MovimientoEstado == 3 || datarow.MovimientoEstado == 4 || (TrnMode == 4)) {
            $('input[name*="' + ("MovimientoValor1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
            $('input[name*="' + ("MovimientoCantidad1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
            $('input[name*="' + ("MovimientoIDEntrada1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
            $('input[name*="' + ("ProductoId1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
            $('select[name*="' + ("MovimientoEstado1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
            $('a[id*="' + ("BtnEliminar1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
            $('input[name*="' + ("MovimientoLinea1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
        } else if (datarow.MovimientoEstado == 2) {
            if (tMovimiento == ConfigTipoMovEntrada || tMovimiento == ConfigTipoMovAjEntrada) {
                $('input[name*="' + ("MovimientoValor1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
                $('input[name*="' + ("MovimientoCantidad1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
                $('input[name*="' + ("MovimientoIDEntrada1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
                $('input[name*="' + ("ProductoId1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
                $('select[name*="' + ("MovimientoEstado1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
                $('a[id*="' + ("BtnEliminar1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
                $('input[name*="' + ("MovimientoLinea1").replaceAll('1', indx) + '"]').attr("disabled", "disabled");
            }
        }
    } else {
        //valor de la linea
        $('input[name*="' + ("MovimientoLinea1").replaceAll('1', indx) + '"]').val(indx);
    }

    TableMovProductos.column(8).visible(false);//Oculta columna de Movimientolinea
}

function EliminaLinea(o) {
    var estado = $("select[name*='MovimientoEstado'][data-index*='" + $(o).data("index") + "']").val();
    if (estado != 4) {
        var result = confirm("¿Desea eliminar esta linea?");
        //style : text-decoration: line-through 
        if (result) {
            $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']")
                .attr("readonly", "readonly")
                .css("text-decoration", "line-through ");

            $("input[name*='ProductoDesc'][data-index*='" + $(o).data("index") + "']")
                .removeClass("red-text")
                .css("text-decoration", "line-through ");

            $("input[name*='MovimientoCantidad'][data-index*='" + $(o).data("index") + "']")
                .attr("readonly", "readonly")
                .css("text-decoration", "line-through ");

            $("input[name*='MovimientoValor'][data-index*='" + $(o).data("index") + "']")
                .attr("readonly", "readonly")
                .css("text-decoration", "line-through ");

            $("select[name*='MovimientoEstado'][data-index*='" + $(o).data("index") + "'] option").each(function () {
                if ($(this).val() == 4) {
                    $(this).removeAttr("selected", "");
                    $(this).removeAttr("disabled", "disabled");
                } else {
                    $(this).attr("disabled", "disabled");
                }
            })

            $("select[name*='MovimientoEstado'][data-index*='" + $(o).data("index") + "']")
                .val(4)
                .attr("readonly", "readonly")
                .css("text-decoration", "line-through ");

        }
    } else {
        $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']")
            .removeAttr("readonly")
            .css("text-decoration", "");

        $("input[name*='ProductoDesc'][data-index*='" + $(o).data("index") + "']")
            //.removeClass("red-text")
            .css("text-decoration", "");

        $("input[name*='MovimientoCantidad'][data-index*='" + $(o).data("index") + "']")
            .removeAttr("readonly")
            .css("text-decoration", "");

        $("input[name*='MovimientoValor'][data-index*='" + $(o).data("index") + "']")
            .removeAttr("readonly")
            .css("text-decoration", "");

        $("select[name*='MovimientoEstado'][data-index*='" + $(o).data("index") + "'] option").each(function () {
            if ($(this).val() == 1) {
                $(this).removeAttr("selected", "");
                $(this).removeAttr("disabled", "disabled");
            } else if ($(this).val() == 2) {
                $(this).removeAttr("disabled", "disabled");
            } else {
                $(this).attr("disabled", "disabled");
            }
        })

        $("select[name*='MovimientoEstado'][data-index*='" + $(o).data("index") + "']")
            .val(1)
            .removeAttr("readonly")
            .css("text-decoration", "");
    }
}

function addSelectEstado(o) {
    $(Estados).each(function (index, est) {
        if (est.Value == 3 || est.Value == 4) {
            $(o).append(' <option value="' + est.Value + '" disabled = "disabled">' + est.Text + '</option>')
        }
        else {
            $(o).append(' <option value="' + est.Value + '">' + est.Text + '</option>')
        }
    });
}

function sumaValores() {
    var TotCant = 0;
    var TotVlr = 0;

    $("input[name*='MovimientoCantidad']").each(function () {
        TotVlr += parseInt($(this).val() !== '' ? $(this).val() : 0) *
            parseInt($("input[name*='MovimientoValor'][data-index*='" + $(this).data("index") + "']").val() !== '' ? $("input[name*='MovimientoValor'][data-index*='" + $(this).data("index") + "']").val() : 0)

        TotCant += parseInt($(this).val() !== '' ? $(this).val() : 0);
    });

    $('#TotCant').val(TotCant);
    $('#TotVrl').val(TotVlr);
}


function ListaTipoMovimiento() {
    $(TipoMov).each(function (index, data) {
        ConfigTipoMovEntrada = data.ConfigTipoMovEntrada;
        ConfigTipoMovSalida = data.ConfigTipoMovSalida;
        ConfigTipoMovAjEntrada = data.ConfigTipoMovAjEntrada;
        ConfigTipoMovAjSalida = data.ConfigTipoMovAjSalida;

        configTableTipoMov($("#TipoMovimientoID").val());
    });
}

function selectMovimiento(data) {
    $("#ProductoId" + indexTable).val(data.ProductoId);
    $("#ProductoDesc" + indexTable).val(data.ProductoDesc);
    $("#MovimientoValor" + indexTable).val(data.MovimientoValor);
    $("#MovimientoCantidad" + indexTable).val(data.MovimientoDisponible);
    $("#MovimientoIDEntrada" + indexTable).val(data.MovimientoId);
    $("#MovimientoEntradaDisp" + indexTable).val(data.MovimientoDisponible);

    $("#MovimientoCantidad" + indexTable).rules("add", {
        max: data.MovimientoDisponible
    });

    $('#productosModal').modal('hide');
    $("#ProductoId" + indexTable).blur();
}

function actualizaRuleDisponible() {
    var tMovimiento = $("#TipoMovimientoID").val();
    if (tMovimiento == ConfigTipoMovAjSalida || tMovimiento == ConfigTipoMovSalida) {
        var productos = new Array();
        var idx = 1;
        $("input[name*='ProductoId']").each(function () {
            productos.push({
                MovimientoEntradaId: $("#MovimientoIDEntrada" + idx).val(),
                MovimientoId: $("#MovimientoId").val(),
                ProductoId: $(this).val()
            });

            idx++;
        });
        //console.log(productos);
        $.ajax({
            url: "/Movimientos/DisponibleXProducto",
            type: "post",
            data: { productos: JSON.stringify(productos), mode: TrnMode },
            dataType: 'json',
            success: function (result) {
                var idx = 1;
                $.each(result, function (key, value) {
                    //Quita regla de max a la cantidad
                    $("#MovimientoCantidad" + idx).rules("remove", "max");

                    //Agrega regla de max a la cantidad
                    $("#MovimientoCantidad" + idx).rules("add", {
                        max: value.MovimientoDisponible
                    });
                    $("#MovimientoEntradaDisp" + idx).val(value.MovimientoDisponible);
                    idx++;
                });
            },
            beforeSend: function () {
            },
            complete: function () {
            },
            error: function (xhr, status) {
                msgError(status);
            }
        });
    }
}

function LimpiarCamposRow(i) {
    $("#ProductoDesc" + i).val("");
    $("#MovimientoValor" + i).val("");
    $("#MovimientoCantidad" + i).val("");
    $("#MovimientoIDEntrada" + i).val("");
    $("#MovimientoEntradaDisp" + i).val("");
}

function configTableTipoMov(val) {

    TableMovProductos.column(8).visible(false);
    if (TrnMode == 1 || TrnMode == 2) {
        if (val == ConfigTipoMovAjSalida || val == ConfigTipoMovSalida) {
            //Muestra columna 6 y 7(entrada)
            TableMovProductos.column(6).visible(true);
            TableMovProductos.column(7).visible(true);

            $("input[name*=MovimientoIDEntrada]").each(function () {
                $(this).attr("readonly", "readonly");
            });

            //Agrega propiedad de readonly al valor
            $("input[name*=MovimientoValor]").each(function () {
                $(this).attr("readonly", "readonly");
            });


            //Habilita estado 3 Ejecutado
            $("select[name*='MovimientoEstado'] option").each(function () {
                if ($(this).val() == 3) {
                    $(this).removeAttr("disabled");
                }
            });
        }
        else {
            //Valor de la entrada vacia
            $("input[name*=MovimientoIDEntrada]").each(function () {
                $(this).val("");
            });
            $("input[name*=MovimientoEntradaDisp]").each(function () {
                $(this).val("");
            });
            //Quita propiedad de readonly al valor
            $("input[name*=MovimientoValor]").each(function () {
                $(this).removeAttr("readonly");
            });

            //Quita regla de max a la cantidad
            $("input[name*=MovimientoCantidad]").each(function () {
                $(this).rules("remove", 'max');
            });
            //$("#MovimientoCantidad" + indexTable).rules("remove", "max");

            $("select[name*='MovimientoEstado'] option").each(function () {
                if ($(this).val() == 3) {
                    $(this).attr("disabled", "disabled");
                }
            });

            //Oculta columna 6 y 7(entrada)
            TableMovProductos.column(6).visible(false);
            TableMovProductos.column(7).visible(false);
        }
    }//if (TrnMode == 1 || TrnMode == 2) {
    else {
        if (val == ConfigTipoMovAjSalida || val == ConfigTipoMovSalida) {
            //Muestra columna 6 y 7(entrada)
            TableMovProductos.column(6).visible(true);
            TableMovProductos.column(7).visible(false);

            $("input[name*=MovimientoIDEntrada]").each(function () {
                $(this).attr("readonly", "readonly");
            });

            //Agrega propiedad de readonly al valor
            $("input[name*=MovimientoValor]").each(function () {
                $(this).attr("readonly", "readonly");
            });


            //Habilita estado 3 Ejecutado
            $("select[name*='MovimientoEstado'] option").each(function () {
                if ($(this).val() == 3) {
                    $(this).removeAttr("disabled");
                }
            });
        }
        else {
            //Valor de la entrada vacia
            $("input[name*=MovimientoIDEntrada]").each(function () {
                $(this).val("");
            });
            $("input[name*=MovimientoEntradaDisp]").each(function () {
                $(this).val("");
            });
            //Quita propiedad de readonly al valor
            $("input[name*=MovimientoValor]").each(function () {
                $(this).removeAttr("readonly");
            });
            //Quita regla de max a la cantidad
            //$("#MovimientoCantidad" + indexTable).rules("remove", "max");

            $("select[name*='MovimientoEstado'] option").each(function () {
                if ($(this).val() == 3) {
                    $(this).attr("disabled", "disabled");
                }
            });

            //Oculta columna 6 y 7(entrada)
            TableMovProductos.column(6).visible(false);
            TableMovProductos.column(7).visible(true);
        }
    }
}

function datosGrid(dataList) {
    $(dataList).each(function (index, item) {
        addRowTableMov(item);
    });

    actualizaRuleDisponible();
    $("#nroTableMovProductos").val(UltimaLinea)
}

function promptDetalleSalidas(o) {
    // var productoid = $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']").val();
    var linea = $("input[name*='ProductoId'][data-index*='" + $(o).data("index") + "']").data("linea")
    var movimientoid = $("#MovimientoId").val();
    sLoading();
    $.ajax({
        url: '/Movimientos/_DetalleMovimiento',
        data: { MovimientoId: movimientoid, linea: linea },
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html'

    })
        .success(function (result) {
            $('#movModal').html(result);
        })
        .error(function (xhr, status) {
            alert(xhr.responseText);
        })
        .complete(function (xhr, status) {
            hLoading();
            if (status == "success") {
                $('#_Modal').modal();
            }
        });
}