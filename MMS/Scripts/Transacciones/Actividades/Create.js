var VueActividades;
var templateProducto;
var select2ProductosOptions, select2CCOptions;
$.MMS.Actividad = function (mod, actividad, items, files, options) {
    mod = mod.toLowerCase();
    //console.log(items);
    var datePickerOpts = {
        format: 'YYYY-MM-DD',
        weekStart: 1,
        time: false
    };

    var template = function (data) {
        return '<div class="select2-result-repository clearfix" style="width: 50%">' +
            `<div><b>${data.ClienteID} - ${data.ClienteRazonSocial}</b></div>` +
            "<div>" +
            `<small>Planta: ${data.PlantaID}</small><br>` +
            `<small>Zona: ${data.Zona}</small><br>` +
            `<small>Canal: ${data.Canal}</small><br>` +
            `<small>vendedor: ${data.Vendedor}</small>` +
            "</div>" +
            '</div>';
    };

    var select2ClientesOptions = {
        placeholder: "Clientes",
        theme: "bootstrap",
        ajax: {
            type: "GET",
            url: function (params) {
                return "/api/Actividades/BuscarCliente?q=" + encodeURIComponent(params.term)
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

   /* if (actividad.ClienteID != null) {

        $.get("/api/Actividades/GetCliente/" + actividad.ClienteID)
            .done(function (result) {
                if (result) {
                    var _clientes = [];
                    _clientes.push({
                        text: result.ClienteRazonSocial,
                        id: result.ClienteID,
                        data: result
                    });

                    select2ClientesOptions.data = _clientes;

                    $("#Actividad_ClienteID").select2(select2ClientesOptions);
                    GetpptoVendedor();
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });
    } else {
        $("#Actividad_ClienteID").select2(select2ClientesOptions);
    }*/ 

    if (actividad.Marcas != null) {
        $("#Actividad_MarcaID").val(actividad.Marcas.split(","));
        $("#Actividad_MarcaID").change();
    }



    //$("#Actividad_ClienteID").change(function () {

    //    if ($(this).val() != "") 
    //        GetpptoVendedor();


    //});

    $("#Actividad_ClienteID").on('select2:select', function (e) {

        var data = e.params.data;//Data del select2
        $("#Actividad_PlantaID").val(data.data.PlantaID);
        $("#Actividad_CanalID").val(data.data.CanalId);
        //$("#Actividad_CanalID").change();
        //GetpptoVendedor();
    });


    $("#Actividad_TipoActividadID").select2({
        placeholder: "Tipo de Ação",
        theme: "bootstrap",
    });

    //$("#Actividad_CanalID").select2({
    //    placeholder: "Buscar un canal",
    //    theme: "bootstrap",
    //});

    //$("#Actividad_CanalID").on('select2:select', function (e) {
    //    GetpptoVendedor();
    //});

    $("#Actividad_MarcaID").select2({
        placeholder: "Marca",
        theme: "bootstrap",
    });

    $("#Actividad_MarcaID").change(function () {
        var marcas = $("#Actividad_MarcaID").val();
        if (marcas != null)
            $("#Actividad_Marcas").val(marcas.join());
    });

    $('#Actividad_ActividadMetaE').inputmask("decimal", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 2,
        autoGroup: true,
        autoUnmask: true
    });

    $('#Actividad_ActividadMetaV').inputmask("decimal", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 2,
        autoGroup: true,
        autoUnmask: true
    });


    //Detalle
    templateProducto = function (data) {
        return '<div class="select2-result-repository clearfix" style="width: 50%">' +
            `<div><b>${data.ProductoId} - ${data.ProductoDesc}</b></div>` +
            "<div>" +
            `<small>Tipo Producto: ${data.TipoProducto}</small><br>` +
            "</div>" +
            '</div>';
    };

    select2ProductosOptions = {
        placeholder: "Buscar material POP",
        theme: "bootstrap",
        ajax: {
            type: "GET",
            url: function (params) {
                return "/api/Actividades/BuscarProducto?q=" + encodeURIComponent(params.term)
            },
            delay: 300,
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.ProductoDesc,
                            id: item.ProductoId,
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

            return templateProducto(repo.data);
        },
        templateSelection: function (repo) {
            if (!repo.id)
                return repo.text;

            return templateProducto(repo.data);
        },
        escapeMarkup: function (m) { return m; }
    };




    select2CCOptions = {
        placeholder: {
            id: "-1",
            text: "-- Centro de Custo --",
            selected: 'selected'
        },
        theme: "bootstrap",
        data: options.centroCostos,
    };

    VueActividades = new Vue({
        el: "#actividadContent",
        data: {
            ClienteId: "",
            UltimoItem: 0,
            Presupuesto: [{
                presupuesto: 0,
                gasto: 0,
                centroCostoId: "",
                centroCostoDesc: "",
            }],
            items: [{
                ActividadItemId: 0,
                ProductoId: "",
                ActividadItemDescripcion: "",
                ActividadItemCantidad: 1,
                ActividadItemProducto: "",
                ActividadItemPrecio: 0,
                CentroCostoID: "",
                Total: 0
            }]
        },
        mounted: function () {
            var vm = this;
            vm.items = [];
            if (items.length == 0) {
                vm.addItem(null);
            }
            else {
                for (var i = 0; i < items.length; i++) {
                    vm.addItem(items[i]);
                    //setPropertiesItem(items[i], i);
                }
            }
        },
        methods: {
            ModalPresupuesto: function () {
                $("#modalPresupuesto").modal("show");
            },
            addItem: function (item = null) {
                var vm = this;
                //var indexSetTimeOut = 0;
                if (item == null) {
                    vm.items.push({
                        ActividadItemId: 0,
                        ProductoId: "",
                        ActividadItemDescripcion: "",
                        ActividadItemCantidad: 1,
                        ActividadItemProducto: "",
                        ActividadItemPrecio: 0,
                        CentroCostoID: "",
                        Total: 0,
                        delete: false
                    });
                    setPropertiesItem(null, vm.items.length - 1);
                } else {
                    vm.items.push(item);
                    setPropertiesItem(item, vm.items.length - 1);
                }

                vm.UltimoItem++;

            },
            remove: function (index) {
                var vm = this;
                var item = vm.items[index];
                item.delete = true;
                vm.items[index] = item;
            },
            CalcularTotales: function (index) {
                var vm = this;
                var item = vm.items[index];
                item.Total = (item.ActividadItemCantidad * item.ActividadItemPrecio.toString().replace(",", ".")).toFixed(2);
                item.Total = (Number.isNaN(item.Total) ? 0 : item.Total);
                vm.items[index] = item;
            },
        },

    })

    VueActividades.Presupuesto = VueActividades.Presupuesto.splice();

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

    $('[data-toggle="tooltip"]').tooltip();

    if (mod == "create" || mod == "edit" || mod == "delete")
        checkValidationSummaryErrors();


    $('#Actividad_ActividadFechaDesde').val(moment(options.FechaDesde).format("YYYY-MM-DD"));
    $('#Actividad_ActividadFechaHasta').val(moment(options.FechaHasta).format("YYYY-MM-DD"));

    setVigencia();
    updateMaterialTextFields("#cardForm");

    if (mod == "create" || mod == "edit") {


        //$("#Actividad_CanalID").rules("add", { required: true });
        $("#Actividad_TipoActividadID").rules("add", { required: true });
        $("#Actividad_ClienteID").rules("add", { required: true });

        $("#Actividad_ActividadFechaDesde").rules("add", {
            min: moment(new Date()).add(1, "d").format("YYYY-MM-DD")
        });

        $("#Actividad_ActividadFechaHasta").rules("add", {
            min: moment($("#Actividad_ActividadFechaDesde").val()).add(1, "d").format("YYYY-MM-DD")
        });

        $('#Actividad_ActividadFechaDesde').bootstrapMaterialDatePicker(datePickerOpts).change(function () {
            $("#Actividad_ActividadFechaHasta").rules("remove", "min");
            $("#Actividad_ActividadFechaHasta").rules("add", {
                min: moment($("#Actividad_ActividadFechaDesde").val()).add(1, "d").format("YYYY-MM-DD")
            });
            setVigencia();
            $(this).valid();
            updateMaterialTextFields("#cardForm");

        });

        $('#Actividad_ActividadFechaHasta').bootstrapMaterialDatePicker(datePickerOpts).change(function () {
            setVigencia();
            $(this).valid();
            updateMaterialTextFields("#cardForm");
        });
        var validate = true;
        $("#formActividad").submit(function (e) {

            validate = true;

            validate = ValidaItems();
            if (!validate) {
                e.preventDefault();
                //msgAlert("error");
                return;
            } else {
                //msgAlert("fuck");
            }

            //return;

        })

    }
    else if (mod == "details" || mod == "delete") {
        $('#Actividad_CumplimientoTotal').inputmask("numeric", {
            radixPoint: ",",
            groupSeparator: ".",
            digits: 0,
            autoGroup: true,
            autoUnmask: true
        });

        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=datetime]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
        $("#cardForm textarea").prop("readonly", true);
    }



};

