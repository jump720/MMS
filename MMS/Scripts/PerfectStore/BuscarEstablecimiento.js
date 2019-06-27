$.MMS.BuscarEstablecimiento = function (controllerName, controllerNameWeb) {


    dt = dataTablesIndex("tableDetalle", "/api/" + controllerNameWeb + "/BuscarEstablecimiento", "/" + controllerName + "/Index",
        [
            { data: "Fecha" },
            {
                data: "NombreEstablecimiento",
                render: function (data, type, row) {
                    if (row.NombreEstablecimiento != null)
                        if (row.NombreEstablecimiento.length > 20) {
                            return row.NombreEstablecimiento.substring(0, 20);
                        }
                    return row.NombreEstablecimiento;
                }
            },
             { data: "Ubicacion" },
            {
                data: "Direccion",
                render: function (data, type, row) {
                    if (row.Direccion != null)
                        if (row.Direccion.length > 20) {
                            return row.Direccion.substring(0, 20);
                        }

                    return row.Direccion;
                }
            },

             {
                 data: "UsuarioId",
                 render: function (data, type, row) {
                     if (row.UsuarioId != null)
                         if (row.UsuarioId.length > 20) {
                             return row.UsuarioId.substring(0, 20);
                         }
                     return row.UsuarioId;
                 }
             },
            {
                data: "Id",
                render: function (value) {
                    return (
                        `<a href='/${controllerName}/Create/${value}' onclick="redirtWithReturnSearch(this, event)" class='btn-table btn-info btn-sm btn-sm-circle waves-circle waves-effect waves-float @ViewData[$"has_{controllerName}Create"]' data-toggle="tooltip" data-placement="top" title="Create Copy"><i class='material-icons'>send</i></a>`);
                }
            }
        ],
                {
                    mainFilter: false,
                    modalTable: true,
                    filterControls: ["txtModalNombreEstablecimiento", "txtModalUsuarioId", "txtModalDireccion", "txtModalUbicacion"],
                    fnServerParams: function () {
                        return [
                            //{ name: "_fecha", value: $('#txtFecha').val() },
                            { name: "_nombreestablecimiento", value: $('#txtModalNombreEstablecimiento').val() },
                            { name: "_usuarioid", value: $('#txtModalUsuarioId').val() },
                            { name: "_direccion", value: $('#txtModalDireccion').val() },
                            { name: "_ubicacion", value: $('#txtModalUbicacion').val() },
                        ];
                    }
                });
}