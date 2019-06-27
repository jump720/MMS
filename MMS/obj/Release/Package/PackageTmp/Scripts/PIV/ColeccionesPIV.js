$.MMS.ColeccionesPIV = function (mod, clientes) {
    mod = mod.toLowerCase();
    clientes = clientes || [];

    var vueClientesTable = new Vue({
        el: "#tableClientes",
        data: {
            clientes: clientes
        },
        methods: {
            remove: function (index) {
                this.clientes.splice(index, 1);
            }
        }
    });

    if (mod == "create" || mod == "edit" || mod == "delete")
        checkValidationSummaryErrors();

    if (mod == "create" || mod == "edit") {
        $("#selectBuscarCliente").select2({
            placeholder: "Search Customer to add",
            theme: "bootstrap",
            ajax: {
                type: "GET",
                url: function (params) {
                    return "/api/ColeccionesPIV/BuscarCliente?q=" + encodeURIComponent(params.term)
                },
                delay: 300,
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    return {
                        results: $.map(data, function (cliente) {
                            if ($("#cardForm .codigo-cliente:contains('" + cliente.Id + "')").length > 0)
                                return null;

                            return {
                                text: cliente.RazonSocial,
                                id: cliente.Id,
                                data: cliente
                            }
                        }),
                        pagination: {
                            more: (params.page * 30) < data.total_count
                        }
                    };
                }
            },
            minimumInputLength: 1,
            templateResult: function (repo) {
                if (repo.loading) return repo.text;
                data = repo.data;

                return '<div class="select2-result-repository clearfix">' +
                            `<div><b>${data.Id} - ${data.RazonSocial}</b></div>` +
                            "<div>" +
                                `<small>Location: ${data.Pais} - ${data.Departamento} - ${data.Ciudad}</small><br>` +
                                `<small>Channel: ${data.Canal}</small>` +
                            "</div>" +
                        '</div>';
            },
            escapeMarkup: function (m) { return m; }
        });

        $("#selectBuscarCliente").on('select2:select', function (e) {
            if ($(this).select2('data').length == 0)
                return;

            var data = $(this).select2('data')[0].data;

            console.log(data);

            if (data.ColeccionPIV) {
                swal({
                    title: "Confirmation",
                    text: `This Customer is already related to another PIP Collection "${data.ColeccionPIV.Nombre}". ¿Are you sure you want to relate it to this PIP Collection?`,
                    type: "warning",
                    showCancelButton: true,
                    closeOnConfirm: true
                }, function () {
                    vueClientesTable.clientes.push(data);
                });
            }
            else
                vueClientesTable.clientes.push(data);

            $(this).val("").change();
        });
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
    }
};