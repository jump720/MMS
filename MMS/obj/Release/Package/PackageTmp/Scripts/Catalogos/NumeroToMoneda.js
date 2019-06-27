$(document).ready(function () {


    $('#tablePresupuesto')
        .on('page.dt', function () { formatoNumber(); })
        .on('order.dt', function () { formatoNumber(); })
        .on('search.dt', function () { formatoNumber(); })
        .dataTable({

        sDom: '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
        oLanguage: {
            sLengthMenu: "Mostrando _MENU_ ",
            sZeroRecords: "No se encontraron registros",
            sInfo: "Mostrando _START_ - _END_ de _TOTAL_ registros",
            sInfoEmpty: "Mostrando 0 - 0 de 0 registros",
            sInfoFiltered: "(Filtrando de _MAX_ registros totales)",
            oPaginate: { sPrevious: "Anterior", sNext: "Siguiente", sFirst: "Primera", sLast: "Ultima" },
            sEmptyTable: "No hay registros",
            sSearch: "Buscar:"
        }
    });

    $("select[name=tablePresupuesto_length]").val('10'); //seleccionar valor por defecto del select
    $('select[name=tablePresupuesto_length]').addClass("browser-default");

    var rows = $('.table-with-number tbody tr').length
    for (i = 1; i <= rows; i++) {
        $('#Presupuesto' + i).number(true, 0, ',', '.');
        $('#Gasto' + i).number(true, 0, ',', '.');
        //$('#Venta' + i).number(true, 0, ',', '.');
        $('#Autorizacion' + i).number(true, 0, ',', '.')
    }
});

function formatoNumber() {
    $("td[id*='Presupuesto']").number(true, 0, ',', '.');
    $("td[id*='Gasto']").number(true, 0, ',', '.');
    //$("td[id*='Venta']").number(true, 0, ',', '.');

    $("td[id*='Venta']").each(function () {
        if (parseInt(this.innerText) < 0) {
            $(this).html($.number(parseInt(this.innerText), 0, ',', '.'))
            //console.log($.number(parseInt(this.innerText), 0, ',', '.'));
            //console.log($(this).html())
        } else {
            $(this).number(true, 0, ',', '.');
        }

    })
    
}

/*<![CDATA[*/
function compararPresupuesto(s) {
    var presupuesto = parseInt($('#Presupuesto' + s).val());
    var gasto = parseInt($('#Gasto' + s).val());
    //alert(presupuesto + " " + gasto);
    if (presupuesto < gasto) {
        document.getElementById('Presupuesto' + s).className = "invalid";
        document.getElementById('Validacion' + s).innerHTML = "";
        var text = document.createTextNode("El Presupuesto debe ser mayor que el Gasto.");
        document.getElementById('Validacion' + s).appendChild(text);
        document.getElementById("btnGuardar").disabled = true;
    }
    else {
        document.getElementById('Presupuesto' + s).className = "valid";
        document.getElementById('Validacion' + s).innerHTML = "";
        document.getElementById("btnGuardar").disabled = false;
    }
}
/*]]>*/