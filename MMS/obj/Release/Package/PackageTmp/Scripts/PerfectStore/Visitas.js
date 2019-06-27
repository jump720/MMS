var photostag;
$.MMS.Visitas = function (mod, photos, TipoIndustriaAutomotriz) {
    mod = mod.toLowerCase();

    var datePickerOpts = {
        format: 'YYYY-MM-DD',
        weekStart: 1,
        time: false
    };

    Vue.config.devtools = true;

    $("#Visita_TipoVisitaId").select2({
        placeholder: "Search Type of Visit",
        theme: "bootstrap",
    });

    $("#Visita_TipoIndustriaId").select2({
        placeholder: "Search Industry",
        theme: "bootstrap",
    });

    $("#Visita_PaisId").select2({
        placeholder: "Search Country",
        theme: "bootstrap",
    });

    $("#Visita_CiudadId").select2({
        placeholder: "Search City",
        theme: "bootstrap",
    });

    $("#Visita_DepartamentoId").select2({
        placeholder: "Search State",
        theme: "bootstrap",
    });

    $("#Marcas").select2({
        placeholder: "Search Brands",
        theme: "bootstrap",
    });

    $("#Marcas").change(function () {
        if ($(this).val() != null)
            $("#Visita_Marcas").val($(this).val().toString());
    });

    $("#Visita_TipoIndustriaId").change(function () {
        //msgSuccess($(this).val() + ' == ' + TipoIndustriaAutomotriz);
        if ($(this).val() == TipoIndustriaAutomotriz) {
            $("#Marcas").prop("disabled", false);
            $("#Visita_DisponibilidadProducto").prop("disabled", false);
            $("#Visita_NumeroMecanicos").prop("readonly", false);
        } else {
            $("#Marcas").prop("disabled", true);
            $("#Visita_DisponibilidadProducto").prop("disabled", true);
            $("#Visita_NumeroMecanicos").prop("readonly", true);
        }
    });

    if ($("#Visita_TipoIndustriaId").val() == TipoIndustriaAutomotriz) {
        $("#Marcas").prop("disabled", false);
        $("#Visita_DisponibilidadProducto").prop("disabled", false);
        $("#Visita_NumeroMecanicos").prop("readonly", false);
    } else {
        $("#Marcas").prop("disabled", true);
        $("#Visita_DisponibilidadProducto").prop("disabled", true);
        $("#Visita_NumeroMecanicos").prop("readonly", true);
    }


    $("#Visita_VentaRealizada").inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    $("#Visita_Nit").change(function () {

        if ($(this).val() != "") {
            $.get("/api/VisitasWeb/GetDV?nit=" + $(this).val())
              .done(function (result) {
                  if (result) {
                      console.log(result.Data);
                      $("#Visita_Dv").val(result.Data);
                  }
              })
              .fail(function (jqXHR, textStatus, errorThrown) {
                  msgError(errorThrown);
              });
        } else {
            $("#Visita_Dv").val("");
        }
    })

    var vuePhotosTable = new Vue({
        el: "#ListPhotos",
        data: {
            photos: photos
        },
        methods: {
            addNewPhoto: function (event) {
                this.photos.push({ order: this.photos.length + 1, FileName: "", New: true })
            },
            remove: function (index) {
                this.photos.splice(index, 1);
            }
        }
    });

    if ($("#Visita_Marcas").val() != null) {
        $("#Marcas").val(JSON.parse("[" + $("#Visita_Marcas").val() + "]"));
        $("#Marcas").change();
    }

    if (mod == "edit" || mod == "details" || mod == "delete") {
        photostag = $('#photos-thumb');

        photostag.lightGallery({
            thumbnail: true,
            selector: 'a'
        });
    }



    if (mod == "create" || mod == "edit") {

        $("#Visita_TipoVisitaId").rules("remove");
        $("#Visita_TipoVisitaId").rules("add", { required: true });

        $("#Visita_TipoIndustriaId").rules("remove");
        $("#Visita_TipoIndustriaId").rules("add", { required: true });

        $('#Visita_PaisId').each(function (index, item) {
            $(item).rules("remove");
            $(item).rules("add", { required: true });
        });

        $('#Visita_DepartamentoId').each(function (index, item) {
            $(item).rules("remove");
            $(item).rules("add", { required: true });
        });

        $('#Visita_CiudadId').each(function (index, item) {
            $(item).rules("remove");
            $(item).rules("add", { required: true });
        });


        $('#Visita_Fecha').bootstrapMaterialDatePicker(datePickerOpts).change(function () {
            $(this).valid();
            updateMaterialTextFields("#cardForm");
        });

        if ($("#Visita_Latitud").val() == "" && $("#Visita_Longitud").val() == "")
            GetLocation();
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=number]").prop("readonly", true);
        $("#cardForm input[type=datetime]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
        $("#cardForm textarea").prop("readonly", true);
    }
}



