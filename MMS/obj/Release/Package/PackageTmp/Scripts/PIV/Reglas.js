$.MMS.Reglas = function (mod, itemId) {
    mod = mod.toLowerCase();

    var template = function (data) {
        return '<div class="select2-result-repository clearfix">' +
                    `<div><b>${data.Codigo} - ${data.Descripcion}</b></div>` +
                    "<div>" +
                        `<small>Brand: ${data.Marca}</small><br>` +
                        `<small>Category: ${data.Categoria}</small><br>` +
                        `<small>Group: ${data.Grupo}</small>` +
                    "</div>" +
                '</div>';
    };

    var select2Options = {
        placeholder: "Search Item",
        theme: "bootstrap",
        ajax: {
            type: "GET",
            url: function (params) {
                return "/api/Plantillas/BuscarItem?q=" + encodeURIComponent(params.term)
            },
            delay: 300,
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.Descripcion,
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

            return template(repo.data);
        },
        templateSelection: function (repo) {
            if (!repo.id)
                return repo.text;

            return `(${repo.data.Marca}) ${repo.data.Codigo} - ${repo.data.Descripcion}`;
        },
        escapeMarkup: function (m) { return m; }
    };

    $("#Regla_ItemId").select2(select2Options);

    $("#Regla_MarcaId").select2({
        theme: "bootstrap",
        placeholder: "Select Brand"
    });

    $("#Tipo").change(function () {
        if ($(this).val() == 1) { // item
            $("#brandContainer").hide();
            $("#itemContainer").show();
        }
        else { // brand
            $("#brandContainer").show();
            $("#itemContainer").hide();
        }
    });

    $("#Tipo").select2({
        theme: "bootstrap",
        placeholder: "Select Type",
        minimumResultsForSearch: -1 // disable search
    }).change();

    $('#Regla_Meta').inputmask("numeric", {
        radixPoint: ",",
        groupSeparator: ".",
        digits: 0,
        autoGroup: true,
        autoUnmask: true
    });

    if (mod == "create" || mod == "edit") {
        $("#Regla_ItemId").rules("add", {
            required: true,
            messages: {
                required: "The Item field is required."
            }
        });

        $("#Regla_MarcaId").rules("add", {
            required: true,
            messages: {
                required: "The Brand field is required."
            }
        });
    }

    if (mod == "edit" || mod == "details" || mod == "delete") {
        if (itemId)
            $.get("/api/Plantillas/GetItem/" + itemId)
                .done(function (result) {
                    if (result) {
                        var items = [];
                        items.push({
                            text: result.Descripcion,
                            id: result.Id,
                            data: result
                        });

                        select2Options.data = items;
                        $("#Regla_ItemId").select2(select2Options);
                    }
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    msgError(errorThrown);
                });
    }

    if (mod == "create" || mod == "edit") {
        $("#Tipo").focus();
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
    }
};