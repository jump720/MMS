var vueStepsTable;

$.MMS.FlujosPQRS = function (mod, motivo, steps) {



    mod = mod.toLowerCase();
    $('[data-toggle="tooltip"]').tooltip();

    vueStepsTable = new Vue({
        el: '#steps',
        data: {
            list: steps
        },
        methods: {
            addNewStep: function (event, href) {



            },
            onUpdate: function (event) {

                //Organiza array VueJS
                this.list.splice(event.newIndex, 0, this.list.splice(event.oldIndex, 1)[0])

                ////Organiza(Actualiza) Base de datos
                var oldStep = this.list[event.oldIndex];
                var newStep = this.list[event.newIndex];

                //Organiza Datos array VueJS
                var order = 3;
                this.list.forEach(function (element) {
                    element.Order = order++;
                })


                SaveSteps("order", motivo, oldStep.Order, newStep.Order);
            }
        }
    });




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



function SaveSteps(mod, motivo, oldStep, newStep) {
    mod = mod.toLowerCase();

    if (mod == "create") {


        sLoading();
        $.post("/api/FlujosPQRS/SaveSteps?mod=" + mod + "&motivo=" + motivo)
        //$.post("/api/FlujosPQRS/SaveSteps", { mod : mod, motivo : motivo})
            .done(function (result) {
                if (result) {
                    //msgSuccess(`Steps Saved`);

                    vueStepsTable.list = [];
                    result.forEach(function (element) {
                        if (element.Id > 3) {
                            vueStepsTable.list.push({
                                MotivoPQRSId: element.MotivoPQRSId,
                                Id: element.Id,
                                Order: element.Order,
                                Nombre: element.Nombre,
                                TipoPaso: element.TipoPaso,
                                EnviaCorreoDestinatarios: element.EnviaCorreoDestinatarios
                            })
                        }// if (element.Id > 3){
                    });//result.forEach(function (element) {
                }
                else
                    msgError(`Error Steps the record.`);
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            })
            .always(function () {
                hLoading();
            });
    } else if (mod == "order") {
        sLoading();

        $.post("/api/FlujosPQRS/SaveSteps?mod=" + mod + "&motivo=" + motivo + "&OrderOld=" + oldStep + "&OrderNew=" + newStep)
        //$.post("/api/FlujosPQRS/SaveSteps", { mod : mod, motivo : motivo})
            .done(function (result) {
                if (result) {
                    msgSuccess(`Steps Saved`);
                }
                else
                    msgError(`Error Steps the record.`);
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            })
            .always(function () {
                hLoading();
            });
    }
}