$.MMS.ColeccionesPIVItem = function (mod, itemId) {
  
    mod = mod.toLowerCase();

    var template = function (data) {
        return '<div class="select2-result-repository clearfix">' +
                    `<div><b>${data.Codigo} - ${data.Descripcion}</b></div>` +
                    "<div>" +
                        `<small>Category: ${data.Categoria}</small><br>` +
                        `<small>Group: ${data.Grupo}</small><br>` +
                        `<small>$ ${data.PrecioSugerido}</small>` +
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

            return template(repo.data);
        },
        escapeMarkup: function (m) { return m; }
    };

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
                    $("#ItemId").select2(select2Options);
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            });
    else
        $("#ItemId").select2(select2Options);

    if (mod == "create" || mod == "edit" || mod == "delete") {
        var successMsg, errorMsg, validate;

        if (mod == "delete") {
            validate = false;
            successMsg = "deleted";
            errorMsg = "deleting";
        }
        else {
            validate = true;
            successMsg = "saved";
            errorMsg = "saving";
        }

        $("#frmColeccionPIVItem").submit(function (e) {
            e.preventDefault();

            if (validate)
                if (!$(this).valid())
                    return;

            sLoading();
            $.post(this.action, validate ? $(this).serialize() : null)
                .done(function (result) {
                    if (result) {
                        hModal(getCurrentModalId());
                        msgSuccess(`Record ${successMsg}.`);
                    }
                    else
                        msgError(`Error ${errorMsg} the record.`);
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    msgError(errorThrown);
                })
                .always(function () {
                    hLoading();
                });
        });
    }

    if (mod == "create" || mod == "edit") {
        $("#Codigo").focus();
    }
    else if (mod == "details" || mod == "delete") {
        $("#bodyForm input[type=text]").prop("readonly", true);
        $("#bodyForm select").prop("disabled", true);
    }
};