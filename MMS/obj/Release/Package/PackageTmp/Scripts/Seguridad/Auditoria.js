$(document).ready(function () {
    var tableAuditoriaDataTable = $('#tableAuditoria').dataTable({
        //                bJQueryUI: true,
        "processing": true,
        "serverSide": true,
        "ajax": "/api/AuditoriaApi/Get",
        columns: [
            { data: "AuditoriaId" },
            {
                data: "AuditoriaFecha",
                render: function (data) {
                    var fecha = new Date(data.replace("-", "/").substring(0, 10));
                    var day = (fecha.getDate().toString().length == 1) ? '0' + fecha.getDate().toString() : fecha.getDate().toString();
                    var mes = fecha.getMonth() + 1;
                    var mes = (mes.toString().length == 1) ? '0' + mes.toString() : mes;
                    var data = day + '/' + mes + '/' + fecha.getFullYear();
                    return '<p>' + data + '</p>';
                }
            },
            { data: "AuditoriaHora" },
            { data: "usuarioId" },
            { data: "AuditoriaEvento" },
            { data: "AuditoriaDesc" },
            { data: "ObjetoId" },
            { data: "AuditoriaEquipo" }
        ],
        sDom: '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
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
        //,
        //"fnServerParams": function (aoData) {
        //    aoData.push({ "name": "s_fecha", "value": $('#s_fecha').val() });
        //    aoData.push({ "name": "s_usuarios", "value": $('#s_usuarios').val() });
        //    aoData.push({ "name": "s_evento", "value": $('#s_evento').val() });
        //    aoData.push({ "name": "s_descripcion", "value": $('#s_descripcion').val() });
        //    aoData.push({ "name": "s_objeto", "value": $('#s_objeto').val() });

        //}
    });


    //$("#s_fecha, #s_usuarios, #s_evento, #s_descripcion, #s_objeto").bind('keyup', function (event) {
    //    tableAuditoriaDataTable.fnDraw();
    //});


    $("select[name=tableAuditoria_length]").val('10'); //seleccionar valor por defecto del select
    $('select[name=tableAuditoria_length]').addClass("browser-default");
});