function setVigencia() {
    //var desde = new Date(moment($("#Actividad_ActividadFechaDesde").val()).format("YYYY-MM-DD")).getMonth() + 1;
    //var hasta = new Date(moment($("#Actividad_ActividadFechaHasta").val()).format("YYYY-MM-DD")).getMonth() + 1;

    var desde = moment($("#Actividad_ActividadFechaDesde").val()).locale('es').format("MMMM");
    var hasta = moment($("#Actividad_ActividadFechaHasta").val()).locale('es').format("MMMM");
    if (desde == hasta) {
        $("#vigencia").val(desde);
    } else {
        $("#vigencia").val(desde + ' - ' + hasta);
    }
    //updateMaterialTextFields("#cardForm");
}

/*

function GetpptoVendedor() {

    var plantaid = $("#Actividad_PlantaID").val();
    var canalid = $("#Actividad_CanalID").val();
    if (plantaid != "" && (canalid != "" && canalid != null)) {
        var fecha = moment($("#Actividad_ActividadFechaDesde").val()).add(1, "d").format("YYYY-MM-DD");
        var actividadId = $("#Actividad_ActividadId").val();
        var aaa = 0;
        TotalCentroCostos = [];
        VueActividades.Presupuesto = VueActividades.Presupuesto.splice();
        $.get(`/api/Actividades/GetPresupuesto?CanalId=${canalid}&PlantaId=${plantaid}&Fecha=${fecha}&ActividadId=${actividadId}`)
            .done(function (result) {
                VueActividades.Presupuesto = result;

                if (VueActividades.Presupuesto.length <= 0) {
                    msgAlert("Cliente sin presupuesto");
                }

            })
            .fail(handleError)
            .always(hLoading);
    }
}
*/


