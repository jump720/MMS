$.MMS.AddComment = function (mod, files, docs) {
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
                // alert(index);
                //if (!$(`#PQRSRecordDocumentos_${idx}_TipoDocSoporteId`).data('select2')) {
                //    $(`#PQRSRecordDocumentos_${idx}_TipoDocSoporteId`).select2({
                //        placeholder: "Search Document Type",
                //        theme: "bootstrap"
                //    });
                //}
            },
            addNewDoc: function (event) {
                //this.docs.push({ NroDocumento: "jummmm", TipoDocSoporteId: "" })
                var idx = this.docs.push({ NroDocumento: "", TipoDocSoporteId: "" })
                idx = idx - 1;
            }
        },
        ready: function () {
            alert("ready");
        }
    });



    if (mod == "create") {
        // $("#TipoPaso").find("option").eq(0).remove();//Remueve primer option vacio
    } else if (mod == "delete") {
        $("#formAddComment input[type=text]").prop("readonly", true);
        $("#formAddComment input[type=checkbox]").prop("disabled", true);
        $("#formAddComment select").prop("disabled", true);
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


        $("#formAddComment").submit(function (e) {
            
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
                    if (result) {
                        hModal(getCurrentModalId());
                        msgSuccess(`Comment ${successMsg}.`);
                    }
                    else
                        msgError(`Error ${errorMsg} the comment.`);
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
        text: "You are going to save the comment, this can't be undone, are you sure you want to save it?",
        type: "warning",
        showCancelButton: true,
        closeOnConfirm: false,
        showLoaderOnConfirm: true,
    }, function () {
        $("#btnSave").click();
        swal.close();
    });
}
