//$(document).ready(function () {
//    $('#ObjetosTable').DataTable({
//        "scrollY": "50vh",
//        "scrollCollapse": true,
//        "paging": false,
//        "bFilter": false,
//        "bSort": false,
//        oLanguage: {
//            sLengthMenu: "Mostrando _MENU_ ",
//            sZeroRecords: "No se encontraron registros",
//            sInfo: "Mostrando _START_ - _END_ de _TOTAL_ registros",
//            sInfoEmpty: "Mostrando 0 - 0 de 0 registros",
//            sInfoFiltered: "(Filtrando de _MAX_ registros totales)",
//            oPaginate: { sPrevious: "Anterior", sNext: "Siguiente", sFirst: "Primera", sLast: "Ultima" },
//            sEmptyTable: "No hay registros",
//            sSearch: "Buscar:"
//        }
//    });


//    $("#SelectAll").change(function () {

//        $("#ObjetosTable tbody tr td input:checkbox").prop('checked', $(this).prop("checked"));

//    });

//    $("#ObjetosTable tbody tr td input:checkbox").change(function () {
//        if ($(this).is(':checked')) {

//        } else {
//            $("#SelectAll").prop('checked', $(this).prop("checked"));
//        }        
//    });

//});


$.MMS.Roles = function (mod, Roles) {

    mod = mod.toLowerCase();
    checkValidationSummaryErrors();



    if (mod == "create" || mod == "edit") {
        $("#SelectAllObj").change(function () {

            $("#tableObjetos tbody tr td input:checkbox").prop('checked', $(this).prop("checked"));

        });

        $("#SelectAllApp").change(function () {

            $("#tableApps tbody tr td input:checkbox").prop('checked', $(this).prop("checked"));

        });
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=number]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }



}