async function setPropertiesItem(data, indexItem) {
    return new Promise(resolve => {
        setTimeout(function () {

            //Set select2options
            //console.log("d:" + data);
            if (data != null) {
                if (data.ProductoId != null) {
                    $.get("/api/Actividades/GetProducto/" + data.ProductoId)
                        .done(function (result) {
                            if (result) {
                                var _productos = [];
                                _productos.push({
                                    text: result.ProductoDesc,
                                    id: result.ProductoId,
                                    data: result
                                });

                                select2ProductosOptions.data = _productos;

                                $(`#items_${indexItem}_ProductoId`).select2(select2ProductosOptions);
                            }
                        })
                        .fail(function (jqXHR, textStatus, errorThrown) {
                            msgError(errorThrown);
                        });
                } else {
                    select2ProductosOptions.data = [];
                    $(`#items_${indexItem}_ProductoId`).select2(select2ProductosOptions);
                }
            } else {
                select2ProductosOptions.data = [];
                $(`#items_${indexItem}_ProductoId`).select2(select2ProductosOptions);
            }


            $(`#items_${indexItem}_CentroCostoID`).select2(select2CCOptions);
            //Set Event select
            $(`#items_${indexItem}_ProductoId`).on('select2:select', function (e) {
                var index = $(this).data("index");//index de item en la tabla
                var data = e.params.data;//Data del select2

                var item = VueActividades.items[index];
                item.ActividadItemPrecio = data.data.ProductoPrecio.toString().replace(".", ","); //data.data.ProductoPrecio.toFixed(2)
                item.ProductoId = data.data.ProductoId;
                item.ActividadItemProducto = data.data.ProductoId;
                item.ActividadItemDescripcion = data.data.ProductoDesc;
                VueActividades.items[index] = item;
                VueActividades.CalcularTotales(index);
            });

            $(`#items_${indexItem}_CentroCostoID`).on('select2:select', function (e) {
                var index = $(this).data("index");//index de item en la tabla
                var data = e.params.data;

                var item = VueActividades.items[index];
                //console.log(item);
                item.CentroCostoID = data.data.CentroCostoID;
                VueActividades.items[index] = item;
            });
            if (data != null) {
                if (data.CentroCostoID != null) {
                    $(`#items_${indexItem}_CentroCostoID`).val(data.CentroCostoID);
                    $(`#items_${indexItem}_CentroCostoID`).change();
                }
            }
            //console.log(indexItem);
            resolve('resolved');

        }.bind(this), 100);
    });
}

