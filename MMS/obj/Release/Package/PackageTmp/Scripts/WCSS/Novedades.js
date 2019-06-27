var vueItemsTabl;
var DataSelectItem;
var DataSelectMotivo;
var DataSelectCausa;
var idx;
var templateItems;
var select2ItemsOptions;
var templateMotivo;
var select2MotivoOptions;
$.MMS.Novedades = function (mod, items, novedad, dataItems, dataMotivo, files, dataCausa) {
    mod = mod.toLowerCase();

    Vue.config.devtools = true;


    if (mod != "create") {
        var vueItemsDeleteTable = new Vue({
            el: "#itemsDelete",
            data: {
                itemsDelete: []
            }
        });
    }

    var vueItemsTable = new Vue({
        el: "#tableItems",
        data: {
            items: items
        },
        methods: {
            remove: function (index) {
                if (this.items[index].Id != 0) {
                    vueItemsDeleteTable.itemsDelete.push({ Id: this.items[index].Id });
                }
                this.items.splice(index, 1);
            },
            edit: function (index) {
                // this.items.splice(index, 1);
                idx = index;
                var item = this.items[index];
                ItemsModal("edit", item);
            },
            details: function (index) {
                // this.items.splice(index, 1);
                idx = index;
                var item = this.items[index];
                ItemsModal("details", item);
            }
        }
    });


    var vueFilesTable = new Vue({
        el: "#ListFiles",
        data: {
            files: files
        },
        methods: {
            addNewFile: function (event) {
                this.files.push({ order: this.files.length + 1, FileName: "", New: true })
            },
            remove: function (index) {
                this.files.splice(index, 1);

            },
            SizeFile: function (index, event) {
                if ($(event.target)[0].files.length > 0) {
                    var size = ($(event.target)[0].files[0].size / 1000000).toFixed(2) + " mb";
                    $("#Size" + index).html(size);
                    var bsize = MaxSizeFile();
                    if (!bsize) {
                        $(event.target).val("");
                        $("#Size" + index).html("");
                        MaxSizeFile();
                    }
                }
            }
        }
    });


    $("#Novedad_TipoPersona").select2({ minimumResultsForSearch: -1, theme: "bootstrap" });
    $("#Novedad_Prioridad").select2({ minimumResultsForSearch: -1, theme: "bootstrap" });

    $("#Novedad_TipoPersona").find("option").eq(0).remove();//Remueve primer option vacio
    $("#Novedad_Prioridad").find("option").eq(0).remove();//Remueve primer option vacio

    $("#Novedad_TipoPersona").change(function () {
        if ($(this).val() == "1") {//Empleado ATG
            $("#Ppersona").hide();
            $("#Pempleado").show();
            $("#Novedad_Persona").val($("#Persona_Empleado").val());
        } else if ($(this).val() == "2") { //Cliente
            $("#Ppersona").hide();
            $("#Pempleado").hide();
            $("#Novedad_Persona").val($("#Novedad_ClienteId").val());

        } else {
            $("#Ppersona").show();
            $("#Pempleado").hide();
            $("#Novedad_Persona").val("");
        }
    })

    $("#Persona_Empleado").change(function () {
        if ($("#Novedad_TipoPersona").val() == "1")
            $("#Novedad_Persona").val($("#Persona_Empleado").val());
    })

    $("#Novedad_ClienteId").change(function () {
        if ($("#Novedad_TipoPersona").val() == "2")
            $("#Novedad_Persona").val($("#Novedad_ClienteId").val());
    })

    var templateUsuario = function (data) {
        return '<div class="select2-result-repository clearfix" style="width: 50%">' +
            `<div><b>${data.UsuarioId} - ${data.UsuarioNombre}</b></div>` +
            '</div>';
    };

    var select2UsuarioOptions = {
        placeholder: "Search Employee",
        theme: "bootstrap",
        ajax: {
            type: "GET",
            url: function (params) {
                return "/api/Usuarios/BuscarUsuario?q=" + encodeURIComponent(params.term)
            },
            delay: 300,
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.UsuarioNombre,
                            id: item.UsuarioId,
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

            return templateUsuario(repo.data);
        },
        templateSelection: function (repo) {
            if (!repo.id)
                return repo.text;

            return templateUsuario(repo.data);
        },
        escapeMarkup: function (m) { return m; }
    };


    if (novedad != null) {

        if (novedad.TipoPersona == "1") {
            $.get("/api/Usuarios/GetUsuario/" + novedad.Persona)
            .done(function (result) {
                if (result) {
                    var _usuario = [];
                    _usuario.push({
                        text: result.UsuarioNombre,
                        id: result.UsuarioId,
                        data: result
                    });

                    select2UsuarioOptions.data = _usuario;


                    $("#Persona_Empleado").select2(select2UsuarioOptions);
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });
        } else {
            $("#Persona_Empleado").select2(select2UsuarioOptions);
        }

    } else {
        $("#Persona_Empleado").select2(select2UsuarioOptions);

    }


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


    if (novedad != null) {

        $.get("/api/Devoluciones/GetCliente/" + novedad.ClienteId)
            .done(function (result) {
                if (result) {
                    var _clientes = [];
                    _clientes.push({
                        text: result.ClienteRazonSocial,
                        id: result.ClienteID,
                        data: result
                    });

                    select2Options.data = _clientes;
                    //$("#Devolucion_ClienteId").val(result.ClienteID).change();

                    $("#Novedad_ClienteId").select2(select2Options);
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });

        console.log(novedad.TipoPersona);
        if (novedad.TipoPersona == "1") {//Empleado ATG
            $("#Ppersona").hide();
            $("#Pempleado").show();

        } else if (novedad.TipoPersona == "2") { //Cliente
            $("#Ppersona").hide();
            $("#Pempleado").hide();


        } else {
            $("#Ppersona").show();
            $("#Pempleado").hide();

        }


    } else {
        

        $("#Novedad_ClienteId").select2(select2Options);

    }



    //SELECT2 PARA ITEMS

    templateItems = function (data) {

        return '<div class="select2-result-repository clearfix" style="width: 100%">' +
            `<div><b>${data.Codigo} - ${data.Descripcion}</b></div>` +
            "<div>" +
            `<small>Packing unit: ${data.UnidadEmpaque}</small><br>` +
            "</div>" +
            '</div>';
    };

    select2ItemsOptions = {
        placeholder: "Search Item",
        theme: "bootstrap",
        data: dataItems,
        ajax: {
            type: "GET",
            url: function (params) {
                return "/api/Plantillas/BuscarItem?q=" + encodeURIComponent(params.term)
            },
            delay: 300,
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.Descripcion,
                            id: item.Id,
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


    $("#_selectItemId").select2(select2ItemsOptions);





    $("#_selectItemId").on('select2:select', function (e) {
        if ($(this).select2('data').length == 0)
            return;

        DataSelectItem = $(this).select2('data')[0].data;

    });

    //SELECT2 PARA ITEMS

    //SELECT2 PARA Motivos
    templateMotivo = function (data) {

        return '<div class="select2-result-repository clearfix" style="width: 100%">' +
            `<div><b>${data.Id} - ${data.Nombre}</b></div>` +
            '</div>';
    };


    select2MotivoOptions = {
        placeholder: "Search Reason",
        theme: "bootstrap",
        data: dataMotivo,
        ajax: {
            type: "GET",
            url: function (params) {
                return "/api/MotivosPQRS/BuscarMotivoPQRS?q=" + encodeURIComponent(params.term) + "&t=3"
            },
            delay: 300,
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.Nombre,
                            id: item.Id,
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

            return templateMotivo(repo.data);
        },
        templateSelection: function (repo) {
            if (!repo.id)
                return repo.text;

            return templateMotivo(repo.data);
        },
        escapeMarkup: function (m) { return m; }
    };

    //$("#_selectMotivoPQRSId").select2(select2MotivoOptions);
    $("#_selectMotivoPQRSId").select2({
        data: dataMotivo,
        placeholder: "Search Reason",
        theme: "bootstrap",
    });

    $("#_selectMotivoPQRSId").on('select2:select', function (e) {
        if ($(this).select2('data').length == 0)
            return;

        DataSelectMotivo = $(this).select2('data')[0].data;

    });

    //SELECT2 PARA Motivos

    $("#_selectCausaPQRSId").select2({
        data: dataCausa,
        placeholder: "Search Cause",
        theme: "bootstrap",
    });

    $("#_selectCausaPQRSId").on('select2:select', function (e) {
        if ($(this).select2('data').length == 0)
            return;

        DataSelectCausa = $(this).select2('data')[0].data;

    });

    $("#_selectEstado").select2({
        placeholder: "Search Status",
        allowClear: true,
        theme: "bootstrap"
    })


    $('#_Precio').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    $('#_PrecioAsumido').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    $("#btnSave").click(function (e) {
        if (ValidaFormItems()) {
            if (idx == null) {
                var item = {
                    Id: 0,
                    ItemId: DataSelectItem.Id,
                    ItemDesc: DataSelectItem.Codigo + ' - ' + DataSelectItem.Descripcion,
                    Cantidad: $("#_Cantidad").val(),
                    Precio: $("#_Precio").val(),
                    NroFactura: $("#_NroFactura").val(),
                    NroGuia: $("#_NroGuia").val(),
                    MotivoPQRSId: DataSelectMotivo,
                    CausaPQRSId: DataSelectCausa,
                    ComentarioAdicional: $("#_ComentarioAdicional").val(),
                    Estado: $("#_selectEstado").val(),
                    CantidadRecibida: $("#_CantidadRecibida").val(),
                    CantidadSubida: $("#_CantidadSubida").val(),
                    ComentarioEstadoMercancia: $("#_ComentarioEstadoMercancia").val(),
                    DocSoporte: $("#_DocSoporte").val(),
                    PrecioAsumido: $("#_PrecioAsumido").val(),
                    EditButton: "",
                };

                vueItemsTable.items.push(item);
            } else {
                var item = vueItemsTable.items[idx];

                item.ItemId = DataSelectItem.Id;
                item.ItemDesc = DataSelectItem.Codigo + ' - ' + DataSelectItem.Descripcion;
                item.Cantidad = $("#_Cantidad").val();
                item.Precio = $("#_Precio").val();
                item.NroFactura = $("#_NroFactura").val();
                item.NroGuia = $("#_NroGuia").val();
                item.MotivoPQRSId = DataSelectMotivo;
                item.CausaPQRSId = DataSelectCausa;
                item.ComentarioAdicional = $("#_ComentarioAdicional").val();
                item.Estado = $("#_selectEstado").val();
                item.CantidadRecibida = $("#_CantidadRecibida").val();
                item.CantidadSubida = $("#_CantidadSubida").val();
                item.ComentarioEstadoMercancia = $("#_ComentarioEstadoMercancia").val();
                item.DocSoporte = $("#_DocSoporte").val();
                item.PrecioAsumido = $("#_PrecioAsumido").val(),
                vueItemsTable.items[idx] = item;
            }

            idx = null;
            DataSelectItem = null;
            DataSelectMotivo = null;
            DataSelectCausa = null;
            $("#frmItems .modal").modal("hide");
        }
    })


    if (mod == "create" || mod == "edit") {
        var estado = $("#Novedad_Estado").val();

        if (estado == "" || estado == "Open") {

            $(".AdditionalInfo").css("display", "none");

            $("#_selectEstado").prop("disabled", true);
            $("#_DocSoporte").prop("readonly", true);
            $("#_CantidadRecibida").prop("readonly", true);
            $("#_CantidadSubida").prop("readonly", true);
            $("#_ComentarioEstadoMercancia").prop("readonly", true);
            $("#_PrecioAsumido").prop("readonly", true);
        } else {
            $("#btnAdd").hide();
            $("#btnSave").hide();
            //$(".btnEdit").hide();
            $(".btnDelete").hide();
            $(".btnAdd").hide();

            $("#cardForm input[type=text]").prop("readonly", true);
            $("#cardForm input[type=number]").prop("readonly", true);
            $("#cardForm input[type=checkbox]").prop("disabled", true);
            $("#cardForm select").prop("disabled", true);
            $("#cardForm textarea").prop("readonly", true);

            if (estado == "In_Process") {

                $("form").submit(function (e) {
                    $("#cardForm select").prop("disabled", false);
                })

                $("#_selectEstado").prop("disabled", false);
                $("#_DocSoporte").prop("readonly", false);
                $("#_CantidadRecibida").prop("readonly", false);
                $("#_CantidadSubida").prop("readonly", false);
                $("#_ComentarioEstadoMercancia").prop("readonly", false);
                $("#_PrecioAsumido").prop("readonly", false);
            }
        }
    }
    else if (mod == "details" || mod == "delete") {

        $("#btnAdd").hide();
        $("#btnSave").hide();
        $(".btnEdit").hide();
        $(".btnDelete").hide();
        $(".btnAdd").hide();

        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=number]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
        $("#cardForm textarea").prop("readonly", true);

    }




}



function ItemsModal(mod, item) {
    mod = mod.toLowerCase();
    cleanModal(mod);
    setDataModal(item);
    $("#btnSave").show();
    if (mod == "create") {
        $("#_titlemodal").html("Add Item");
    }
    else if (mod == "edit") {
        $("#_titlemodal").html("Modify Item");
    }
    else if (mod == "delete") {
        $("#btnSave").hide();
        $("#_titlemodal").html("Delete Item");
    }
    else if (mod == "details") {
        $("#btnSave").hide();
        $("#_titlemodal").html("View Item");
    }
    else {
        $("#_titlemodal").html("Item");
    }

    $("#frmItems .modal")
        .on("hidden.bs.modal", function () {
            idx = null;
            //item = null;
        })
        .modal();
}

function cleanModal(mod) {
    $("#_selectItemId").val("").change();
    $("#_Cantidad").val("");
    $("label[for=_Cantidad]").addClass("active");

    $("#_Precio").val("");
    $("label[for=_Precio]").addClass("active");

    $("#_NroFactura").val("");
    $("label[for=_NroFactura]").addClass("active");

    $("#_NroGuia").val("");
    $("label[for=_NroGuia]").addClass("active");

    $("#_selectMotivoPQRSId").val("").change();
    $("#_selectCausaPQRSId").val("").change();
    $("#_ComentarioAdicional").val("");
    $("label[for=_ComentarioAdicional]").addClass("active");

    $("#_selectEstado").val("").change();;
    $("#_CantidadRecibida").val("");
    $("label[for=_CantidadRecibida]").addClass("active");

    $("#_CantidadSubida").val("");
    $("label[for=_CantidadSubida]").addClass("active");

    $("#_ComentarioEstadoMercancia").val("");
    $("label[for=_ComentarioEstadoMercancia]").addClass("active");

    $("#_PrecioAsumido").val("");
    $("label[for=_PrecioAsumido]").addClass("active");

    $("#_DocSoporte").val("");
    $("label[for=_DocSoporte]").addClass("active");

    if (mod == "details" || mod == "delete") {
        $("#_selectItemId").prop("disabled", true);
        $("#_Cantidad").prop("readonly", true);
        $("#_Precio").prop("readonly", true);
        $("#_NroFactura").prop("readonly", true);
        $("#_NroGuia").prop("readonly", true);
        $("#_selectMotivoPQRSId").prop("disabled", true);
        $("#_selectCausaPQRSId").prop("disabled", true);
        $("#_ComentarioAdicional").prop("readonly", true);
        $("#_selectEstado").prop("disabled", true);
        $("#_CantidadRecibida").prop("readonly", true);
        $("#_CantidadSubida").prop("readonly", true);
        $("#_DocSoporte").prop("readonly", true);
        $("#_ComentarioEstadoMercancia").prop("readonly", true);
        $("#_PrecioAsumido").prop("readonly", true);
    } else {



        var estado = $("#Novedad_Estado").val();
        if (estado == "Open") {
            $("#_selectItemId").prop("disabled", false);
            $("#_Cantidad").prop("readonly", false);
            $("#_Precio").prop("readonly", false);
            $("#_NroFactura").prop("readonly", false);
            $("#_NroGuia").prop("readonly", false);
            $("#_selectMotivoPQRSId").prop("disabled", false);
            $("#_selectCausaPQRSId").prop("disabled", false);
            $("#_ComentarioAdicional").prop("readonly", false);
        } else {
            $("#_selectItemId").prop("disabled", true);
            $("#_Cantidad").prop("readonly", true);
            $("#_Precio").prop("readonly", true);
            $("#_NroFactura").prop("readonly", true);
            $("#_NroGuia").prop("readonly", true);
            $("#_selectMotivoPQRSId").prop("disabled", true);
            $("#_selectCausaPQRSId").prop("disabled", true);
            $("#_ComentarioAdicional").prop("readonly", true);
        }

        if (estado != "" && estado != "Open") {
            $("#_selectEstado").prop("disabled", false);
            $("#_CantidadRecibida").prop("readonly", false);
            $("#_CantidadSubida").prop("readonly", false);
            $("#_DocSoporte").prop("readonly", false);
            $("#_ComentarioEstadoMercancia").prop("readonly", false);
            $("#_PrecioAsumido").prop("readonly", false);
        }
    }


}

function setDataModal(item) {
    if (idx != null) {
        //item = vueItemsTable.items[idx];

        $.get("/api/Plantillas/GetItem/" + item.ItemId)
            .done(function (result) {
                if (result) {
                    var _items = [];
                    _items.push({
                        text: result.Descripcion,
                        id: result.Id,
                        data: result
                    });



                    select2ItemsOptions.data = _items;
                    $("#_selectItemId").val(result.Id).change();
                    DataSelectItem = result;
                    //$("#_selectItemId").select2(select2ItemsOptions);
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });




        $("#_Cantidad").val(item.Cantidad);
        $("#_Precio").val(item.Precio);
        $("#_NroFactura").val(item.NroFactura);
        $("#_NroGuia").val(item.NroGuia);


        $.get("/api/MotivosPQRS/GetMotivoPQRS?id=" + item.MotivoPQRSId.Id + "&t=2")
            .done(function (result) {
                if (result) {
                    var MotivosPQRS = [];
                    MotivosPQRS.push({
                        text: result.Nombre,
                        id: result.Id,
                        data: result
                    });

                   // select2MotivoOptions.data = MotivosPQRS;
                    $("#_selectMotivoPQRSId").val(result.Id).change();
                    DataSelectMotivo = result;

                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });



        $.get("/api/CausaPQRS/GetCausaPQRS?id=" + item.CausaPQRSId.Id + "&t=2")
       .done(function (result) {
           if (result) {
               var causa = [];
               causa.push({
                   text: result.Nombre,
                   id: result.Id,
                   data: result
               });

               //select2CausaPQRSOptions.data = causa;
               $("#_selectCausaPQRSId").val(result.Id).change();
               DataSelectCausa = result;
               //$("#_selectTipoDevolucionId").select2(select2TipoDevolucionOptions);
           }
       })
       .fail(function (jqXHR, textStatus, errorThrown) {
           msgError(errorThrown);
       });

        $("#_ComentarioAdicional").val(item.ComentarioAdicional);
        $("#_selectEstado").val(item.Estado).change();;
        $("#_CantidadRecibida").val(item.CantidadRecibida);
        $("#_CantidadSubida").val(item.CantidadSubida);
        $("#_ComentarioEstadoMercancia").val(item.ComentarioEstadoMercancia);
        $("#_PrecioAsumido").val(item.PrecioAsumido);
        $("#_DocSoporte").val(item.DocSoporte);
    }
}

function ValidaFormItems() {
    var result = true;

    if (DataSelectItem == null) {
        msgError("Campo Item requerido");
        result = false;
    } else if (DataSelectItem.Id == "") {
        msgError("Campo Item requerido");
        result = false;
    } else {
        var modUnidad = $("#_Cantidad").val() % DataSelectItem.UnidadEmpaque;
        if (modUnidad != 0) {
            msgError("Cantidad por fuera de unidad de empaque");
            result = false;
        }
    }



    if (DataSelectMotivo == null) {
        msgError("Campo Motivo requerido");
        result = false;
    } else if (DataSelectMotivo.Id == "") {
        msgError("Campo Motivo requerido");
        result = false;
    }

    if (DataSelectCausa == null) {
        msgError("Campo Causa requerido");
        result = false;
    } else if (DataSelectCausa.Id == "") {
        msgError("Campo Causa requerido");
        result = false;
    }

    //if ($("#_selectEstado").val() == "" || $("#_selectEstado").val() == null) {
    //    msgError("Campo Motivo requerido");
    //    result = false;
    //}

    if ($("#_Cantidad").val() == "" || $("#_Cantidad").val() == null) {
        msgError("Campo Cantidad requerido");
        result = false;
    }

    if ($("#_Precio").val() == "" || $("#_Precio").val() == null) {
        msgError("Campo Precio requerido");
        result = false;
    }

    //if ($("#_NroFactura").val() == "" || $("#_NroFactura").val() == null) {
    //    msgError("Campo Factura requerido");
    //    result = false;
    //}



    return result;
}

function AttachFile() {
    $("#frmFiles .modal")
        .on("hidden.bs.modal", function () {
            idx = null;
            //item = null;
        })
        .modal();
}



function MaxSizeFile() {

    var result = true;

    var fsize = 0;
    $('input[type=file]').each(function () {
        if ($(this)[0].files.length > 0) {
            fsize += $(this)[0].files[0].size;
        }
    });
    $("#tSize").html((fsize / 1000000).toFixed(2));

    fsize = fsize / 1000000;
    if (fsize > 50) {
        msgError("The attachment size exceeds the allowable limit");
        result = false;
    }


    return result;
}
