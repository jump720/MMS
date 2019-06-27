$.MMS.ActividadCierre = function (mod,files) {
    mod = mod.toLowerCase();

    //$("#formResetPassword input[type=text]").prop("readonly", true);
    //$("#formResetPassword input[type=checkbox]").prop("disabled", true);
    var sliderRating;
    sliderRating = $("#nouislider_rating")[0];
    noUiSlider.create(sliderRating, {
        start: [1],
        connect: 'lower',
        step: 0.1,
        range: {
            'min': [1],
            'max': [100]
        }
    });
    sliderRating.noUiSlider.on('update', function () {
        var val = sliderRating.noUiSlider.get();
        $("#CumplimientoPorcentaje").val(parseInt(val));


        val += '%';
        $('span.js-nouislider-value').text(parseInt(val));
    });

    $('#CumplimientoTotal').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    $("#EstadoCierre").select2({
        placeholder: "Estado cierre",
        theme: "bootstrap",
        allowClear: true
    });

    $("#MetaCierre").select2({
        placeholder: "Comparar cumplimiento $ contra:",
        theme: "bootstrap",
        allowClear: true
    });

    $("#EstadoCierre").rules("add", { required: true });
    $("#MetaCierre").rules("add", { required: true });

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

    if (mod == "create" || mod == "edit" || mod == "delete") {


        validate = true;
        successMsg = "Actividad cerrada";
        errorMsg = "Cerrando actividad";



        $("#formActividadCierre").submit(function (e) {
            //e.preventDefault();

            if (validate)
                if (!$(this).valid()) {
                    e.preventDefault();
                    return;
                }



            sLoading();

            //$.post(this.action, validate ? $(this).serialize() : null)
            //    .done(function (result) {
            //        if (result.Res) {0
            //            hModal(getCurrentModalId());
            //            msgSuccess(`Success, ${successMsg}.`);
            //        }
            //        else
            //            msgError(`Error ${errorMsg}.`);
            //    })
            //    .fail(function (jqXHR, textStatus, errorThrown) {
            //        msgError(errorThrown);
            //    })
            //    .always(function () {
            //        hLoading();
            //    });

        });
    }

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