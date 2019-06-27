
var vueCommentFileTable;
var vueDocTable;
$.MMS.PQRS = function (mod, formato, items, filesFormato) {
    mod = mod.toLowerCase();

    //Select2
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

    if (formato != null) {

        $.get("/api/Devoluciones/GetCliente/" + formato.ClienteId)
            .done(function (result) {
                if (result) {
                    var _clientes = [];
                    _clientes.push({
                        text: result.ClienteRazonSocial,
                        id: result.ClienteID,
                        data: result
                    });

                    select2Options.data = _clientes;

                    $("#formato_ClienteId").select2(select2Options);
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });
    } else {
        $("#formato_ClienteId").select2(select2Options);
    }


    $("#TipoPQRS").select2();


    $("#_selectEstado").select2({
        placeholder: "Search Status",
        allowClear: true,
        theme: "bootstrap"
    })
    //Select2

    var vueItemsTable = new Vue({
        el: "#tableItems",
        data: {
            items: items
        },
        methods: {
            details: function (index) {
                idx = index;
                var item = this.items[index];
                ItemsModal("details", item);
            }
        }
    });


    var vueFilesformatoTable = new Vue({
        el: "#ListFiles",
        data: {
            filesFormato: filesFormato
        },
        methods: {
        }
    });


    vueCommentFileTable = new Vue({
        el: "#ListCommentFiles",
        data: {
            CommentFiles: []
        },
        methods: {
        }
    });


    vueDocTable = new Vue({
        el: "#tableDocs",
        data: {
            docs: []
        }
    });

    if (mod == "details") {

        //$("#btnAdd").hide();
        //$("#btnSave").hide();
        //$(".btnEdit").hide();
        //$(".btnDelete").hide();
        //$(".btnAdd").hide();

        $(".cardInfo input[type=text]").prop("readonly", true);
        $(".cardInfo input[type=number]").prop("readonly", true);
        $(".cardInfo input[type=checkbox]").prop("disabled", true);
        $(".cardInfo select").prop("disabled", true);
        $(".cardInfo textarea").prop("readonly", true);

    }
}

function AttachFileFormato() {
    $("#frmFiles .modal")
        .on("hidden.bs.modal", function () {
            idx = null;
            //item = null;
        })
        .modal();
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
    $("#_ComentarioAdicional").val("");
    $("label[for=_ComentarioAdicional]").addClass("active");

    $("#_selectEstado").val("").change();;
    $("#_CantidadRecibida").val("");
    $("label[for=_CantidadRecibida]").addClass("active");

    $("#_CantidadSubida").val("");
    $("label[for=_CantidadSubida]").addClass("active");

    $("#_ComentarioEstadoMercancia").val("");
    $("label[for=_ComentarioEstadoMercancia]").addClass("active");

    $("#_DocSoporte").val("");
    $("label[for=_DocSoporte]").addClass("active");

    if (mod == "details" || mod == "delete") {
        $("#_selectItemId").prop("disabled", true);
        $("#_Cantidad").prop("readonly", true);
        $("#_Precio").prop("readonly", true);
        $("#_NroFactura").prop("readonly", true);
        $("#_NroGuia").prop("readonly", true);
        $("#_selectMotivoPQRSId").prop("disabled", true);
        $("#_ComentarioAdicional").prop("readonly", true);
        $("#_selectEstado").prop("disabled", true);
        $("#_CantidadRecibida").prop("readonly", true);
        $("#_CantidadSubida").prop("readonly", true);
        $("#_DocSoporte").prop("readonly", true);
        $("#_ComentarioEstadoMercancia").prop("readonly", true);
    } else {
        $("#_selectItemId").prop("disabled", false);
        $("#_Cantidad").prop("readonly", false);
        $("#_Precio").prop("readonly", false);
        $("#_NroFactura").prop("readonly", false);
        $("#_NroGuia").prop("readonly", false);
        $("#_selectMotivoPQRSId").prop("disabled", false);
        $("#_ComentarioAdicional").prop("readonly", false);


        var estado = $("#Devolucion_Estado").val();
        if (estado != "" && estado != "Open") {
            $("#_selectEstado").prop("disabled", false);
            $("#_CantidadRecibida").prop("readonly", false);
            $("#_CantidadSubida").prop("readonly", false);
            $("#_DocSoporte").prop("readonly", false);
            $("#_ComentarioEstadoMercancia").prop("readonly", false);
        }
    }


}

function setDataModal(item) {
    if (idx != null) {



        $.get("/api/Plantillas/GetItem/" + item.ItemId)
            .done(function (result) {
                if (result) {

                    var option = document.createElement("option");
                    option.text = result.Codigo + " - " + result.Descripcion;
                    option.value = result.Id;
                    option.selected = true;

                    var SItemId = document.getElementById("_selectItemId");
                    SItemId.add(option);



                    $("#_selectItemId").select2();
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });




        $("#_Cantidad").val(item.Cantidad);
        $("#_Precio").val(item.Precio);
        $("#_NroFactura").val(item.NroFactura);
        $("#_NroGuia").val(item.NroGuia);


        $.get("/api/MotivosPQRS/GetMotivoPQRS?id=" + item.MotivoPQRSId.Id + "&t=1")
            .done(function (result) {
                if (result) {

                    var option = document.createElement("option");
                    option.text = result.Nombre;
                    option.value = result.Id;
                    option.selected = true;

                    var SMotivoPQRSId = document.getElementById("_selectMotivoPQRSId");
                    SMotivoPQRSId.add(option);

                    $("#_selectMotivoPQRSId").select2();




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
        $("#_DocSoporte").val(item.DocSoporte);
    }
}

function AttachCommentFile(StrArchivos) {

    var Archivos = JSON.parse(StrArchivos);
    $("#frmCommentFiles .modal")
        .on("hidden.bs.modal", function () {
            idx = null;
            //item = null;
        })
        .modal();
    vueCommentFileTable.CommentFiles = Archivos;
}


function DocumentsModal(StrDocuments) {

    var documents = JSON.parse(StrDocuments);
    $("#frmCommentDocuments .modal")
        .on("hidden.bs.modal", function () {
            idx = null;
            //item = null;
        })
        .modal();
    vueDocTable.docs = documents;

    vueDocTable
}




