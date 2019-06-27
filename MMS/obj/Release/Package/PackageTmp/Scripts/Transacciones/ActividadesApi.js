$(document).ready(function () {

    var estado = "";
    var tableActividades = $('#tableActividades').dataTable({
        //                bJQueryUI: true,
        "processing": true,
        "serverSide": true,
        "ajax": {
            url: "/api/ActividadesApi/Get",
            type: "POST"
        },
        columns: [
            { data: "ActividadId" },
            {
                data: "ActividadFecha",
                render: function (data) {
                    var fecha = new Date(data.replace("-", "/").substring(0, 10));
                    var day = (fecha.getDate().toString().length == 1) ? '0' + fecha.getDate().toString() : fecha.getDate().toString();
                    var mes = fecha.getMonth() + 1;
                    var mes = (mes.toString().length == 1) ? '0' + mes.toString() : mes;
                    var data = day + '/' + mes + '/' + fecha.getFullYear();
                    return '<p>' + data + '</p>';
                }
            },
            {
                data: "ActividadEstado",
                render: function (data) {

                    estado = data;
                    if (data >= 1 && data <= 7) {
                        return estados.filter(x => x.Value == data)[0].Text;
                    } else {
                        return data;
                    }
                }
            },
            { data: "ActividadTitulo" },
            { data: "ClienteID" },
            { data: "ClienteRazonSocial" },
            { data: "UsuarioIdElabora" },
            { data: "UsuarioNombre" },
            {
                data: "ActividadId",
                render: function (data) {
                    var result = '<div class="fixed-action-btn-list horizontal">' +
                                 '       <a class="btn-floating btn-sm blue">' +
                                 '           <i class="material-icons">more_horiz</i>' +
                                 '       </a>' +
                                 '       <ul>'

                    if (estado == 1 || estado == 6) {
                        result += '           <li><a href="/Actividades/Edit?id=' + data + '" class="btn-floating green tooltipped" data-position="top" data-delay="50" data-tooltip="Editar"> <i class="material-icons" onclick="">mode_edit</i> </a></li>'
                        result += '           <li><a href="/Actividades/Delete?id=' + data + '" class="btn-floating red tooltipped" data-position="top" data-delay="50" data-tooltip="Eliminar"> <i class="material-icons" onclick="">delete</i> </a></li>'
                    }

                    result += '           <li><a href="/Actividades/Details?id=' + data + '" class="btn-floating yellow tooltipped" data-position="top" data-delay="50" data-tooltip="Ver Detalle"> <i class="material-icons" onclick="">description</i> </a></li>'

                    //Enviar a aprobación de Trade o principal
                    if (estado == 1 || estado == 6) {
                        result += '           <li><a href="javascript:void(0)" onclick="actividadPendiente(' + data + ');" class="btn-floating tooltipped" data-position="top" data-delay="50" data-tooltip="Enviar a Autorizar"> <i class="material-icons" onclick="">queue</i> </a></li>'
                    }

                 

                    if (estado == 4 || estado == 3) {
                        result += '           <li><a href="/Actividades/Report?archivo=PDF&filtro=' + data + '" class="btn-floating red tooltipped" data-position="top" data-delay="50" data-tooltip="Reporte PDF"> <i class="material-icons" onclick="">assignment</i> </a></li>'
                    }

                    result += '       </ul>' +
                                 '   </div>';

                    return result;
                }
            }

        ],
        sDom: '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
        oLanguage: {
            sLengthMenu: "Mostrando _MENU_ ",
            sZeroRecords: "No se encontraron registros",
            sInfo: "Mostrando _START_ - _END_ de _TOTAL_ registros",
            sInfoEmpty: "Mostrando 0 - 0 de 0 registros",
            sInfoFiltered: "(Filtrando de _MAX_ registros totales)",
            oPaginate: { sPrevious: "Anterior", sNext: "Siguiente", sFirst: "Primera", sLast: "Ultima" },
            sEmptyTable: "Não tem registros",
            sSearch: "Buscar:"
        },
        "drawCallback": function (settings) {
            $('.tooltipped').tooltip({ delay: 50 });
        }

    });




    $("select[name=tableActividades_length]").val('10'); //seleccionar valor por defecto del select
    $('select[name=tableActividades_length]').addClass("browser-default");
});


function actividadPendiente(ActividadId) {
    //alert(ActividadId);
    /*  Abierto = 1,
        Pendiente = 2,
        Autorizado = 3,
        Despachado = 4,
        Ejecutado = 5,
        Rechazado = 6,
        PendienteTrade = 7*/

    $.ajax({
        url: "../Actividades/CambiaEstadoActividad",
        type: "GET",
        data: { ActividadId: ActividadId, Estado: 7 },//Estado: 2
        dataType: 'text',
        success: function (result) {

            if (result) {
                Materialize.toast("Se mando la actividad " + ActividadId + " a autorizar correctamente", 2000, '', function () { window.location.href = '/Actividades' });
            } else {
                Materialize.toast("Error al cambiar de estado", 5000, '');
            }
        },
        beforeSend: function () {
            $("#Loading").openModal({
                dismissible: false
            });
        },
        complete: function () {
            $("#Loading").closeModal();
        },
        error: function (xhr, status) {
            Materialize.toast(status, 4000, '', function () { $("#Loading").closeModal(); });


        }
    });

}


