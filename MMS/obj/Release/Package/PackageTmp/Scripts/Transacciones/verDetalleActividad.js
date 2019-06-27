function _verOrdenDeActividad(filaActual, ActividadId) {

    $.ajax({
        cache: false,
        type: 'GET',
        url: "/PresupuestoVendedor/ObtenerOrdenDeActividad",
        data: { ActividadId: ActividadId },
        success: function (data) {
            var tr = $(filaActual).closest('tr');
            var row = $('#table').DataTable().row(tr);
            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                var resultStr = "";
                $.each(data, function (id, option) {
                    //Para calcular la fecha.
                    var milli = option.fechaCrea.replace(/\/Date\((-?\d+)\)\//, '$1');
                    var fecha = new Date(parseInt(milli));
                    var day = (fecha.getDate().toString().length == 1) ? '0' + fecha.getDate().toString() : fecha.getDate().toString();
                    var mes = fecha.getMonth() + 1;
                    var mes = (mes.toString().length == 1) ? '0' + mes.toString() : mes;
                    var FechaResult = day + '/' + mes + '/' + fecha.getFullYear();
                    resultStr += '<tr>' +
                                    '<td>' + option.ordenId + '</td>' +
                                    '<td>' + option.ordenEstado + '</td>' +
                                    '<td>' + FechaResult + '</td>' +
                                    '<td>' + option.guia + '</td>' +
                                '</tr>'
                });
                //Se construye la tabla con detalle
                row.child('<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px; width:100%"><tr><th>Nro Orden</th><th>Estado</th><th>Fecha de Creación</th><th>Guia</th></tr>' + resultStr + '<tr><th colspan="4">Productos</th></tr>' + '</table>').show();
                tr.addClass('shown');
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Error al cargar presupuesto de los meses!!');
        }
    });
};