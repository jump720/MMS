function _DeleteCatalogo(ids, nomControl) {
    var str1 = "/";
    var str2 = nomControl;
    var str3 = "/Delete"
    var url1 = str1.concat(str2, str3);
    var splitReult = ids.split(";");
    $.ajax({
        url: url1,
        data: { ids: splitReult },
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html',
        traditional: true,
        success: function (result) {
            $('#modales').html(result);
            //$('#_Delete' + nomControl).openModal();
            $('#_Modal').openModal();
        },
        error: function (xhr, status) {
            alert(status);
        }
    });
};

function _ApproveActividad(ids, nomControl) {
    var str1 = "/";
    var str2 = nomControl;
    var str3 = "/ConfirmarApprove"
    var url1 = str1.concat(str2, str3);
    var splitReult = ids.split(";");
    $.ajax({
        url: url1,
        data: { ids: splitReult },
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html',
        traditional: true,
        success: function (result) {
            $('#modales').html(result);
            $('#_Modal').openModal();
        },
        error: function (xhr, status) {
            alert(status);
        }
    });
}