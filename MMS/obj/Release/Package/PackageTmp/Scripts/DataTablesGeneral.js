//var table;
$(document).ready(function () {
    $('#table')
        .dataTable({
            dom: `<'row'<'col-sm-6'l><'col-sm-6'f>>` +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-5'i><'col-sm-7'p>>",
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

    $('#modalTable').dataTable({
        //                bJQueryUI: true,
        dom: `<'row'<'col-sm-6'l><'col-sm-6'f>>` +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-5'i><'col-sm-7'p>>",
        oLanguage: {
            sLengthMenu: "Mostrando _MENU_ registros por pagina",
            sZeroRecords: "No se encontraron registros",
            sInfo: "Mostrando _START_ - _END_ de _TOTAL_ registros",
            sInfoEmpty: "Mostrando 0 - 0 de 0 registros",
            sInfoFiltered: "(Filtrando de _MAX_ registros totales)",
            oPaginate: { sPrevious: "Anterior", sNext: "Siguiente", sFirst: "Primera", sLast: "Ultima" },
            sEmptyTable: "No hay registros",
            sSearch: "Buscar:"
        }
    });

    $("select[name=table_length]").val('10'); //seleccionar valor por defecto del select
    $('select[name=table_length]').addClass("browser-default"); //agregar una clase de materializecss de esta forma ya no se pierde el select de numero de registros.
    $("select[name=modalTable_length]").val('10'); //seleccionar valor por defecto del select
    $('select[name=modalTable_length]').addClass("browser-default"); //agregar una clase de materializecss de esta forma ya no se pierde el select de numero de registros.

    if (typeof $('select').material_select === "function") {
        $('select').material_select(); //inicializar el select de materialize
    }
})