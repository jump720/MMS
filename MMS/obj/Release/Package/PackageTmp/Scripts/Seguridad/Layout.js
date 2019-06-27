$(document).ready(function () {
    $(".dropdown-button").dropdown();
    $(".button-collapse").sideNav();


})



function actualizarSeguridad() {
    $('#Loading').openModal();
    $.ajax({
        url: '../usuarios/actualizaSeguridad',
        data: {},
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'json'
    })
    .success(function (result) {
        $('#Loading').closeModal();
        if (result) {
            Materialize.toast("Se refresco la seguridad correctamente", 4000)
        } else {
            Materialize.toast("Sesion del usuario expiró", 4000)
        }
    })
    .error(function (xhr, status) {
        Materialize.toast(status, 4000)
        $('#Loading').closeModal();
    });
}

window.onload = function cargarMenu() {

    $.ajax({
        cache: false,
        type: "GET",
        url: "/Objetos/obtenerObjetosConMenu",
        success: function (data) {
            //alert(data);
            pathname = location.pathname;
            pathname2 = pathname.split("/");
            ctrlUrl = pathname2[1].toLowerCase();
            $.each(data, function (id, option) {
                //console.log("option.objeto " + option.objeto.toLowerCase() + " location: " + location.pathname.toLowerCase().replace("/", ""));
                //console.log("ctrlUrl " + ctrlUrl);
                str = option.objeto.toLowerCase().replace(" ", "").split("/");
                ctrID = str[0];
                if (ctrID != null ) {
                    $("#" + ctrID).show();
                }
                if (ctrID == ctrlUrl) {
                    $('#' + ctrID).addClass('active');
                }

                if (option.objeto.toLowerCase() == ("Actividades/Approve").toLowerCase() ||
                        option.objeto.toLowerCase() == ("movimientos/ConsultaInventario").toLowerCase()) {
                    $("#" + option.objeto.replace("/", "")).show();
                    
                }
                if (ctrID == ("visitas").toLowerCase()) {
                    $("#visitasMaps").show();
                }
            });
            if (ctrlUrl == 'auditoria' || ctrlUrl == 'objetos' || ctrlUrl == 'roles' || ctrlUrl == 'usuarios') {
                $("#boldConfig a").click();
            }
            else if (ctrlUrl == 'actividades' || ctrlUrl == 'movimientos' || ctrlUrl == 'ordenes' || ctrlUrl == 'movimientos/ConsultaInventario' || ctrlUrl == 'gastos') {
                $("#boldTrans a").click();
            }
            else {
                $('#boldCatalogos a').click();
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert('Error al cargar los objetos!!');
        }
    });
}