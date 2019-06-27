$.MMS.NivelesAprobacion = function (mod, usuarioId) {
    mod = mod.toLowerCase();

    checkValidationSummaryErrors();

    $("#CanalID, #PlantaID, #UsuarioId").select2({ theme: "bootstrap", });

    var $plantaId = $("#PlantaID");
    var $usuarioId = $("#UsuarioId");
    var ft = true;

    $plantaId.change(() => {
        $usuarioId.html('').append(`<option value=''>-- Usuario --</option>`)

        if (!$plantaId.val()) return;
        $.get('/api/Usuarios/PorPlanta?id=' + $plantaId.val()).done(res => {
            res.forEach(item => {
                $usuarioId.append(`<option value="${item.UsuarioId}">${item.UsuarioNombre}</option>`);
            });
            if (ft && usuarioId) $usuarioId.val(usuarioId);
            ft = false;
        })
    });

    $plantaId.change();

    if (mod == "create" || mod == "edit") {
        $("#Descripcion").focus();
    }
    else if (mod == "details" || mod == "delete") {
        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=number]").prop("readonly", true);
        $("#cardForm select").prop("disabled", true);
    }
};