function ValidaItems() {
    var valid = true;
    var idx = 1;

    var Gastos = [];

    for (var i = 0; i < VueActividades.items.length; i++) {
        var item = VueActividades.items[i];
        if (!item.delete) {
            var ProductoId = $(`#items_${i}_ProductoId`).val();
            var CentroCostoID = $(`#items_${i}_CentroCostoID`).val();
            var Cantidad = $(`#items_${i}_ActividadItemCantidad`).val();
            var Precio = $(`#items_${i}_ActividadItemPrecio`).val();

            if (ProductoId == "" || ProductoId == null) {
                valid = false;
                msgError("Error campo producto requerido", "Item: " + (idx));
            }
            if (CentroCostoID == "" || CentroCostoID == null || CentroCostoID == "-1") {
                valid = false;
                msgError("Error campo centro de costo requerido", "Item: " + (idx));
            }
            if (Cantidad == "" || Cantidad == 0) {
                valid = false;
                msgError("Error campo cantidad requerido", "Item: " + (idx));
            }
            if (Precio == "" || Precio == 0) {
                valid = false;
                msgError("Error campo precio requerido", "Item: " + (idx));
            }
            idx++;

            if (!valid) return valid;

            var Gasto = Gastos.filter(c => {
                return (c.CentroCostoID.indexOf(CentroCostoID) > -1)
            });

            if (Gasto.length <= 0)
                Gastos.push({ CentroCostoID: CentroCostoID, gasto: parseFloat((Cantidad * Precio.replace(",", ".")).toFixed(2)) });
            else
                Gasto[0].gasto += parseFloat((Cantidad * Precio.replace(",", ".")).toFixed(2));
        }//if (!item.delete) {
    }//for items

   /* for (var idx = 0; idx < Gastos.length; idx++) {
        var presupuesto = VueActividades.Presupuesto.filter(c => {
            return (c.centroCostoId.indexOf(Gastos[idx].CentroCostoID) > -1)
        });

        if (presupuesto.length <= 0) {
            msgError("Error, sin presupuesto", "Centro de costo: " + Gastos[idx].CentroCostoID);
            valid = false;
        }
        else {
            if (presupuesto[0].presupuesto < (presupuesto[0].gasto + Gastos[idx].gasto)) {
                msgError("gasto superior al presupuesto (Presupuesto: " + presupuesto[0].presupuesto.formatMoney(0) + " Gasto: " + parseFloat(presupuesto[0].gasto + Gastos[idx].gasto).formatMoney(0) + ")", "Centro de costo: " + Gastos[idx].CentroCostoID);
                valid = false;
            }
        }//if (presupuesto.length <= 0) {
    }//for gastos
    */
    //console.log(Gastos);

    return valid;
}

function AttachFile() {
    $("#frmFiles .modal")
        .on("hidden.bs.modal", function () {
            
            
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
        msgError("El tamaño del archivo excede el limite");
        result = false;
    }


    return result;
}
