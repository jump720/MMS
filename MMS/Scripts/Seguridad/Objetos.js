//$(document).ready(function () {

//    $("#ObjetoMenu").change(function () {
//        if ($("#ObjetoMenu").is(':checked')) {
//            $("#ObjetoMenu").val("true");
//            $(".is-objeto-menu").show();
//        }
//        else {
//            $("#ObjetoMenu").val("false");
//            $(".is-objeto-menu").hide();
//        }
//    })

//    $("#ObjetoMenu").change();

//});


$.MMS.Objetos = function (mod, objetos) {

    mod = mod.toLowerCase();
    checkValidationSummaryErrors();

    $("#Objeto_ObjetoIdPadre").attr('name', 'Objeto.ObjetoIdPadre');
    $("#Objeto_ObjetoIdPadre").select2({ theme: "bootstrap", });


    $("#Objeto_ObjetoMenu").change(function () {
        if ($("#Objeto_ObjetoMenu").is(':checked')) {
            $("#Objeto_ObjetoMenu").val("true");
            $(".is-objeto-menu").show();
        }
        else {
            $("#Objeto_ObjetoMenu").val("false");
            $(".is-objeto-menu").hide();
        }
    })

    $("#Objeto_ObjetoMenu").change();

    if (mod == "edit") {        
        $("#Objeto_ObjetoId").prop("readonly", true);
    }
    

    if (mod == "create" || mod == "edit") {

    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=number]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }



}