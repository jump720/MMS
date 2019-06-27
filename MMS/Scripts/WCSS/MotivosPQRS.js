$.MMS.MotivosPQRS = function (mod) {



    mod = mod.toLowerCase();

    $("#TipoPQRS").select2({
        placeholder: "Search PQRS",
        theme: "bootstrap"
    });

    if (mod == "create") {
        var templateMotivo = function (data) {

            return '<div class="select2-result-repository clearfix" style="width: 100%">' +
                `<div><b>${data.Nombre}</b></div>` +
                '</div>';
        };


        var select2MotivoOptions = {
            placeholder: "Search Reason",
            theme: "bootstrap",
            data: [],
            ajax: {
                type: "GET",
                url: function (params) {
                    var tipoPQRS = $("#TipoPQRS").val();
                    return "/api/MotivosPQRS/BuscarMotivoPQRS?q=" + encodeURIComponent(params.term) + "&t=" + tipoPQRS
                },
                delay: 300,
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    return {
                        results: $.map(data, function (item) {
                            return {
                                text: item.Nombre,
                                id: item.Id,
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

                return templateMotivo(repo.data);
            },
            templateSelection: function (repo) {
                if (!repo.id)
                    return repo.text;

                return templateMotivo(repo.data);
            },
            escapeMarkup: function (m) { return m; }
        };

        //$("#_selectMotivoPQRSId").select2(select2MotivoOptions);
        $("#MotivoPQRSCopy").select2(select2MotivoOptions);

      

        $("#TipoPQRS").change(function () {
            $("#MotivoPQRSCopy").val("").change();
        });
    }

    if (mod == "create" || mod == "edit" || mod == "delete")
        checkValidationSummaryErrors();


    if (mod == "create" || mod == "edit") {

    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }
}