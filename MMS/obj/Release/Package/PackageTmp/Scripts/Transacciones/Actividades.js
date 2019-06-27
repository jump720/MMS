var dataAuto;
var form;
var FlagCliente = true;
var TotalActividad = 0;
var PresupuestoMes = 0;
var GastoMes = 0;
var CentroCostos = [];
var TotalCentroCostos = [];
var TotalActividades = [];
var actividadId = 0;
var meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
var vigencia = "";

$(document).ready(function () {

    if (TrnMode == 1 || TrnMode == 2) {
        var fecha = new Date($("#ActividadFecha").val().replace("-", "/").substring(0, 10));
        $('.datepicker').pickadate({
            selectMonths: true, // Creates a dropdown to control month
            selectYears: 15, // Creates a dropdown of 15 years to control year
            format: 'yyyy-mm-dd',
            monthsFull: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
            monthsShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
            weekdaysFull: ['Domingo', 'Lunes', 'Martes', 'Miercoles', 'Jueves', 'Viernes', 'Sabado'],
            weekdaysShort: ['Dom', 'Lun', 'Mar', 'Mie', 'Jue', 'Vie', 'Sab'],
            today: 'Hoy',
            clear: 'Limpiar',
            close: 'Cerrar',
            min: new Date(fecha.getFullYear(), fecha.getMonth(), fecha.getDate() + 2)
        });

        $('select').material_select();


        document.onkeydown = function (e) {
            KeyDownField(e);
        }

        $('#ActividadItemPrecio0').number(true, 0, ',', '.');
        $('#ActividadItemCantidad0').number(true, 0, ',', '.');
        $('#ActividadItemTTL0').number(true, 0, ',', '.');
        $('#ActividadMetaV').number(true, 0, ',', '.');
        $('#ActividadMetaE').number(true, 0, ',', '.');
        $('#Presupuesto_Mes').number(true, 0, ',', '.');
        $('#Presupuesto_Mes_Ejecutado').number(true, 0, ',', '.');


        // Validator

        calculaTotal(2);
        $.validator.addMethod("Presupuestos", function (value, element) {
            var presupuesto = [];
            presupuesto = AnalisisPresupuesto();
            console.log(presupuesto);
            if (presupuesto.length > 0) {
                for (var i = 0; i < presupuesto.length; i++) {

                    if (presupuesto[i].Total < 0) {

                        for (var j = 0; j < TotalCentroCostos.length; j++) {

                            if (presupuesto[i].Id == TotalCentroCostos[j].Id) {

                                for (var k = 0; k < TotalActividades.length; k++) {

                                    if (TotalCentroCostos[j].Id == TotalActividades[k].Id) {

                                        $.validator.messages.Presupuestos = "La actividad no puede superar el presupuesto de " + TotalCentroCostos[j].centroCostoDesc + ": Presupuesto: " + $.number(TotalCentroCostos[j].Presupuesto) + " Gastado: " + $.number(TotalCentroCostos[j].Gasto) + " Actividad: " + $.number(TotalActividades[k].Total);
                                        //+$.number(TotalCentroCostos[i].Presupuesto) + " Gastado: " + $.number(TotalCentroCostos[i].Gasto) + " Actividad: " + $.number(TotalActividades[i].Total);
                                        return false;

                                    }

                                }
                            }
                        }
                    } else if (i == (presupuesto.length - 1)) {
                        return true;
                    }
                }
            } else {
                $.validator.messages.Presupuestos = "El cliente no tiene presupuesto generado para ningún centro de costo.";
                return false;
            }
        }, $.validator.messages.Presupuestos);

        $.validator.addMethod("FechaRango", function (value, element) {
            if (new Date($("#ActividadFechaDesde").val()) - new Date($("#ActividadFechaHasta").val()) > 0) {
                return false;
            } else {
                return true;
            }
        }, "La fecha hasta no puede ser Menor a la fecha desde");

        $.validator.addMethod("FechaMin", function (value, element) {
            var fecha = new Date($("#ActividadFecha").val().replace("-", "/").substring(0, 10));
            var fechaMin = new Date(fecha.getFullYear(), fecha.getMonth(), fecha.getDate() + 2)
            $.validator.messages.FechaMin = "La Fecha Inicio debe  ser mayor a " + fechaMin.toLocaleDateString() + "  (dos dias de la Fecha creación) : " + fecha.toLocaleDateString();
            if (new Date($("#ActividadFechaDesde").val().replace("-", "/").substring(0, 10)) - fechaMin < 0) {
                return false;
            } else {
                return true;
            }
        }, $.validator.messages.FechaMin);

        $.validator.addMethod("precio", function (value, element) {
            var idcampo = $(element).attr('id')
            var index = idcampo.substring(19, 26);
            //ActividadItemPrecio 21

            var prod = $("input[name*='ActividadItemProducto" + index + "']").val();
            console.log(index + ' ' + idcampo + ' ' + prod)

            if (prod == "") {
                return true;
            }
            else if (value == "" || value == 0) {
                return false;
            }
            else {
                return true;
            }

        }, "Por favor ingrese Precio.");

        $.validator.addMethod("cantidad", function (value, element) {
            var idcampo = $(element).attr('id')
            var index = idcampo.substring(21, 27);
            //ActividadItemCantidad0 21

            var prod = $("input[name*='ActividadItemProducto" + index + "']").val();
            console.log(index + ' ' + idcampo + ' ' + prod)

            if (prod == "") {
                return true;
            }
            else if (value == "" || value == 0) {
                return false;
            }
            else {
                return true;
            }

        }, "Por favor ingrese Cantidad.");

        $.validator.addMethod("producto", function (value, element) {
            var idcampo = $(element).attr('id')
            var index = idcampo.substring(19, 26);
            //ActividadItemPrecio 21

            var prod = $("input[name*='ActividadItemProducto" + index + "']").val();
            console.log(index + ' ' + idcampo + ' ' + prod)

            if (prod == "") {
                return true;
            }
            else if (value == "" || value == 0) {
                return false;
            }
            else {
                return true;
            }

        }, "Por favor Seleccione un producto.");

        $.validator.addMethod("centroCosto", function (value, element) {

            var idcampo = $(element).attr('id')
            var index = idcampo.substring(19, 26);
            var prod = $("input[name*='ActividadItemProducto" + index + "']").val();
            console.log(index + ' ' + idcampo + ' ' + prod)

            if (prod == "") {
                return true;
            }
            else if (value == "" || value == null) {
                return false;
            }
            else {
                return true;
            }

        }, "Por favor seleccione un centro de Costo.");

        $.validator.addMethod("productoRepetido", function (value, element) {
            var nroRepetidos = 0;
            //var inputsProductos = $("#TablaItems tbody input[name*='ActividadItemProducto']");
            var inputsProductos = parseInt(document.getElementById("ActividadUltimoItem").value);
            $.validator.messages.productoRepetido = "";
            for (var x = 0; x <= inputsProductos; x++) {////inputsProductos.length
                var productoX = $("#ActividadItemProducto" + (x)).val();
                var centroCostoX = $("#ActividadItemCC" + (x)).val();
                if (productoX != "" && productoX != "undefined" && productoX != null) {
                    for (var y = x + 1; y <= inputsProductos; y++) {//inputsProductos.length
                        var productoY = $("#ActividadItemProducto" + (y)).val();
                        var centroCostoY = $("#ActividadItemCC" + (y)).val();
                        if (productoX == productoY && centroCostoX == centroCostoY) {
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

        $.validator.addMethod("cliente", function (value, element) {
            var cliente = $("input[name*='ClienteID']").val();

            //alert("cantidad");
            if (cliente == "" || cliente == null) {
                return false;
            } else {
                return true;
            }

        }, "Por favor seleccione un cliente.");

        $.validator.addMethod("TipoActividad", function (value, element) {
            var TipoActividad = $("select[name*='TipoActividadID']").val();

            //alert("cantidad");
            if (TipoActividad == "" || TipoActividad == null) {
                return false;
            } else {
                return true;
            }

        }, "Por favor seleccione tipo actividad.");

        $.validator.addMethod("TipoMarca", function (value, element) {
            var TipoActividad = $("select[name*='TipoActividadID']").val();
            var TipoMarca = $("select[name*='MarcaID']").val();

            if (TipoActividad == "FER" && (TipoMarca == "" || TipoMarca == null)) {
                return false;
            } else {
                return true;
            }

        }, "Por favor seleccione una Marca.");

        form = $("#formActiv").validate({
            errorClass: "field-validation-error red-text text-accent-4",
            errorElement: "span",
            onfocusout: false,
            onkeyup: false,
            focusCleanup: true,
            ignore: "",
            rules: {
                ActividadTitulo: {
                    required: true,
                    productoRepetido: true,
                    Presupuestos: true
                },
                ActividadMetaV: { required: true },
                ActividadMetaE: { required: true },
                ActividadLugarEnvioPOP: { required: true },
                ActividadFechaDesde: {
                    required: true,
                    FechaRango: true,
                    date: true,
                    FechaMin: true
                },
                ActividadFechaHasta: {
                    required: true,
                    FechaRango: true,
                    date: true
                },
                ClienteID: { required: true },
                TipoActividadID: { required: true },
                CanalID: { required: true },
                MarcaID: { TipoMarca: true },

                ActividadObjetivo: { required: true },
                ActividadDesc: { required: true },

                ActividadItemProducto0: { required: true },
                ActividadItemPrecio0: { precio: true },
                ActividadItemCantidad0: { cantidad: true },
                ActividadItemCC0: { centroCosto: true }
                //ActividadTitulo: {  }
            },

            messages: {
                ActividadTitulo: { required: 'Por favor ingrese el título de la actividad.' },
                CanalID: 'Por favor seleccione un canal.',
                MarcaID: 'Por favor seleccione una Marca.',
                ClienteID: 'Por favor seleccione un cliente.',
                TipoActividadID: 'Por favor seleccione tipo actividad.',

                ActividadMetaV: "Por favor ingresar meta de venta",
                ActividadMetaE: "Por favor ingresar meta de evacuación",

                ActividadLugarEnvioPOP: "Por favor ingresar un lugar de envío.",

                ActividadFechaDesde: { required: "Por favor ingresar Fecha Desde" },
                ActividadFechaHasta: { required: "Por favor ingresar Fecha Hasta" },

                ActividadObjetivo: "Por favor ingresar objetivo",
                ActividadDesc: "Por favor ingresar descripción",


                ActividadItemCantidad0: 'Por favor ingrese Cantidad.',
                ActividadItemPrecio0: 'Por favor ingrese Precio.',
                ActividadItemProducto0: 'Por favor Seleccione un producto.',
                ActividadItemCC0: 'Por favor Seleccione un Centro de Costo.'
            },

            errorPlacement: function (error, element) {

                error.insertAfter(element);

            },
            submitHandler: function (form) {
                Materialize.updateTextFields();
                $.ajax({
                    url: form.action,
                    type: form.method,
                    data: $(form).serialize(),
                    dataType: 'json',
                    success: function (result) {

                        if (result.validate) {
                            Materialize.toast(result.mensaje, 2000, '', function () { window_close() });
                        } else {
                            Materialize.toast(result.mensaje, 5000, '');
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
                return false;
            }
        });

        $("#ClienteID").change(function () {
            GetCiudad();
            GetSellIn();
            GetpptoVendedor();
        });

        $("#ClienteID").dblclick(function () {
            promptClientes(this);
            //Materialize.toast("dblclick", 4000);
        });

        $("#TipoActividadID").change(function () {
            $("#ActividadMetaV").removeAttr("readonly");
            $("#ActividadMetaE").removeAttr("readonly");
            if ($(this).val()) {
                switch ($(this).val()) {
                    case TipoActiColo:
                        //$("#ActividadMetaV").attr("readonly", "readonly");
                        $("#ActividadMetaE").val(0);
                        $("#ActividadMetaE").attr("readonly", "readonly");
                        break;
                    case TipoActiEva:
                        $("#ActividadMetaV").val(0);
                        $("#ActividadMetaV").attr("readonly", "readonly");
                        break;
                    case TipoActiMix:
                        break;
                }//switch ($(this).val()) {
            }//if ($(this).val()) {
            Materialize.updateTextFields();
        });
        //GetCentroCosto();
    }
    if (TrnMode == 2 || TrnMode == 3) {
        $('#ActividadItemPrecio0').number(true, 0, ',', '.');
        $('#ActividadItemCantidad0').number(true, 0, ',', '.');
        $('#ActividadItemTTL0').number(true, 0, ',', '.');
        $('#ActividadMetaV').number(true, 0, ',', '.');
        $('#ActividadMetaE').number(true, 0, ',', '.');
        $('#Presupuesto_Mes').number(true, 0, ',', '.');
        $('#Presupuesto_Mes_Ejecutado').number(true, 0, ',', '.');
        actividadId = parseInt($("#ActividadId").val());

        //GetCentroCosto();
        GetCiudad();
        GetSellIn();
    }
    GetCentroCosto();
})

$(window).load(function () {
    //DropZone
    //Dropzone.autoDiscover = false;
    //new Dropzone(document.body, {
    //    url: "../Actividades/UploadAction",
    //    maxFileSize: 4,
    //    parallelUploads: 50,
    //    thumbnailWidth: 120,
    //    thumbnailHeight: 120,
    //    uploadMultiple: true,
    //    autoProcessQueue: false,
    //    addRemoveLinks: true,
    //    previewsContainer: "#previews",
    //    clickable: "#tablaanexosdrop",
    //    init: function () {
    //        var submitButton = document.querySelector("#enviardrop")
    //        myDropzone = this; // closure

    //        submitButton.addEventListener("click", function () {
    //            myDropzone.processQueue(); // Tell Dropzone to process all queued files.
    //        });

    //        myDropzone.on("addedfile", function (file) {
    //            myDropzone.emit("thumbnail", file, "../Scripts/dropzone/File.png");
    //        });
    //        myDropzone.on("complete", function (file) {
    //            myDropzone.removeFile(file);
    //        });
    //    }
    //});

})

function window_close(e) {
    window.location.href = '/Actividades'
}

window.onload = function (e) {

    var table = document.getElementById("TablaItems").getElementsByTagName('tbody')[0];
    var x = table.rows.length;

    if (x == 1) {
        AddRowItems();
    }
}

function AddAnexos(ruta, nombre, fecha) {

    var VarActividadUltimoItem = document.getElementById("ActividadUltimoAnexo").value

    for ($i = 0; $i < 4; $i++) {
        var table = document.getElementById("TablaAnexos").getElementsByTagName('tbody')[0];
        VarActividadUltimoItem++;
        var x = table.rows.length;
        var indice = x;
        var row = table.insertRow(indice);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);
        var cell3 = row.insertCell(3);
        var strindice = "";
        if (indice != 0) {
            strindice = indice.toString();
        }
        var item = (x + 1).toString();
        var EventJs;


        var indice = parseInt(strindice) + 1;

        cell0.innerHTML = "<button style='transform: scaleY(0.6) scaleX(0.6)' id='BtnEliminarItemActividad' type='button' class='btn-floating btn-large waves-effect waves-light red'  aria-hidden='true' onclick='DeleteRowActividades(this);'><i class='material-icons'>delete</i></button>"
        cell1.innerHTML = "<input class='texttip form-control input-sm' id='ActividadAnexosId" + strindice + "' value= '" + (VarActividadUltimoItem + 1) + "' name='ActividadAnexosId" + strindice + "' placeholder='Id' type='text' readonly ='readonly' />"
        cell2.innerHTML = "<input class='texttip form-control input-sm' id='ActividadAnexosDesc" + strindice + "' value = '" + nombre + "' name='ActividadAnexosDesc" + strindice + "' placeholder='Nombre del archivo' readonly='readonly' type='text' />"
        cell3.innerHTML = "<input class='texttip form-control input-sm' id='ActividadAnexosFecha" + strindice + "' value = '" + fecha + "' name='ActividadAnexosFecha0" + strindice + "' placeholder='DD/MM/AAAA' readonly='readonly' type='text' />"



        $("#TablaAnexos tbody tr td > input[name*=ActividadItemId]").each(function (index) {

            $(this).parent().addClass("hide")

        })

        $("#TablaAnexos tbody tr td > input[name*=ActividadAnexosDesc]").each(function (index) {

            $(this).parent().addClass("input-field col s8")

        })

    }
    document.getElementById("ActividadUltimoAnexo").value = VarActividadUltimoItem;



}

function AddRowItems(datarow) {
    var numLineas = 1
    var VarActividadUltimoItem = parseInt(document.getElementById("ActividadUltimoItem").value);

    for ($i = 0; $i < numLineas; $i++) {
        var table = document.getElementById("TablaItems").getElementsByTagName('tbody')[0];
        VarActividadUltimoItem++;

        var x = parseInt(document.getElementById("ActividadUltimoItem").value);//table.rows.length;

        var indice = x;
        var row = table.insertRow(table.rows.length);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);
        var cell3 = row.insertCell(3);
        var cell4 = row.insertCell(4);
        var cell5 = row.insertCell(5);
        var cell6 = row.insertCell(6);
        var cell7 = row.insertCell(7);
        var strindice = "0";
        if (indice != 0) {
            strindice = indice.toString();
        }
        var item = (x + 1).toString();
        $("#ActividadUltimoItem").val(item);
        var EventJs;


        var indice = parseInt(strindice) + 1;
        if (TrnMode == 3) {
            cell0.innerHTML = "";
        }
        else {
            cell0.innerHTML = "<button style='transform: scaleY(0.6) scaleX(0.6)' id='BtnEliminarItemActividad' type='button' class='btn-floating btn-large waves-effect waves-light red' aria-hidden='true' onclick='DeleteRowActividades(this);'><i class='material-icons'>delete</i></button>"
        }
        var ccOptions = "";
        for (var i = 0; i < CentroCostos.length; i++) {
            ccOptions = ccOptions.concat("<option value='" + CentroCostos[i].centroCostoId + "'>" + CentroCostos[i].centroCostoDesc + "</option>");
        }

        cell1.innerHTML = "<input class='texttip form-control input-sm' id='ActividadItemId" + strindice + "' name='ActividadItemId" + strindice + "' placeholder='Id' type='text' value='" + (VarActividadUltimoItem + 1) + "' readonly='readonly' />"
        cell2.innerHTML = "<input class='texttip form-control input-sm tooltipped' id='ActividadItemProducto" + strindice + "' name='ActividadItemProducto" + strindice + "' placeholder='Producto' type='text' ondblclick='promptProductos(" + strindice + ");'  data-position='top' data-delay='50' data-tooltip='Doble clic para buscar productos' onchange='consultaProducto(this);' autocomplete='off'/>"
        cell3.innerHTML = "<input class='texttip form-control input-sm' id='ActividadItemDesc" + strindice + "'name='ActividadItemDesc" + strindice + "' placeholder='Descripción del producto' readonly ='readonly'/>"
        cell4.innerHTML = "<input class='texttip form-control input-sm' id='ActividadItemPrecio" + strindice + "' name='ActividadItemPrecio" + strindice + "' placeholder='Precio' type='text' onkeyup='calculaTotal();' onblur='calculaTotal(" + strindice + ");'/>"
        cell5.innerHTML = "<input class='texttip form-control input-sm' id='ActividadItemCantidad" + strindice + "' name='ActividadItemCantidad" + strindice + "' placeholder='Cantidad' type='text' onkeyup='calculaTotal();' onblur='calculaTotal(" + strindice + ");' />"
        cell6.innerHTML = "<input class='texttip form-control input-sm' id='ActividadItemTTL" + strindice + "' name='ActividadItemTTL" + strindice + "' placeholder='Total' type='text' onkeyup='calculaTotal();'  onchange='calculaTotal(" + strindice + ");' onblur='calculaTotal(" + strindice + ");' readonly ='readonly' />"
        if (TrnMode != 3 && datarow == null) {
            cell7.innerHTML = "<select id='ActividadItemCC" + strindice + "' name='ActividadItemCC" + strindice + "'  onchange='calculaTotal(" + strindice + ");'  class='initialized'><option value='' disabled selected>Centro Costo...</option>" + ccOptions + "</select>"
        } else {
            cell7.innerHTML = "<select id='ActividadItemCC" + strindice + "' name='ActividadItemCC" + strindice + "'  onchange='calculaTotal(" + strindice + ");'  class='initialized'><option value='' disabled selected>Centro Costo...</option></select>"
        }
        $('#ActividadItemPrecio' + strindice).number(true, 0, ',', '.');
        $('#ActividadItemCantidad' + strindice).number(true, 0, ',', '.');
        $('#ActividadItemTTL' + strindice).number(true, 0, ',', '.');
        $('#ActividadItemCC' + strindice).material_select();

        if (TrnMode == 1 || TrnMode == 2) {
            /*Agregar reglas jquery Validate*/
            $('#ActividadItemPrecio' + strindice).rules("add", {
                precio: true
            });
            $('#ActividadItemCantidad' + strindice).rules("add", {
                cantidad: true
            });
            $('#ActividadItemId' + strindice).rules("add", {
                precio: true
            });
            $('#ActividadItemCC' + strindice).rules("add", {
                centroCosto: true
            });
            /*Agregar reglas jquery Validate*/
        }

        //var campo = document.getElementById("ActividadItemId*");
        // campo.parent().addClass('yourClass');

        $("#TablaItems tbody tr td > input[name*=ActividadItemId]").each(function (index) {

            $(this).parent().addClass("hide")
        })

    }
    // document.getElementById("ActividadUltimoItem").value = VarActividadUltimoItem;


    if (datarow != null && (TrnMode == 2 || TrnMode == 3)) {

        $('input[name*="' + ("ActividadItemId" + strindice)).val(datarow.ActividadItemId);
        $('input[name*="' + ("ActividadItemDesc" + strindice)).val(datarow.ActividadItemDescripcion);
        $('input[name*="' + ("ActividadItemProducto" + strindice)).val(datarow.ProductoId);
        $('input[name*="' + ("ActividadItemPrecio" + strindice)).val(datarow.ActividadItemPrecio);
        $('input[name*="' + ("ActividadItemCantidad" + strindice)).val(datarow.ActividadItemCantidad);
        $("select[name*='" + ('ActividadItemCC' + strindice + "']")).val(datarow.CentroCostoID);

        var select = document.getElementById("ActividadItemCC" + strindice);
        for (var i = 0; i < CentroCostos.length; i++) {

            var opciones = document.createElement("option");
            opciones.value = CentroCostos[i].centroCostoId;
            opciones.text = CentroCostos[i].centroCostoDesc;

            if (opciones.value == datarow.CentroCostoID) {
                opciones.selected = true;
            }
            select.add(opciones);
        }
        //document.getElementById("ActividadUltimoItem").value = datarow.ActividadItemId;
        calculaTotal(strindice);

        if (TrnMode == 3) {
            $('input[name*="' + ("ActividadItemId" + strindice)).attr("disabled", "disabled");
            $('input[name*="' + ("ActividadItemDesc" + strindice)).attr("disabled", "disabled");
            $('input[name*="' + ("ActividadItemProducto" + strindice)).attr("disabled", "disabled");
            $('input[name*="' + ("ActividadItemPrecio" + strindice)).attr("disabled", "disabled");
            $('input[name*="' + ("ActividadItemCantidad" + strindice)).attr("disabled", "disabled");
            document.getElementById("ActividadItemCC" + strindice).disabled = true;
        }
        $('#ActividadItemCC' + strindice).material_select();
    }

}

function GetCentroCosto() {

    CentroCostos = [];
    $.ajax({
        cache: false,
        type: "GET",
        url: "/Actividades/GetCentroCosto",
        success: function (result) {

            $.each(result, function (id, option) {

                var newCC = {};
                newCC.centroCostoId = option.centroCostoId;
                newCC.centroCostoDesc = option.centroCostoDesc;
                CentroCostos.push(newCC);

                if (document.getElementById("ActividadItemCC0")) {

                    var select = document.getElementById("ActividadItemCC0");
                    var opciones = document.createElement("option");
                    opciones.text = option.centroCostoDesc;
                    opciones.value = option.centroCostoId;
                    select.add(opciones);

                }
            });
            $('#ActividadItemCC0').material_select();
            AddRowPresupuestos();
            if (TrnMode == 2 || TrnMode == 3) {
                CargarDetalles(ActividadItems, ActividadAut);
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            Materialize.toast('Error al cargar los items!!', 4000);//alert('Error al cargar los items!!');
        }
    });
    //var h = 2;

    //var vl_ClienteId = document.getElementById('ClienteID').value;
    //var obj_Cliente = Clientes.filter(c => c.ClienteID == vl_ClienteId);
    ////Si existe el Cliente
    //if (obj_Cliente.length > 0) {
    //    $("#ClienteNombre").removeClass("red-text");
    //    $("#Cliente_name").val(obj_Cliente[0].ClienteRazonSocial);
    //    $("#ClienteNombre").val(obj_Cliente[0].ClienteRazonSocial);
    //    console.log(obj_Cliente[0].ClienteRazonSocial + "-" + obj_Cliente[0].CanalID);
    //    $('#CanalID').val(obj_Cliente[0].CanalID);
    //    $('#CanalID').material_select();

    //} else {
    //    //No Existe el cliente
    //    $("#ClienteID").val("");
    //    $("#Cliente_name").val("");
    //    $("#ClienteNombre").val("Cliente " + vl_ClienteId + " no encontrado").addClass("red-text");
    //    //$("#ClienteNombre").val("");
    //}
}

function AddRowPresupuestos() {

    for (var i = 0; i < CentroCostos.length; i++) {

        var table = document.getElementById("TablaPresupuestos").getElementsByTagName('tbody')[0];

        var x = table.rows.length;

        var indice = x;
        var row = table.insertRow(indice);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);
        var cell3 = row.insertCell(3);
        var strindice = "0";
        if (indice != 0) {
            strindice = indice.toString();
        }
        var item = (x + 1).toString();
        var EventJs;


        var indice = parseInt(strindice) + 1;

        cell0.innerHTML = "<input class='texttip form-control input-sm' id='CC_" + CentroCostos[i].centroCostoId + "' name='CC_" + CentroCostos[i].centroCostoId + "' value='" + CentroCostos[i].centroCostoDesc + "' readonly='readonly' type='text' />"
        cell1.innerHTML = "<input class='texttip form-control input-sm' id='PresupuestoQ_" + CentroCostos[i].centroCostoId + "' name='PresupuestoQ_" + CentroCostos[i].centroCostoId + "' placeholder='Presupuesto Q' readonly='readonly' type='text' />"
        cell2.innerHTML = "<input class='texttip form-control input-sm' id='PresupuestoQ_Ejec_" + CentroCostos[i].centroCostoId + "' name='PresupuestoQ_Ejec_" + CentroCostos[i].centroCostoId + "' placeholder='Presupuesto Q Ejecutado' readonly='readonly' type='text' />"
        cell3.innerHTML = "<input class='texttip form-control input-sm' id='PresupuestoQ_Disp_" + CentroCostos[i].centroCostoId + "' name='PresupuestoQ_Disp_" + CentroCostos[i].centroCostoId + "' placeholder='Presupuesto Q Disponible' readonly='readonly' type='text' />"
    }
}

function AddRowAutorizadores(datarow) {
    var numLineas = 1

    for ($i = 0; $i < numLineas; $i++) {
        var table = document.getElementById("TablaAutorizadores").getElementsByTagName('tbody')[0];

        var x = table.rows.length;

        var indice = x;
        var row = table.insertRow(indice);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);
        var cell3 = row.insertCell(3);
        var strindice = "0";
        if (indice != 0) {
            strindice = indice.toString();
        }
        var item = (x + 1).toString();
        var EventJs;


        var indice = parseInt(strindice) + 1;



        cell0.innerHTML = "<div class='input-field col s12'><i class='material-icons prefix'>account_circle</i><input class='texttip form-control input-sm' id='ActividadAutUsuarioNombre' name='ActividadAutUsuarioNombre" + strindice + "' readonly='readonly' placeholder='Usuario Autorizador' type='text' /></div>"
        cell1.innerHTML = "<input class='texttip form-control input-sm' id='ActividadAutorizacionFecha" + strindice + "' name='ActividadAutorizacionFecha" + strindice + "' readonly='readonly' placeholder='Usuario Autorizador' type='text' />"
        cell2.innerHTML = "<input id='ActividadAutorizacionAutoriza" + strindice + "' name='ActividadAutorizacionAutoriza" + strindice + "' readonly='readonly' type='checkbox' /> <label for='ActividadAutorizacionAutoriza'  + strindice></label>"
        cell3.innerHTML = "<input class='texttip form-control input-sm' id='ActividadAutorizacionMotivo" + strindice + "' name='ActividadAutorizacionMotivo" + strindice + "' readonly='readonly' placeholder='Motivo' type='text' />"


    }

    if (datarow != null && (TrnMode == 3)) {

        //var fecha = new Date(datarow.ActividadAutorizacionFecha);
        //var day = (fecha.getDate().toString().length == 1) ? '0' + fecha.getDate().toString() : fecha.getDate().toString();
        //var mes = fecha.getMonth() + 1;
        //var mes = (mes.toString().length == 1) ? '0' + mes.toString() : mes;
        //var fecha2 = day + '/' + mes + '/' + fecha.getFullYear();
        var milli = datarow.ActividadAutorizacionFecha.replace(/\/Date\((-?\d+)\)\//, '$1');
        var fecha = new Date(parseInt(milli));
        var day = (fecha.getDate().toString().length == 1) ? '0' + fecha.getDate().toString() : fecha.getDate().toString();
        var mes = fecha.getMonth() + 1;
        var mes = (mes.toString().length == 1) ? '0' + mes.toString() : mes;
        var FechaResult = day + '/' + mes + '/' + fecha.getFullYear();



        $('input[name*="' + ("ActividadAutUsuarioNombre" + strindice)).val(datarow.UsuarioNombre);
        $('input[name*="' + ("ActividadAutorizacionFecha" + strindice)).val(FechaResult);
        if (datarow.ActividadAutorizacionAutoriza == 2) {
            $('input[name*="' + ("ActividadAutorizacionAutoriza" + strindice)).attr('checked', datarow.ActividadAutorizacionAutoriza);
        }
        $('input[name*="' + ("ActividadAutorizacionMotivo" + strindice)).val(datarow.ActividadAutorizacionMotivo);
        if (TrnMode == 3) {
            $('input[name*="' + ("ActividadAutUsuarioNombre" + strindice)).attr("disabled", "disabled");
            $('input[name*="' + ("ActividadAutorizacionFecha" + strindice)).attr("disabled", "disabled");
            $('input[name*="' + ("ActividadAutorizacionAutoriza" + strindice)).attr("disabled", "disabled");
            $('input[name*="' + ("ActividadAutorizacionMotivo" + strindice)).attr("disabled", "disabled");
        }
    }
}

function DeleteRowActividades(r) {

    var i = r.parentNode.parentNode.rowIndex;

    $.ajax({
        url: '/actividades/_ConfirmarEliminar',
        data: { i: i },
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html'

    })
  .success(function (result) {
      $('#modales').html(result);
      $('#_ConfirmarEliminar').openModal();

  })
  .error(function (xhr, status) {
      Materialize.toast(status, 4000);//alert(status);
  });

}

function ConfirmaDeleteActividad(i) {
    document.getElementById("TablaItems").deleteRow(i);
    calculaTotal(i);
    $('#_ConfirmarEliminar').closeModal()
}

function GetCiudad() {
    $.ajax({
        cache: false,
        type: "GET",
        url: "/Actividades/GetCiudadCliente",
        data: { "clienteID": document.getElementById('ClienteID').value },
        success: function (data) {
            document.getElementById('CiudadID').value = data;
        },
        error: function (xhr, ajaxOptions, thrownError) {
            Materialize.toast('Error al cargar los items!!', 4000);//alert('Error al cargar los items!!');
        }
    });
}

 function GetSellIn() {
        $.ajax({
            cache: false,
            type: "GET",
            url: "/Actividades/GetSellInCliente",
            data: { "clienteID": document.getElementById('ClienteID').value },
            success: function (data) {
                document.getElementById('SellInID').value = data;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                Materialize.toast('Error al cargar los items!!', 4000);//alert('Error al cargar los items!!');
            }
        });

    var h = 2;


    var vl_ClienteId = document.getElementById('ClienteID').value;
    var obj_Cliente = Clientes.filter(c => c.ClienteID == vl_ClienteId);
    //Si existe el Cliente
    if (obj_Cliente.length > 0) {
        $("#ClienteNombre").removeClass("red-text");
        $("#Cliente_name").val(obj_Cliente[0].ClienteRazonSocial);
        $("#ClienteNombre").val(obj_Cliente[0].ClienteRazonSocial);
        console.log(obj_Cliente[0].ClienteRazonSocial + "-" + obj_Cliente[0].CanalID);
        $('#CanalID').val(obj_Cliente[0].CanalID);
        $('#CanalID').material_select();

    } else {
        //No Existe el cliente
        $("#ClienteID").val("");
        $("#Cliente_name").val("");
        $("#ClienteNombre").val("Cliente " + vl_ClienteId + " no encontrado").addClass("red-text");
        //$("#ClienteNombre").val("");
    }
}

function GetpptoVendedor() {

    $.ajax({
        cache: false,
        type: "GET",
        url: "/Actividades/GetVendedorName",
        data: { "vendedorID": document.getElementById('UsuarioIdElabora').value },
        success: function (data) {
            document.getElementById('Vendedor_name').value = data;
        },
        error: function (xhr, ajaxOptions, thrownError) {
            Materialize.toast('Error al cargar el nombre del vendedor!!', 4000); //alert('Error al cargar presupuesto del vendedor!!');
        }
    });

    var clienteId = document.getElementById('ClienteID').value;
    if (clienteId != "") {
        var fecha = new Date($("#ActividadFecha").val().replace("-", "/").substring(0, 10));
        var FechaStr = pad(fecha.getDate()) + "/" + pad(fecha.getMonth() + 1) + "/" + fecha.getFullYear();
        var aaa = 0;
        TotalCentroCostos = [];

        $.ajax({
            cache: false,
            type: "GET",
            url: "/Actividades/GetpptoVendedor",
            data: { "ClienteId": clienteId, FechaActividad: FechaStr, actividadId: actividadId },
            success: function (data) {
                $.each(data, function (id, option) {

                    aaa += 1;

                    for (var i = 0; i < CentroCostos.length; i++) {

                        if (option.centroCostoId == CentroCostos[i].centroCostoId) {
                            document.getElementById('PresupuestoQ_' + option.centroCostoId).value = option.presupuesto;
                            document.getElementById('PresupuestoQ_Ejec_' + option.centroCostoId).value = option.gasto;
                            document.getElementById('PresupuestoQ_Disp_' + option.centroCostoId).value = option.presupuesto - option.gasto;
                            $('#PresupuestoQ_' + option.centroCostoId).number(true, 0, ',', '.');
                            $('#PresupuestoQ_Ejec_' + option.centroCostoId).number(true, 0, ',', '.');
                            $('#PresupuestoQ_Disp_' + option.centroCostoId).number(true, 0, ',', '.');
                            removeItemArray(TotalCentroCostos, "Id", option.centroCostoId);
                            var newCC = {};
                            newCC.Id = option.centroCostoId;
                            newCC.centroCostoDesc = option.centroCostoDesc;
                            newCC.Presupuesto = option.presupuesto;
                            newCC.Gasto = option.gasto;
                            TotalCentroCostos.push(newCC);
                            aaa = 0;
                        } else {
                            if (aaa == 0) {
                                document.getElementById('PresupuestoQ_' + CentroCostos[i].centroCostoId).value = 0;
                                document.getElementById('PresupuestoQ_Ejec_' + CentroCostos[i].centroCostoId).value = 0;
                                document.getElementById('PresupuestoQ_Disp_' + CentroCostos[i].centroCostoId).value = 0;
                                $('#PresupuestoQ_' + CentroCostos[i].centroCostoId).number(true, 0, ',', '.');
                                $('#PresupuestoQ_Ejec_' + CentroCostos[i].centroCostoId).number(true, 0, ',', '.');
                                $('#PresupuestoQ_Disp_' + CentroCostos[i].centroCostoId).number(true, 0, ',', '.');
                                removeItemArray(TotalCentroCostos, "Id", CentroCostos[i].centroCostoId);
                                var newCC = {};
                                newCC.Id = CentroCostos[i].centroCostoId;
                                newCC.centroCostoDesc = CentroCostos[i].centroCostoDesc;
                                newCC.Presupuesto = 0;
                                newCC.Gasto = 0;
                                TotalCentroCostos.push(newCC);
                            }
                        }

                    }
                    //PresupuestoMes = data.presupuesto;
                    //GastoMes = data.gasto;
                });
                AnalisisPresupuesto();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                Materialize.toast('Cliente no tiene presupuesto cargado!', 4000); //alert('Error al cargar presupuesto del vendedor!!');
            }
        });

        $.ajax({
            cache: false,
            type: "GET",
            url: "/Actividades/GetVentaxCliente",
            data: { "ClienteID": clienteId },
            success: function (data) {


                $.each(data, function (id, option) {

                    //var e = document.getElementById("ClienteID");
                    //var selectedOp = e.options[e.selectedIndex].text;

                    //  document.getElementById('Cliente_name').value = selectedOp;

                    //                    document.getElementById('LabelVentaP1').innerHTML = 'Venta Enero';
                    document.getElementById('VentaP1').value = option.venta;

                    $('#VentaP1').number(true, 0, ',', '.');

                    //document.getElementById('LabelVentaP2').innerHTML = 'Venta Febrero';
                    //document.getElementById('VentaP2').value = option.mes;

                    //document.getElementById('LabelVentaP3').innerHTML = 'Venta Marzo';
                    //document.getElementById('VentaP3').value = option.ano;
                });
            },
            error: function (xhr, ajaxOptions, thrownError) {
                Materialize.toast('Error al cargar venta del cliente!!', 4000); //alert('Error al cargar presupuesto del vendedor!!');
            }
        });

    }
}

function promptProductos(i) {
    var tMovimiento = $("#TipoMovimientoID").val();

    // if (tMovimiento != "") {
    //if (tMovimiento != ConfigTipoMovAjSalida) {
    //    //prompt de productos
    //    var estado = $("#MovimientoEstado" + i).val();
    // if (estado != 4 && estado != 3) {
    indexTable = i;
    $.ajax({
        url: '/productos/_productos',
        data: { ValidaInventario: true },
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html'

    })
   .success(function (result) {
       $('#_ModalSheet').html(result);
   })
   .error(function (xhr, status) {
       alert(xhr.responseText);
   })
    .complete(function (xhr, status) {
        if (status == "success") {
            $('#_ModalSheet').openModal();
        }
    });
    //  }
    //} else {
    //prompt de productos con inventario
    //var estado = $("#MovimientoEstado" + i).val();

    ////if ((estado != 4 && estado != 3)) {
    //if ((3 == 3)) {
    //    indexTable = i;
    //    $.ajax({
    //        url: '/Movimientos/_ListaProductosDisponible',
    //        data: {},
    //        contentType: 'application/html; charset=utf-8',
    //        type: 'GET',
    //        dataType: 'html'

    //    })
    //   .success(function (result) {
    //       $('#modales').html(result);
    //   })
    //   .error(function (xhr, status) {
    //       alert(xhr.responseText);
    //   })
    //    .complete(function (xhr, status) {
    //        if (status == "success") {
    //            $('#_Modal').closeModal();
    //            $('#_Modal').openModal();
    //        }
    //    });
    //}
    // }
    //   } else {
    //      Materialize.toast("Primero debe seleccionar un tipo de movimiento");
    // }
}



function selectProductos(id, desc) {

    $("#ActividadItemProducto" + indexTable).val(id);
    $("#ActividadItemDesc" + indexTable).val(desc);
    $('#_ModalSheet').closeModal();
    GetProductPrice(id, indexTable);
    //  $("#ProductoId" + indexTable).blur();
}

function calculaTotal(idx) {

    //var precio = parseInt($("input[name*='ActividadItemPrecio" + i + "']").val())
    //var cantidad = parseInt($("input[name*='ActividadItemCantidad" + i + "']").val())
    //$("#ActividadItemTTL" + i).val(precio * cantidad);

    //var totales = $("input[name*='ActividadItemTTL']")
    //var grantotal = 0;
    //for (var i = 0; i <= totales.length - 1; i++) {
    //    if ($("input[name*='ActividadItemTTL" + i + "']").val() != "") {
    //        grantotal += parseInt($("input[name*='ActividadItemTTL" + i + "']").val())
    //    }
    //}
    //document.getElementById("GrandTTLField").innerHTML = $.number(grantotal);
    //TotalActividad = grantotal;
    //AnalisisPresupuesto();


    //var precio = parseInt($("input[name*='ActividadItemPrecio" + i + "']").val())
    //var cantidad = parseInt($("input[name*='ActividadItemCantidad" + i + "']").val())
    //$("#ActividadItemTTL" + i).val(precio * cantidad);

    var totales = 0;
    var linesTotal = $("#ActividadUltimoItem").val();
    var grantotal = 0;
    var totalMarket = 0;
    var totalSale = 0;
    var totalOther = 0;
    TotalActividades = [];

    for (var i = 0; i <= linesTotal; i++) {
        if ($("input[name*='ActividadItemPrecio" + i + "']").val() != ""
             && $("input[name*='ActividadItemPrecio" + i + "']").val() != "undefined"
             && $("input[name*='ActividadItemPrecio" + i + "']").val() != null) {
            var centroCosto = $("select[name*='ActividadItemCC" + i + "']").val();
            var precio = parseInt($("input[name*='ActividadItemPrecio" + i + "']").val())
            var cantidad = parseInt($("input[name*='ActividadItemCantidad" + i + "']").val())
            precio = (precio == "") ? 0 : precio;
            cantidad = (cantidad == "") ? 0 : cantidad;
            totales = (precio * cantidad);
            $("#ActividadItemTTL" + i).val(totales);

            if (centroCosto != "" && centroCosto != null) {
                if (centroCosto == "MER") {

                    removeItemArray(TotalActividades, "Id", centroCosto);
                    totalMarket += totales;
                    var newCC = {};
                    newCC.Id = centroCosto;
                    newCC.Total = totalMarket;
                    TotalActividades.push(newCC);

                } else if (centroCosto == "VEN") {

                    removeItemArray(TotalActividades, "Id", centroCosto);
                    totalSale += totales;
                    var newCC = {};
                    newCC.Id = centroCosto;
                    newCC.Total = totalSale;
                    TotalActividades.push(newCC);

                } else if (centroCosto == "SIS") {

                    removeItemArray(TotalActividades, "Id", centroCosto);
                    totalOther += totales;
                    var newCC = {};
                    newCC.Id = centroCosto;
                    newCC.Total = totalOther;
                    TotalActividades.push(newCC);

                }
            }
            else {

                totalOther += totales;

            }
            grantotal += parseInt($("input[name*='ActividadItemTTL" + i + "']").val())
        }
    }
    document.getElementById("GrandTTLField").innerHTML = $.number(grantotal);
    document.getElementById("GrandTTLMarketField").innerHTML = $.number(totalMarket);
    document.getElementById("GrandTTLSaleField").innerHTML = $.number(totalSale);
    document.getElementById("GrandTTLOtherField").innerHTML = $.number(totalOther);
    //TotalActividad = grantotal;

    GetpptoVendedor();

}

function KeyDownField(e) {
    name = e.srcElement.id;
    if (e.ctrlKey == true && e.keyCode == 32) {
        // acción para ctrl + space y evitar que ejecute la acción propia del navegador               
        AddRowItems();
        return false;
    }
}

function GetProductPrice(ProductoID, i) {

    $.ajax({
        cache: false,
        type: "GET",
        url: "/Actividades/GetProductPrice",
        data: { "ProductoID": ProductoID },
        success: function (data) {
            if (data > 0) {
                document.getElementById("ActividadItemPrecio" + i).value = data;
                $("input[name='ActividadItemPrecio" + i + "']").attr("readonly", "readonly")
                $("input[name='ActividadItemCantidad" + i + "']").focus()
                $("#ActividadItemPrecio" + i).number(true, 0, ',', '.');
            }
            else {
                document.getElementById("ActividadItemPrecio" + i).value = 0;
                $("input[name='ActividadItemPrecio" + i + "']").removeAttr("readonly")
                $("input[name='ActividadItemPrecio" + i + "']").focus()
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            Materialize.toast('Error al cargar valor producto!!', 4000); //alert('Error al cargar valor producto!!');
        }
    });
}

function CargarDetalles(ActividadItems, ActividadAut) {

    $(ActividadItems).each(function (index, item) {
        AddRowItems(item);
    });
    if (TrnMode == 3) {
        $(ActividadAut).each(function (index, item) {
            AddRowAutorizadores(item);
        });
    }

}

function consultaProducto(o) {
    var id = $(o).val();

    if (id != "") {
        var idcampo = $(o).attr('id')
        var index = idcampo.substring(21, 28);
        console.log(index);
        $.ajax({
            url: '../productos/infoProducto',
            data: { ProductoId: id, ValidaInventario: true },
            contentType: 'application/html; charset=utf-8',
            type: 'GET',
            dataType: 'json'
        })
        .success(function (result) {
            if (result.ProductoId != null) {
                $("input[name='ActividadItemProducto" + index + "']").val(result.ProductoId);
                $("input[name='ActividadItemDesc" + index + "']").val(result.ProductoDesc);

                GetProductPrice(id, index);
            } else {
                $("input[name='ActividadItemProducto" + index + "']").val("");
                $("input[name='ActividadItemDesc" + index + "']").val("Producto " + id + " no encontrado").addClass("red-text");
                $("input[name='ActividadItemCantidad" + index + "']").val(0);
                $("input[name='ActividadItemPrecio" + index + "']").attr("readonly", "readonly")
                $("input[name='ActividadItemPrecio" + index + "']").val(0);
                //$("input[name='ActividadItemTTL" + index + "']").val(0);
                calculaTotal(index);
            }
        })
        .error(function (xhr, status) {
            $("input[name='ActividadItemDesc" + index + "']").val("");

            Materialize.toast(status, 4000)

        });

    }
}


function promptClientes(obj) {
    var ClienteId = $(obj).val();
    //if (ClienteId != "") {
    $.ajax({
        url: '/Clientes/_Clientes',
        data: {},
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html'

    })
        .success(function (result) {
            $('#_ModalSheet').html(result);
        })
        .error(function (xhr, status) {
            alert(xhr.responseText);
        })
        .complete(function (xhr, status) {
            if (status == "success") {
                $('#_ModalSheet').openModal();
            }
        });

    //}//if (ClienteId != "") {
}

function selectCliente(ClienteID) {
    $("#ClienteID").val(ClienteID);
    $("#ClienteID").change();
    $('#_ModalSheet').closeModal();
}

function pad(n) { return n < 10 ? "0" + n : n; }

//Para eliminar Items del Array
function removeItemArray(object, key, value) {
    if (value == undefined)
        return;

    for (var i in object) {
        if (object[i][key] == value) {
            object.splice(i, 1);
        }
    }
}

function AnalisisPresupuesto() {
    // GetpptoVendedor();

    //var presupuesto = 0;
    //var gasto = 0;
    //var total = 0;
    var cc = [];
    var ta = [];
    var result = [];
    cc = TotalCentroCostos;
    ta = TotalActividades;

    for (var i = 0; i < cc.length; i++) {
        for (var j = 0; j < ta.length; j++) {

            if (ta[j].Id == cc[i].Id) {
                var newResult = {};
                newResult.Id = ta[j].Id;
                newResult.Total = cc[i].Presupuesto - (cc[i].Gasto + ta[j].Total);
                result.push(newResult);
            }

        }
    }
    return result;
    //presupuesto = PresupuestoMes;
    //gasto = GastoMes;
    //total = TotalActividad;


    //return presupuesto - (gasto + total);
    // $("#Error").html(" Presupuesto: " + $.number(PresupuestoMes) + " Gastado: " + $.number(GastoMes) + " Actividad: " + $.number(TotalActividad));
}

function ResetValidation() {
    $('span.field-validation-error').remove();
}