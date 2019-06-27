var TableOrdenItems;
var form;
$(document).ready(function () {
    checkValidationSummaryErrors();

    TableOrdenItems = $('#TableOrdenItems').DataTable({
        dom: "<'row'<'col-sm-12'tr>>",
        "scrollY": "50vh",
        "scrollCollapse": true,
        "paging": false,
        "bFilter": false,
        "bSort": false,
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

    //TableOrdenItems.column(0).visible(false);//Oculta Columna de Linea
    if (TrnMode == 2) {
        $("#formOrdenItems").validate({
            onfocusout: false,
            onkeyup: false,
            rules: {
                OrdenComentario: { required: true }
            },
            messages: {
                OrdenComentario: { required: "El campo comentario es requerido" }
            },
            errorPlacement: function (error, element) {
                error.insertAfter(element);
            }
        });

        $("#OrdenComentario").rules("add", { required: true, messages: { required: "El campo comentario es requerido" } })
    }

    datosGrid(dataList);
    updateMaterialTextFields();
});

function datosGrid(dataList) {
    var UltimaLinea = 1;
    $(dataList).each(function (index, item) {

        addRowTableOrdenItems(item);
        UltimaLinea++;
    });

    $("#nroTableOrdenItems").val(UltimaLinea)
}

function addRowTableOrdenItems(datarow) {
    indx = (parseInt($("#nroTableOrdenItems").val()) + 1).toString();
    $("#nroTableOrdenItems").val(indx)

    var makeInput = function (name, readonly = true, extras = '', validate = false) {
        return `<div class="form-group form-float">
                    ${(validate ? `<div class="input-group">` : ``)}
                        <div class="form-line">
                            <input type="text" id="${name}" name="${name}" value="" data-index="index1" ${(readonly ? `readonly="readonly"` : ``)} class="form-control" ${extras}
                        </div>
                    ${(validate ? `</div>` : ``)}
                    <span class="field-validation-error" data-valmsg-for="${name}" data-valmsg-replace="true">
                        <label for="${name}" class="m-b-0"></label>
                    </span>
                </div>`.replaceAll('1', indx);
    }

    var cell0 = makeInput("OrdenItemsLinea1");
    var cell1 = makeInput("ProductoId1");
    var cell2 = makeInput("ProductoDesc1", true, "disabled='disabled'");
    var cell3 = makeInput("OrdenItemsVlr1");
    var cell4 = makeInput("OrdenItemsCant1");
    var cell5 = makeInput("OrdenItemsCantConfirmada1", TrnMode == 2 ? false : true, '', true);
    var cell6 = makeInput("OrdenItemsNroMov1");
    var cell7 = makeInput("CentroCostoID1");

    TableOrdenItems.row.add([cell0, cell1, cell2, cell3, cell4, cell5, cell6, cell7]).draw(false);

    if (TrnMode == 2) {
        $(`input[name*='OrdenItemsCantConfirmada${indx}`).rules("add", {
            required: true,
            messages: {
                required: "Campo de cantidad confirmada es requerido"
            }
        });
    }
    $(`input[name*='OrdenItemsVlr${indx}`).number(true, 2, ',', '.');

    if (datarow != null) {
        $('input[name*="' + ("OrdenItemsLinea1").split("1").join(indx) + '"]').val(datarow.OrdenItemsLinea);
        $('input[name*="' + ("ProductoId1").split("1").join(indx) + '"]').val(datarow.ProductoId);
        $('input[name*="' + ("ProductoDesc1").split("1").join(indx) + '"]').val(datarow.ProductoDesc);
        $('input[name*="' + ("OrdenItemsVlr1").split("1").join(indx) + '"]').val(datarow.OrdenItemsVlr);
        $('input[name*="' + ("OrdenItemsCant1").split("1").join(indx) + '"]').val(datarow.OrdenItemsCant);
        $('input[name*="' + ("OrdenItemsCantConfirmada1").split("1").join(indx) + '"]').val(datarow.OrdenItemsCantConfirmada);
        $('input[name*="' + ("OrdenItemsNroMov1").split("1").join(indx) + '"]').val(datarow.OrdenItemsNroMov);
        $('input[name*="' + ("CentroCostoID1").split("1").join(indx) + '"]').val(datarow.CentroCostoID);

        if (TrnMode == 2) {
            DisponibilidadProducto($('input[name*="' + ("ProductoId1").split("1").join(indx) + '"]'),
                $('input[name*="' + ("OrdenItemsCantConfirmada1").split("1").join(indx) + '"]'),
                datarow.OrdenItemsCant);
        }//if (TrnMode == 2) {
    }//if (datarow != null) {
}

function DisponibilidadProducto(obj_producto, obj_cantconfirmada, cantidad) {
    $.ajax({
        url: "/Movimientos/DisponibilidadProducto",
        type: "get",
        data: { productoId: $(obj_producto).val(), TypeJson: true },
        dataType: 'json',
        success: function (result) {
            var suma = 0;
            $.each(result, function (key, value) {
                suma += value.MovimientoDisponible;
            });
            suma += parseInt($(obj_cantconfirmada).val());
            console.log(suma)
            if (cantidad > suma) {
                $(obj_cantconfirmada).rules("add", {
                    max: suma,
                    messages: {
                        max: "Cantidad maxima: " + suma
                    }
                });
            } else {
                $(obj_cantconfirmada).rules("add", {
                    max: cantidad,
                    messages: {
                        max: "Cantidad maxima: " + cantidad
                    }
                });
            }
        },
        error: function (xhr, status) {
            msgError(status);
        }
    });
}

function CambiaEstado(ordenid, estado) {
    $.ajax({
        url: "../ordenes/CambiaEstadoOrden",
        type: "GET",
        data: { OrdenId: ordenid, estado: estado },
        dataType: 'text',
        success: function (result) {
            if (result) {
                msgSuccess("Se cambio de estado la orden " + ordenid + " correctamente")
                setTimeout(() => location.reload(), 2000)
            } else {
                msgError("Error al cambiar de estado");
            }
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
        }
    });
}