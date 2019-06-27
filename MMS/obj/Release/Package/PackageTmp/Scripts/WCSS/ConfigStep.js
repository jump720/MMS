var vueConditionTable;
$.MMS.ConfigStep = function (mod, flujo, checks, conds) {
    mod = mod.toLowerCase();


    var vueCheckTable = new Vue({
        el: "#_tablechecks",
        data: {
            checks: checks
        },
        mounted: function () {
            var vm = this;
            vm.remakeRules();
        },
        methods: {
            remove: function (index) {

                this.checks.splice(index, 1);
            },
            addNewCheck: function (event) {
                var vm = this;
                var idx = this.checks.push({ Id: 0, MotivoPQRSId: flujo.MotivoPQRSId, FlujoPQRSId: flujo.Id, Descripcion: "", Requerido: true })
                idx = idx - 1;
                //alert("eyy");

                setTimeout(function () {

                    vm.remakeRules();
                }, 100);
            },
            remakeRules: function () {
                var vm = this;
                for (var i in vm.checks) {
                    $(`.TareasDescripcion-${i}`).each(function (index, item) {
                        $(item).rules("remove");
                        $(item).rules("add", { required: true });
                    });
                }
            }
        },
        ready: function () {
            alert("ready");
        }
    });

    vueConditionTable = new Vue({
        el: "#_tableConditions",
        data: {
            conds: conds
        },
        mounted: function () {
            var vmc = this;
            vmc.remakeRules();

            for (var i in vmc.conds) {
                $(`.ValorCondiciones-${i}`).change(function () {
                    var index = $(this).data("index");
                    vmc.conds[index].Valor = $(this).val();
                    $(this).valid();
                });

                $(`.CondicionesTipoCondicion-${i}`).change(function () {
                    var index = $(this).data("index");
                    vmc.conds[index].TipoCondicion = $(this).val();
                    $(this).valid();
                });

                $(`.SelectCondicionesValor-${i}`).change(function () {
                    var index = $(this).data("index");
                    vmc.conds[index].CondicionesValor = $(this).val();
                    $(this).valid();
                });
            }

        },
        methods: {
            remakeRules: function () {
                var vmc = this;
                for (var i in vmc.conds) {
                    $(`.CondicionesTipoCondicion-${i}, .CondicionesDescripcion-${i}`).each(function (index, item) {
                        $(item).rules("remove");
                        $(item).rules("add", { required: true });
                    });

                    $(`.ValorCondiciones-${i}`).inputmask("numeric", {
                        radixPoint: ",",
                        groupSeparator: ".",
                        digits: 0,
                        autoGroup: true,
                        autoUnmask: true
                    });

                    $(`.CondicionesTipoCondicion-${i}`).select2({ placeholder: "Select", theme: "bootstrap" });
                    $(`.SelectCondicionesValor-${i}`).select2({ placeholder: "Select", theme: "bootstrap" });

                    
                }
            },
            remove: function (index) {

                this.conds.splice(index, 1);
            },
            addNewCond: function (event) {
                var vmc = this;
                var idx = this.conds.push({ Id: 0, MotivoPQRSId: flujo.MotivoPQRSId, FlujoPQRSId: flujo.Id, Descripcion: "", TipoCondicion: "1", CondicionesValor: "1", Valor: 0, SiNo: true })
                idx = idx - 1;

                setTimeout(function () {

                    vmc.remakeRules();

                    var index = vmc.conds.length - 1;

                    $(`.ValorCondiciones-${index}`).change(function () {
                        var index = $(this).data("index");
                        vmc.conds[index].Valor = $(this).val();
                        $(this).valid();
                    });

                    $(`.CondicionesTipoCondicion-${index}`).change(function () {
                        var index = $(this).data("index");
                        vmc.conds[index].TipoCondicion = $(this).val();
                        $(this).valid();
                    });

                    $(`.SelectCondicionesValor-${index}`).change(function () {
                        var index = $(this).data("index");
                        vmc.conds[index].CondicionesValor = $(this).val();
                        $(this).valid();
                    });

                }, 100);
              
            }
        }
        
    });


  

    //tableUsuarios = $("#tableUsuarios").dataTable({
    //    scrollY: '55vh',
    //    scrollCollapse: true,
    //    searching: false,
    //    paging: false
    //});

    if (mod == "create") {
        //$("#TipoPaso").find("option").eq(0).remove();//Remueve primer option vacio
    } else if (mod == "delete") {
        //$("#formStep input[type=text]").prop("readonly", true);
        //$("#formStep input[type=checkbox]").prop("disabled", true);
        //$("#formStep select").prop("disabled", true);
    }

    if (mod == "create" || mod == "edit" || mod == "delete") {

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


        $("#formConfig").submit(function (e) {




            e.preventDefault();

            //if (validate)
                if (!$(this).valid())
                    return;

            //var cbSelected = $('input[name*=check]:checked').length;

            //if (cbSelected <= 0) {
            //    msgError(`Debe seleccionar al menos un usuario`);
            //    return;
            //}


            sLoading();
            if (mod == "delete") {
                //$.post(this.action + "?MotivoPQRSId=" + motivo + "&Id=" + id)
                //.done(function (result) {
                //    if (result) {
                //        hModal(getCurrentModalId());
                //        msgSuccess(`Record ${successMsg}.`);
                //    }
                //    else
                //        msgError(`Error ${errorMsg} the record.`);
                //})
                //.fail(function (jqXHR, textStatus, errorThrown) {
                //    msgError(errorThrown);
                //})
                //.always(function () {
                //    hLoading();
                //});
            } else {
                $.post(this.action, validate ? $(this).serialize() : null)
                    .done(function (result) {
                        if (result.Res) {
                            hModal(getCurrentModalId());
                            msgSuccess(`Record ${successMsg}.`);
                        }
                        else {
                            msgError(`Error ${errorMsg} the record.`);
                            msgError(result.Msg);
                        }
                    })
                    .fail(function (jqXHR, textStatus, errorThrown) {
                        msgError(errorThrown);
                    })
                    .always(function () {
                        hLoading();
                    });
            }
        });
    }
}

function selectCond() {
    //$('#formConfig select').select2({ placeholder: "Select", theme: "bootstrap" });


  

}