var vueItemsTabl;
var DataSelectItem;
var DataSelectMotivo;
var DataSelectCausa;
var idx;
var templateItems;
var select2ItemsOptions;
var templateMotivo;
var select2MotivoOptions;
$.MMS.Recruitments = function (mod) {
    mod = mod.toLowerCase();

    $("#CentroCostoID").select2({
        placeholder: "Centro de Custo",
        theme: "bootstrap",
        allowClear: true
    });

    $("#CentroCostoID").attr('name', 'CentroCostoID');

    $("#DepartmentId").select2({
        placeholder: "Departamento",
        theme: "bootstrap",
        allowClear: true
    });

    $("#DepartmentId").attr('name', 'DepartmentId');

    $("#ProposedDepartmentId").select2({
        placeholder: "Departamento Proposto",
        theme: "bootstrap",
        allowClear: true
    });

    $("#ProposedDepartmentId").attr('name', 'ProposedDepartmentId');

    $("#ProposedCostCenterID").select2({
        placeholder: "Centro de Custo",
        theme: "bootstrap",
        allowClear: true
    });

    $("#ProposedCostCenterID").attr('name', 'ProposedCostCenterID');

    $("#Sector").select2({
        placeholder: "Setor",
        theme: "bootstrap",
        allowClear: true
    });

    $("#Sector").attr('name', 'Sector');

    $("#Position").select2({
        placeholder: "Posição",
        theme: "bootstrap",
        allowClear: true
    });

    $("#Position").attr('name', 'Position');

    $("#ContractType").select2({
        placeholder: "Tipo de Contrato",
        theme: "bootstrap",
        allowClear: true
    });

    $("#ContractType").attr('name', 'ContractType');

    $("#Budget").select2({
        placeholder: "Budget",
        theme: "bootstrap",
        allowClear: true
    });

    $("#Budget").attr('name', 'Budget');

    $("#ResignationReason").select2({
        placeholder: "Motivo",
        theme: "bootstrap",
        allowClear: true
    });

    $("#ResignationReason").attr('name', 'ResignationReason');

    $("#PreviousNotice").select2({
        placeholder: "Aviso Prévio",
        theme: "bootstrap",
        allowClear: true
    });

    $("#PreviousNotice").attr('name', 'PreviousNotice');

    $("#UsuarioIdSubstitute").select2({
        placeholder: "Usuário Substituido",
        theme: "bootstrap",
        allowClear: true
    });

    $("#UsuarioIdSubstitute").attr('name', 'UsuarioIdSubstitute');

    $("#ExperienceTime").select2({
        placeholder: "Tempo de Experiência",
        theme: "bootstrap",
        allowClear: true
    });

    $("#ExperienceTime").attr('name', 'ExperienceTime');

    $("#AreaManagerID").select2({
        placeholder: "Gerente",
        theme: "bootstrap",
        allowClear: true
    });

    $("#AreaManagerID").attr('name', 'AreaManagerID');

    $("#HumanResourcesID").select2({
        placeholder: "RH",
        theme: "bootstrap",
        allowClear: true
    });

    $("#HumanResourcesID").attr('name', 'HumanResourcesID');

    $("#ImmediateBossID").select2({
        placeholder: "Chefe",
        theme: "bootstrap",
        allowClear: true
    });

    $("#ImmediateBossID").attr('name', 'ImmediateBossID');

    var datePickerOpts = {
        format: 'YYYY-MM-DD',
        weekStart: 1,
        time: false
    };

    $("#_selectEstado").select2({
        placeholder: "Search Status",
        allowClear: true,
        theme: "bootstrap"
    });

    if (mod == "create" || mod == "edit") {

        //$('#StartDate').bootstrapMaterialDatePicker(datePickerOpts).change(function () {
        //    $(this).valid();
        //    updateMaterialTextFields("#cardForm");
        //});

        var estado = $("#Estado").val();

        if (estado == "" || estado == "Open") {

            $(".AdditionalInfo").css("display", "none");

            $("#_selectEstado").prop("disabled", true);
            $("#_DocSoporte").prop("readonly", true);
            $("#_CantidadRecibida").prop("readonly", true);
            $("#_CantidadSubida").prop("readonly", true);
            $("#_ComentarioEstadoMercancia").prop("readonly", true);
            $("#_PrecioAsumido").prop("readonly", true);
        } else {
            $("#btnAdd").hide();
            $("#btnSave").hide();
            //$(".btnEdit").hide();
            $(".btnDelete").hide();
            $(".btnAdd").hide();

            $("#cardForm input[type=text]").prop("readonly", true);
            $("#cardForm input[type=number]").prop("readonly", true);
            $("#cardForm input[type=checkbox]").prop("disabled", true);
            $("#cardForm select").prop("disabled", true);
            $("#cardForm textarea").prop("readonly", true);

            if (estado == "In_Process") {

                $("form").submit(function (e) {
                    $("#cardForm select").prop("disabled", false);
                })

                $("#_selectEstado").prop("disabled", false);
                $("#_DocSoporte").prop("readonly", false);
                $("#_CantidadRecibida").prop("readonly", false);
                $("#_CantidadSubida").prop("readonly", false);
                $("#_ComentarioEstadoMercancia").prop("readonly", false);
                $("#_PrecioAsumido").prop("readonly", false);
            }
        }
    }
    else if (mod == "details" || mod == "delete") {

        $("#btnAdd").hide();
        $("#btnSave").hide();
        $(".btnEdit").hide();
        $(".btnDelete").hide();
        $(".btnAdd").hide();

        $("#cardForm input[type=text]").prop("readonly", true);
        $("#cardForm input[type=number]").prop("readonly", true);
        $("#cardForm input[type=checkbox]").prop("disabled", true);
        $("#cardForm select").prop("disabled", true);
        $("#cardForm textarea").prop("readonly", true);

    }
}