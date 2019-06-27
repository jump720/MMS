$.MMS.Periodos = function (mod, revisiones, fechaIni, fechaFin, revisionFinalFechaIni) {
    mod = mod.toLowerCase();

    var datePickerOpts = {
        format: 'YYYY-MM-DD',
        weekStart: 1,
        time: false
    };

    var vueTableId = "#tableRevisiones";
    var vueRevisionesTable = new Vue({
        el: vueTableId,
        data: {
            revisiones: revisiones,
            revisionesId: [],
        },
        mounted: function () {
            if (mod == "details" || mod == "delete")
                return;

            var vm = this;
            vm.remakeRules();
            vm.updateMaterialFields();

            for (var i in vm.revisiones) {
                $(`.fechaIni-${i}`).bootstrapMaterialDatePicker(datePickerOpts).change(function () {
                    var index = $(this).data("index");
                    vm.revisiones[index].FechaIni = $(this).val();
                    $(this).valid();
                });
                $(`.fechaFin-${i}`).bootstrapMaterialDatePicker(datePickerOpts).change(function () {
                    var index = $(this).data("index");
                    vm.revisiones[index].FechaFin = $(this).val();
                    $(this).valid();
                });
            }
            
        },
        methods: {
            remove: function (index) {
                var revisionId = this.revisiones[index].Id;
                if (revisionId != 0)
                    this.revisionesId.push(revisionId);

                this.revisiones.splice(index, 1);
                this.remakeRules();
            },
            add: function () {
                var vm = this;

                vm.revisiones.push({
                    Id: 0,
                    FechaIni: "",
                    FechaFin: "",
                    ActivoManual: false
                });

                setTimeout(function () {
                    var index = vm.revisiones.length - 1;

                    $(`.fechaIni-${index}`).bootstrapMaterialDatePicker(datePickerOpts).change(function () {
                        var i = $(this).data("index");
                        vm.revisiones[i].FechaIni = $(this).val();
                        $(this).valid();
                    });
                    $(`.fechaFin-${index}`).bootstrapMaterialDatePicker(datePickerOpts).change(function () {
                        var i = $(this).data("index");
                        vm.revisiones[i].FechaFin = $(this).val();
                        $(this).valid();
                    });

                    vm.remakeRules();
                    vm.updateMaterialFields();
                }, 100)
            },
            disableManual: function (index) {
                var vm = this;

                if (vm.revisiones[index].ActivoManual) {
                    for (var i in vm.revisiones) {
                        if (index == i || !vm.revisiones[i].ActivoManual)
                            continue;

                        vm.revisiones[i].ActivoManual = false;
                    }
                }
            },
            makeDate: function (selector) {
                var value = $(selector).val();
                if (!value)
                    return;

                return moment(value).add(1, "d").format("YYYY-MM-DD");
            },
            updateMaterialFields: function () {
                $.AdminBSB.input.activate(vueTableId);
                updateMaterialTextFields(vueTableId);
            },
            remakeRules: function () {
                var vm = this;

                for (var i in vm.revisiones) {
                    $(`.fechaIni-${i}, .fechaFin-${i}, .periodoRevisionNombre-${i}`).each(function (index, item) {
                        $(item).rules("remove");
                        $(item).rules("add", { required: true });
                    });

                    if (i == 0)
                        $(`.fechaIni-${i}`).rules("add", {
                            min: function () {
                                return vm.makeDate("#Periodo_FechaIni");
                            }
                        });
                    else
                        $(`.fechaIni-${i}`).rules("add", {
                            min: function (item) {
                                var index = $(item).data("index");
                                return vm.makeDate(`.fechaFin-${index - 1}`);
                            }
                        });

                    $(`.fechaFin-${i}`).rules("add", {
                        min: function (item) {
                            var index = $(item).data("index");
                            return vm.makeDate(`.fechaIni-${index}`);
                        }
                    });
                }

                $("#Periodo_RevisionFinalFechaIni").rules("remove", "min");

                if (vm.revisiones.length == 0)
                    $("#Periodo_RevisionFinalFechaIni").rules("add", {
                        min: function () {
                            return vm.makeDate("#Periodo_FechaIni");
                        }
                    });
                else
                    $("#Periodo_RevisionFinalFechaIni").rules("add", {
                        min: function () {
                            return vm.makeDate(`.fechaFin-${vm.revisiones.length - 1}`);
                        }
                    });
            }
        }
    });

    if (mod == "create" || mod == "edit" || mod == "delete")
        checkValidationSummaryErrors();

    if (mod == "edit" || mod == "details" || mod == "delete") {
        $("#Periodo_FechaIni").val(fechaIni);
        $("#Periodo_FechaFin").val(fechaFin);
        $("#Periodo_RevisionFinalFechaIni").val(revisionFinalFechaIni);
    }

    if (mod == "create" || mod == "edit") {
        $("#Periodo_FechaFin").rules("add", {
            min: function () {
                return vueRevisionesTable.makeDate("#Periodo_RevisionFinalFechaIni")
            }
        });

        $('#Periodo_FechaIni, #Periodo_RevisionFinalFechaIni, #Periodo_FechaFin').bootstrapMaterialDatePicker(datePickerOpts).change(function () {
            $(this).valid();
            updateMaterialTextFields("#cardForm");
            
        });

        $("#Periodo_Descripcion").focus();
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=datetime]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
    }
};