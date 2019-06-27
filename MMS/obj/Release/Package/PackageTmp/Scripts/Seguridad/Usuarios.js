//$(document).ready(function () {

//    $("#Usuarioactivo").click(function () {
//        if ($("#Usuarioactivo").is(':checked')) {
//            $("#Usuarioactivo").val("true");
//        } else {
//            $("#Usuarioactivo").val("false");
//        }
//    })

//    $("#UsuarioAprobadorPrincipal").click(function () {
//        if ($("#UsuarioAprobadorPrincipal").is(':checked')) {
//            $("#UsuarioAprobadorPrincipal").val("true");
//        } else {
//            $("#UsuarioAprobadorPrincipal").val("false");
//        }
//    })



//})

$.MMS.Usuarios = function (mod) {

    mod = mod.toLowerCase();
    checkValidationSummaryErrors();
    //$("#Usuario.UsuarioPadreId").select2({
    //    theme: "bootstrap",

    //});


    $("#Usuario_UsuarioPadreId").select2({
        placeholder: "Select manager",
        theme: "bootstrap",
        allowClear: true
    });

    $("#Usuario_UsuarioPadreId").attr('name', 'Usuario.UsuarioPadreId');

    $("#UsuarioHV_AreaId").attr('name', 'UsuarioHV.AreaId');
    $("#UsuarioHV_AreaId").select2({ theme: "bootstrap", });

    $("#Usuario_UsuarioAprobadorPrincipal").select2({
        placeholder: "Select If Primary Approver",
        theme: "bootstrap",
        allowClear: true
    });



    $('#Usuario_UsuarioMontoAut').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });


    if (mod == "edit") {
        $("#Usuario_UsuarioId").prop("readonly", true);
        $("#Usuario_Usuariopassword").prop("readonly", true);
    }


    if (mod == "create" || mod == "edit") {
        $("#UsuarioHV_AreaId").rules("add", {
            required: true,
            messages: {
                required: "The Area field is required."
            }
        });
        $("#UsuarioHV_Cargo").rules("add", {
            required: true,
            messages: {
                required: "The Title field is required."
            }
        });
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=number]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }



}


//function _ResetPassword(usuario, usuarioNombre) {



//    $.ajax({
//        url: '/usuarios/_ResetPassword',
//        data: { Usuario: usuario, UsuarioNombre: usuarioNombre },
//        contentType: 'application/html; charset=utf-8',
//        type: 'GET',
//        dataType: 'html'      

//    })
//    .success(function (result) {
//        $('#modales').html(result);
//        $('#_Modal').openModal();

//    })
//    .error(function (xhr, status) {

//        alert(xhr.responseText);
//    });

//    //Materialize.updateTextFields();

//}


//function _ResetPasswordSubmit() {

//    //var usuario = $("#Usuario").val();
//    //var usuarioNombre = $("#UsuarioNombre").val();;
//    //var password1 = $("#Password1").val();;
//    //var password2 = $("#Password2").val();;

//    $('#_Modal').closeModal();
//    $.post("/usuarios/_ResetPassword", $("form").serialize(), function () {
//        // alert("success");
//    })
//    .done(function (result) {
//        $('#modales').html(result);

//        $('#_Modal').openModal();
//    })
//    .fail(function () {
//        $('#_Modal').closeModal();
//        alert(status);
//    })
//    .always(function () {
//        //alert("finished");
//    });



//}