function _DDLocation(listas) {

    var splitReult = listas.split(";");
    var str1 = splitReult[0];
    var str2 = splitReult[1];
    var str3 = splitReult[2];
    //Variable para cargar el valor del país
    var idPais = "";

    //Si no se selecciona un país se deshabilitan los departamentos O
    //si no se selecciona un departamento se deshabilitan las ciudades
    if (str1 == "PaisID")
        str1 = "Visita_PaisId";
    else if (str1 == "DepartamentoID")
        str1 = "Visita_DepartamentoId";

    if ($("#" + str1).val() == '0' || $("#" + str1).val() == "") {
        document.getElementById(str2).disabled = true;
        return;
    }

    //Para deshabilitar el campo de Ciudad al elegir el país
    if (str1 == "Visita_PaisId" && $("#Visita_CiudadId").val() != "") {
        $("#Visita_CiudadId").html('');
        document.getElementById("Visita_CiudadId").disabled = true;
    }
    if (str3 != null) {
        idPais = $("#" + str3).val();
    }
    var itemSeleccionado = $("#" + str1).val();
    var ddlModel = $("#" + str2);

    if (str1 == "Visita_PaisId")
        str1 = "PaisID";
    else if (str1 == "Visita_DepartamentoId")
        str1 = "DepartamentoID";

    $.ajax({
        cache: false,
        type: "GET",
        url: "/Clientes/GetDropDownInCascada",
        data: { "id": itemSeleccionado, "tipo": str1, "idPais": idPais },
        success: function (data) {
            document.getElementById(str2).disabled = false;
            ddlModel.html('');
            ddlModel.append($("<option value=''>Seleccione una Opción</option>"));
            $.each(data, function (id, option) {

                ddlModel.append($('<option></option>').val(option.id).html(option.name));
            });
            if ($("#" + str2).val() == null) {
                document.getElementById(str2).disabled = true;
                ddlModel.append($("<option value='0'>Sin Items.</option>"));
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            Materialize.toast("Error al cargar los items!!", 5000);
        }
    });
};


function GetLocation() {
    sLoading();


    if (navigator.geolocation) {
        // Código de la aplicación
        navigator.geolocation.getCurrentPosition(function (objPosition) {
            var lon = objPosition.coords.longitude;
            var lat = objPosition.coords.latitude;

            $("#Visita_Latitud").val(lat.replace(".", ","));
            $("#Visita_Longitud").val(lon.replace(".", ","));

            msgSuccess("Geolocalización capturada");
            $("#locationico").html("my_location");

        }, function (objPositionError) {
            $("#locationico").html("location_disabled");
            switch (objPositionError.code) {
                case objPositionError.PERMISSION_DENIED:
                    msgError("No se ha permitido el acceso a la posición del usuario.");
                    break;
                case objPositionError.POSITION_UNAVAILABLE:
                    msgError("No se ha podido acceder a la información de su posición.");
                    break;
                case objPositionError.TIMEOUT:
                    msgError("El servicio ha tardado demasiado tiempo en responder.");
                    break;
                default:
                    msgError("Error desconocido.");
            }
        }, {
            maximumAge: 75000,
            timeout: 15000
        });
    }
    else {
        $("#locationico").html("location_disabled");
        // No hay soporte para la geolocalización: podemos desistir o utilizar algún método alternativo
        msgError("No hay soporte para la geolocalización: podemos desistir o utilizar algún método alternativo");
    }

    //$("#Visita_Latitud").val(("4.6938691").replace(".",","));
    //$("#Visita_Longitud").val(("-74.0758855").replace(".", ","));

    hLoading();
}


//function RemovePhoto(id) {
//    $("#" + id).remove();
//}