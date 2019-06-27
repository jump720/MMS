$.MMS.AddAnswer = function (mod, files, docs, checks, conds) {
    mod = mod.toLowerCase();



    var vueFilesTable = new Vue({
        el: "#_ListFiles",
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

    var vueDocTable = new Vue({
        el: "#_tableDocs",
        data: {
            docs: docs
        },
        methods: {
            SetSelect2: function () {

                $(`.select_TipoDocSoporteId`).select2({
                    placeholder: "Search Document Type",
                    theme: "bootstrap"
                });
              
            },
            addNewDoc: function (event) {
                
                var idx = this.docs.push({ NroDocumento: "", TipoDocSoporteId: "" })
                idx = idx - 1;
            }
        },
        ready: function () {
            alert("ready");
        }
    });


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
           remakeRules: function () {
               
            }
        },
        ready: function () {
            
        }
    });

    var vueConditionTable = new Vue({
        el: "#_tableConditions",
        data: {
            conds: conds
        },
        mounted: function () {
            var vmc = this;
            vmc.remakeRules();

            for (var i in vmc.conds) {
                $(`.RespValorCondiciones-${i}`).change(function () {
                    var index = $(this).data("index");
                    var idx = $(this).data("idx");
                    vmc.conds[index].Condiciones[idx].RespValor = $(this).val();
                    $(this).valid();
                });

                //$(`.CondicionesTipoCondicion-${i}`).change(function () {
                //    var index = $(this).data("index");
                //    vmc.conds[index].TipoCondicion = $(this).val();
                //    $(this).valid();
                //});

                //$(`.SelectCondicionesValor-${i}`).change(function () {
                //    var index = $(this).data("index");
                //    vmc.conds[index].CondicionesValor = $(this).val();
                //    $(this).valid();
                //});
            }

        },
        methods: {
            remakeRules: function () {
                var vmc = this;
                for (var i in vmc.conds) {
                  

                    $(`.RespValorCondiciones-${i}`).inputmask("numeric", {
                        radixPoint: ",",
                        groupSeparator: ".",
                        digits: 0,
                        autoGroup: true,
                        autoUnmask: true
                    });

                    //$(`.CondicionesTipoCondicion-${i}`).select2({ placeholder: "Select", theme: "bootstrap" });
                    //$(`.SelectCondicionesValor-${i}`).select2({ placeholder: "Select", theme: "bootstrap" });
                }
            },
        }

    });


    if (mod == "create") {
        // $("#TipoPaso").find("option").eq(0).remove();//Remueve primer option vacio
    } else if (mod == "delete") {
        $("#formAddAnswer input[type=text]").prop("readonly", true);
        $("#formAddAnswer input[type=checkbox]").prop("disabled", true);
        $("#formAddAnswer select").prop("disabled", true);
    }

    if (mod == "create" || mod == "edit" || mod == "delete") {

        if (mod == "delete") {
            validate = false;
            successMsg = "deleted";
            errorMsg = "deleting";
        }
        else {
            validate = true;
            successMsg = "Added";
            errorMsg = "Adding";
        }


        $("#formAddAnswer").submit(function (e) {

            e.preventDefault();

            if (validate)
                if (!$(this).valid())
                    return;


            var flagSize = MaxSizeFile();
            if (!flagSize)
                return;

            sLoading();


            $.ajax({
                url: this.action,//"/Api/PQRS/AddComment" ,
                type: 'POST',
                beforeSend: function () { },
                success: function (result) {
                    if (result.Res) {
                        hModal(getCurrentModalId());
                        msgSuccess(`Answer ${successMsg}.`);
                    }
                    else {
                        msgError(`Error ${errorMsg} the Answer`);
                        msgError(result.Msg);
                    }
                },
                xhr: function () {  // Custom XMLHttpRequest
                    var myXhr = $.ajaxSettings.xhr();
                    if (myXhr.upload) { // Check if upload property exists
                        // Progress code if you want
                    }
                    return myXhr;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    msgError(errorThrown);
                },
                data: new FormData(this),
                cache: false,
                processData: false,
                contentType: false
            }).fail(function (jqXHR, textStatus, errorThrown) {
                msgError(errorThrown);
            })
                .always(function () {
                    hLoading();
                });



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
        msgError("The attachment size exceeds the allowable limit");
        result = false;
    }


    return result;
}


function SaveComment() {
    swal({
        title: "Confirmation",
        text: "You are going to save the answer, this can't be undone, are you sure you want to save it?",
        type: "warning",
        showCancelButton: true,
        closeOnConfirm: false,
        showLoaderOnConfirm: true,
    }, function () {
        $("#btnSave").click();
        swal.close();
    });
}
