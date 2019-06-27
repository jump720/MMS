$.MMS.Peak = function (paramId, paramTipo) {
    sLoading();
    Vue.use(VTooltip);

    var sliderCompletado, sliderRating;

    Vue.component("objetivo-info", {
        template: '#componentObjetivoInfo',
        props: {
            objetivo: { type: Object },
            readonly: { type: Boolean },
            tipo: { type: Number, default: 0 },
        },
        methods: {
            removeInheritedReference: function () {
                this.objetivo.PeakObjetivoId = null;
            },
        }
    });

    var vuePeak = new Vue({
        el: "#peakContent",
        data: {
            tipo: paramTipo,
            tabId: 1,
            completado: 0,
            calificacion: 0,
            numeroObjetivos: 0,
            peak: {
                Id: 0,
                Estado: 0,
                FechaEnvio: ""
            },
            peakTemp: {},
            objetivos: [],
            coreValues: [],
            planesDesarrollo: [],
            skills: [],
            periodo: {
                Id: 0,
                Descripcion: "",
                FechaIni: "",
                RevisionFinalFechaIni: "",
                FechaFin: "",
                Estado: 0,
                Actual: false,
                RevisionFinal: false,
            },
            revisiones: [],
            usuario: {
                Id: 0,
                Nombre: "",
                Cargo: "",
                Area: null,
                UsuarioPadre: null
            },
            areas: [],
            confirmacion: {
                areaId: null,
                cargo: null,
                usuarioPadre: null
            },
            cvRef: {},
            cvSeleccionado: {
                CoreValue: {}
            },
            planDesarrollo: {},
            objetivo: {
                Id: 0,
                Numero: 0,
                Peso: "",
                Heredable: false,
                Objetivo: "",
                FechaMeta: "",
                MedidoPor: "",
                Estado: 0,
                ComentariosRechazo: "",
                Completado: 0,
                ResultadosActuales: "",
                Comentarios: "",
                MotivoModificacion: "",
                AprobacionModificacion: null,
                PeakObjetivoId: null,
                SolicitudEliminacionHeredado: null
            },
            aprobacion: {
                estado: 1,
                comentariosRechazo: ""
            },
            avance: {
                completado: 0,
                resultadosActuales: "",
                comentarios: ""
            },
            modificacion: {
                motivoModificacion: ""
            },
            heredar: {
                objetivos: [],
            },
            revisionSeleccionada: {
                PeriodoRevision: {
                    Id: 0
                },
                Actual: false,
                Comentarios: "",
                ComentariosJefe: "",
                Cerrada: false,
            },
            revision: {
                comentarios: "",
                comentariosJefe: ""
            }
        },
        computed: {
            calificacionPorcentual: function () {
                return Math.round((100 * this.calificacion) / 1.5);
            },
            estadoPeriodo: function () {
                switch (this.periodo.Estado) {
                    case -2:
                        return "Not Started";
                    case -1:
                        return "Finished";
                    case 1:
                        return "Standby";
                    case 2:
                        return "Review";
                    case 3:
                        return "Final Review";
                    default:
                        return "";
                }
            },
            estadoPeak: function () {
                switch (this.peak.Estado) {
                    case 0:
                        return "Not Created";
                    case 1:
                        return "Objectives Definition";
                    case 2:
                        return "Objectives Approval";
                    case 3:
                        return "Standby";
                    case 4:
                        return "Review";
                    case 5:
                        return "Final Review";
                    case 6:
                        return "Finished";
                    case 7:
                        return "Objectives Modification Approval";
                    case 8:
                        return "Development Plan";
                    default:
                        return "";
                }
            },
        },
        mounted: function () {
            var vm = this;
            $.get(`/api/Peak/Get/${paramId}?tipo=${paramTipo}`)
                .done(function (result) {
                    if (result.Peak.FechaEnvio)
                        result.Peak.FechaEnvio = moment(result.Peak.FechaEnvio).format("YYYY-MM-DD HH:mm");

                    vm.numeroObjetivos = result.NumeroObjetivos;
                    vm.peak = result.Peak;
                    vm.periodo = result.Periodo;
                    vm.usuario = result.Usuario;
                    vm.areas = result.Areas;
                    vm.revisiones = result.Revisiones;
                    vm.coreValues = result.CoreValues;
                    vm.skills = result.Skills;
                    vm.planesDesarrollo = result.PlanesDesarrollo;
                    vm.asignarObjetivos(result.Objetivos);

                    vm.recalcularCompletado();
                    vm.recalcularCalificacion();
                    hLoading();

                    $('#frmObjetivo [name=FechaMeta], #frmModificacionObjetivo [name=FechaMeta]').bootstrapMaterialDatePicker({
                        format: 'YYYY-MM-DD',
                        weekStart: 1,
                        time: false,
                        minDate: vm.periodo.FechaIni,
                        maxDate: vm.periodo.FechaFin
                    }).change(function () {
                        vm.objetivo.FechaMeta = $(this).val();
                        $(this).valid();
                    });

                    $('#frmPlanDesarrollo [name=FechaMeta]').bootstrapMaterialDatePicker({
                        format: 'YYYY-MM-DD',
                        weekStart: 1,
                        time: false,
                    }).change(function () {
                        vm.planDesarrollo.FechaMeta = $(this).val();
                        $(this).valid();
                    });
                })
                .fail(handleError);
        },
        watch: {},
        methods: {
            finishFinalReview: function () {
                var vm = this;
                var errors = [];

                var count = 0;
                for (var i in vm.objetivos)
                    if (!vm.objetivos[i].Calificacion == null || !vm.objetivos[i].ComentariosJefe)
                        count++;

                if (count > 0)
                    errors.push("All Objectives must be rated.");

                count = 0;
                for (var i in vm.coreValues)
                    if (!vm.coreValues[i].Evaluacion)
                        count++;

                if (count > 0)
                    errors.push("All Core Values must have Manager's Assessment.");

                if (!vm.peak.ComentariosCompetencias)
                    errors.push("Manager's Comments - Competencies required.");

                if (!vm.peak.ResumenContribucionesJefe || !vm.peak.FortalezasJefe || !vm.peak.ObjetivosFuturoJefe)
                    errors.push("Manager Feedback required.");

                if (!vm.peak.RendimientoGeneral)
                    errors.push("Overall Performance Rating required.");

                if (errors.length > 0) {
                    var err = "";
                    for (var i in errors)
                        err += `<li>${errors[i]}</li>`;

                    msgAlert(`<ul>${err}</ul>`);
                    return;
                }

                swal({
                    title: "Finish Final Review",
                    text: "This Final Review will be finished.<br>After this action you will not be able to modify it.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Finish Final Review",
                }, function () {
                    $.post(`/api/Peak/FinalizarRevisionFinal/${vm.peak.Id}`)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("Final Review Finished. Development Plan enabled");
                            }
                            else
                                msgError("Error finishing Final Review.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            openReview: function () {
                var vm = this;
                var count = 0;

                swal({
                    title: "Open Review",
                    text: "If you Open this Review, the Associate will be able to send this Review to you again.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Open",
                }, function () {
                    $.post(`/api/Peak/AbrirRevision/${vm.peak.Id}?periodoRevisionId=${vm.revisionSeleccionada.PeriodoRevision.Id}`)
                        .done(function (result) {
                            if (result) {
                                vm.revisionSeleccionada.Cerrada = false;
                                msgSuccess("Review opened.");
                            }
                            else
                                msgError("Error opening the Review.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            finish: function () {
                var vm = this;

                if (vm.planesDesarrollo.length == 0) {
                    msgAlert("There must be at least one Development Plan");
                    return;
                }

                swal({
                    title: "Finish Peak",
                    text: "This Peak will be finished.<br>After this action you will not be able to modify it.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Finish",
                }, function () {
                    $.post(`/api/Peak/Finalizar/${vm.peak.Id}`)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("Peak Finished.");
                            }
                            else
                                msgError("Error Finishing Peak.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            eliminarPlanDesarrollo: function (index) {
                var vm = this;

                swal({
                    title: "Delete Development Plan",
                    text: "This Development will be deleted",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonColor: "#f44336",
                    confirmButtonText: "Delete",
                }, function () {
                    $.post(`/api/Peak/EliminarPlanDesarrollo/${vm.planesDesarrollo[index].Id}`)
                        .done(function (result) {
                            if (result) {
                                vm.planesDesarrollo.splice(index, 1);
                                msgSuccess("Develpment Plan Removed.");
                            }
                            else
                                msgError("Error deleting Develpment Plan.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            submitPlanDesarrollo: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/PlanDesarrollo/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            if (vm.planDesarrollo.Id == 0) {
                                var planDesarrollo = jQuery.extend(true, {}, vm.planDesarrollo);

                                planDesarrollo.Id = result.Id;
                                vm.planesDesarrollo.push(planDesarrollo);
                            }
                            else {
                                for (var i in vm.planesDesarrollo)
                                    if (vm.planesDesarrollo[i].Id == vm.planDesarrollo.Id)
                                        vm.planesDesarrollo.splice(i, 1, jQuery.extend(true, {}, vm.planDesarrollo));
                            }

                            msgSuccess("Development Plan Saved.");
                            $("#frmPlanDesarrollo .modal").modal("hide");
                        }
                        else
                            msgError("Error saving Development Plan.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalPlanDesarrollo: function (index) {
                var vm = this;

                if (index == -1) {
                    vm.planDesarrollo.Id = 0;
                    vm.planDesarrollo.Area = "";
                    vm.planDesarrollo.Plan = "";
                    vm.planDesarrollo.FechaMeta = "";
                    vm.planDesarrollo.ResultadoDeseado = "";
                }
                else
                    vm.planDesarrollo = jQuery.extend(true, {}, vm.planesDesarrollo[index]);

                vm.modal("#frmPlanDesarrollo .modal", function () {
                    $("#frmPlanDesarrollo").find("[name=Area]").focus();
                });
                $.MMS.validateForm("#frmPlanDesarrollo");
            },
            submitRendimientoGeneral: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/RendimientoGeneral/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            vm.peak.RendimientoGeneral = vm.peakTemp.RendimientoGeneral;

                            msgSuccess("Peak Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Peak.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalRendimientoGeneral: function () {
                var vm = this;
                vm.peakTemp = jQuery.extend(true, {}, vm.peak);

                vm.modal("#frmRendimientoGeneral .modal", function () {
                    $("#frmRendimientoGeneral").find("[name=RendimientoGeneral]").focus();
                });
                $.MMS.validateForm("#frmRendimientoGeneral");
            },
            submitFactorAjuste: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/FactorAjuste/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            vm.peak.FactorAjuste = result.FactorAjuste;
                            vm.peak.JustificacionFactorAjuste = vm.peakTemp.JustificacionFactorAjuste;
                            vm.recalcularCalificacion();

                            msgSuccess("Peak Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Peak.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalFactorAjuste: function () {
                var vm = this;
                vm.peakTemp = jQuery.extend(true, {}, vm.peak);

                vm.modal("#frmFactorAjuste .modal", function () {
                    $("#frmFactorAjuste").find("[name=FactorAjuste]").focus();
                });
                $.MMS.validateForm("#frmFactorAjuste");

                setTimeout(function () {
                    $("#frmFactorAjuste").find("[name=JustificacionFactorAjuste]")
                    .rules("add", {
                        required: function () {
                            var $factorAjuste = $("#frmFactorAjuste").find("[name=FactorAjuste]");
                            if (!$factorAjuste.valid())
                                return false;

                            return parseFloat($factorAjuste.val().replace(',', '.')) > 0;
                        }
                    });
                }, 150)
            },
            submitCalificacionObjetivo: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/CalificacionObjetivo/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            var index = vm.objetivo.Numero - 1;

                            vm.objetivos[index].Calificacion = result.Calificacion;
                            vm.objetivos[index].Factor = result.Factor;
                            vm.objetivos[index].ComentariosJefe = vm.objetivo.ComentariosJefe;
                            vm.recalcularCalificacion();

                            msgSuccess("Objective Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Objective.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalCalificacionObjetivo: function (index) {
                var vm = this;
                vm.objetivo = jQuery.extend(true, {}, vm.objetivos[index]);

                if (vm.objetivo.Calificacion == null) {
                    vm.objetivo.Calificacion = 0;
                    vm.objetivo.Factor = 0;
                }

                vm.modal("#frmCalificacionObjetivo .modal", function () {
                    if (!sliderRating) {
                        sliderRating = $("#nouislider_rating")[0];
                        noUiSlider.create(sliderRating, {
                            start: [vm.objetivo.Calificacion],
                            connect: 'lower',
                            step: 0.1,
                            range: {
                                'min': [0],
                                'max': [1.5]
                            }
                        });
                        sliderRating.noUiSlider.on('update', function () {
                            var val = sliderRating.noUiSlider.get();
                            vm.objetivo.Calificacion = parseFloat(val);
                            vm.objetivo.Factor = (vm.objetivo.Calificacion * vm.objetivo.Peso) / 100;
                        });
                    }
                    else
                        sliderRating.noUiSlider.set(vm.objetivo.Calificacion);

                    if (vm.tipo == 'manage' || !vm.periodo.RevisionFinal || (vm.tipo == 'review' && vm.peak.Estado != 5))
                        sliderRating.setAttribute('disabled', true);
                    else
                        sliderRating.removeAttribute('disabled');

                    $("#frmCalificacionObjetivo").find("[name=ComentariosJefe]").focus();
                });
                $.MMS.validateForm("#frmCalificacionObjetivo");
            },
            undoSendForFinalReview: function () {
                var vm = this;

                swal({
                    title: "Undo Send for Final Review",
                    text: "The Final Review information will be able to modify it.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                }, function () {
                    $.post(`/api/Peak/CancelarRevisionFinal/${vm.peak.Id}`)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("Undo done.");
                            }
                            else
                                msgError("Error undoing the action.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            sendForFinalReview: function () {
                var vm = this;

                for (var i in vm.objetivos)
                    if (vm.objetivos[i].Estado != 2) { // aprobado
                        msgAlert("All Objectives must be Approved");
                        return;
                    }

                for (var i in vm.coreValues)
                    if (!vm.coreValues[i].Autoevaluacion) {
                        msgAlert("All Core Values must have Self Assessment.");
                        return;
                    }

                swal({
                    title: "Send for Final Review",
                    text: "The Core Values and the Assessment will be sent for Final Review.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Send",
                }, function () {
                    $.post(`/api/Peak/EnvioRevisionFinal/${vm.peak.Id}`)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;

                                msgSuccess("Peak Sent.");
                            }
                            else
                                msgError("Error Peak.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            submitAutoevaluacion: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/AssesmentFeedback/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            vm.peak.ResumenContribuciones = vm.peakTemp.ResumenContribuciones;
                            vm.peak.ResumenContribucionesJefe = vm.peakTemp.ResumenContribucionesJefe;
                            vm.peak.Fortalezas = vm.peakTemp.Fortalezas;
                            vm.peak.FortalezasJefe = vm.peakTemp.FortalezasJefe;
                            vm.peak.ObjetivosFuturo = vm.peakTemp.ObjetivosFuturo;
                            vm.peak.ObjetivosFuturoJefe = vm.peakTemp.ObjetivosFuturoJefe;

                            msgSuccess("Peak Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Peak.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalAssessmentFeedback: function (fieldName) {
                var vm = this;
                vm.peakTemp = jQuery.extend(true, {}, vm.peak);

                vm.modal("#frmAutoevaluacion .modal", function () {
                    $("#frmAutoevaluacion").find(`[name=${fieldName}]`).focus();
                });
                $.MMS.validateForm("#frmAutoevaluacion");
            },
            submitCoreValuesComentarios: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/ComentariosCompetencias/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            vm.peak.ComentariosCompetencias = vm.peakTemp.ComentariosCompetencias;

                            msgSuccess("Peak Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Peak.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalCoreValuesComments: function () {
                var vm = this;
                vm.peakTemp = jQuery.extend(true, {}, vm.peak);

                vm.modal("#frmCoreValuesComentarios .modal", function () {
                    $("#frmCoreValuesComentarios").find("[name=ComentariosCompetencias]").focus();
                });
                $.MMS.validateForm("#frmCoreValuesComentarios");
            },
            submitCoreValue: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/" + (vm.tipo == 'manage' ? 'CoreValueAutoevaluacion' : 'CoreValueEvaluacion'), $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            vm.cvRef.Autoevaluacion = vm.cvSeleccionado.Autoevaluacion;
                            vm.cvRef.Evaluacion = vm.cvSeleccionado.Evaluacion;

                            msgSuccess("Core Value Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Core Value.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalCoreValue: function (index) {
                var vm = this;

                vm.cvSeleccionado = jQuery.extend(true, {}, vm.coreValues[index]);
                vm.cvRef = vm.coreValues[index];

                vm.modal("#frmCoreValue .modal");
                $.MMS.validateForm("#frmCoreValue");
            },
            sendReviewResults: function () {
                var vm = this;
                var count = 0;

                for (var i in vm.revisionSeleccionada.Objetivos)
                    if (!vm.revisionSeleccionada.Objetivos[i].ComentariosJefe)
                        count++;

                if (count > 0) {
                    msgAlert("All Objectives must have Manager's Comments");
                    return;
                }

                var fnSend = function (close) {
                    sLoading();
                    $.post(`/api/Peak/EnvioResultadoRevision/${vm.peak.Id}?periodoRevisionId=${vm.revisionSeleccionada.PeriodoRevision.Id}&cerrar=${close.toString()}`)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                vm.revisionSeleccionada.Cerrada = close;

                                for (var i in vm.revisionSeleccionada.Objetivos)
                                    vm.revisionSeleccionada.Objetivos[i].TieneCambios = false;

                                msgSuccess("The Review Result were sent.");
                            }
                            else
                                msgError("Error sending Review Result.");
                        })
                        .fail(handleError)
                        .always(hLoading);
                };

                swalExtend({
                    swalFunction: function () {
                        swal({
                            title: "Send Review Result",
                            text: "The Review Result will be send to the Associate.<br> If you Close this Review, the Associate will not be able to send this Review to you again.",
                            type: "warning",
                            html: true,
                            showCancelButton: true,
                            confirmButtonText: "Send and Close",
                        }, function () {
                            fnSend(true);
                        });
                    },
                    buttonNames: ["Send"],
                    clickFunctionList: [
                        function () {
                            fnSend(false);
                        },
                    ]
                });
            },
            submitComentariosObjetivo: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/ComentariosObjetivoRevision/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            var index = vm.objetivo.Numero - 1;
                            vm.revisionSeleccionada.Objetivos[index].ComentariosJefe = vm.objetivo.ComentariosJefe;

                            msgSuccess("Objective Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Objective.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalComentariosObjetivo: function (index) {
                var vm = this;

                vm.objetivo = jQuery.extend(true, {}, vm.revisionSeleccionada.Objetivos[index]);

                vm.modal("#frmComentariosObjetivo .modal", function () {
                    $("#frmComentariosObjetivo").find("[name=ComentariosJefe]").focus();
                });
                $.MMS.validateForm("#frmComentariosObjetivo");
            },
            undoSendForReview: function () {
                var vm = this;

                swal({
                    title: "Undo Send for Review",
                    text: "The Review Comments will be able to modify them.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                }, function () {
                    $.post(`/api/Peak/CancelarRevision/${vm.peak.Id}?periodoRevisionId=${vm.revisionSeleccionada.PeriodoRevision.Id}`)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("Undo done.");
                            }
                            else
                                msgError("Error undoing the action.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            sendForReview: function () {
                var vm = this;

                var count = 0, countRA = 0;
                for (var i in vm.objetivos) {
                    if (vm.objetivos[i].Estado != 2) // aprobado
                        count++;

                    if (!vm.objetivos[i].ResultadosActuales)
                        countRA++;
                }

                if (count > 0) {
                    msgAlert("All Objectives must be Approved");
                    return;
                }
                else if (countRA > 0) {
                    msgAlert("All Objectives must have Actual Results");
                    return;
                }

                swal({
                    title: "Send for Review",
                    text: "The Objetives and the Review Comments will be sent for Review.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Send",
                }, function () {
                    $.post(`/api/Peak/EnvioRevision/${vm.peak.Id}?periodoRevisionId=${vm.revisionSeleccionada.PeriodoRevision.Id}`)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                vm.revisionSeleccionada.Objetivos = result.Objetivos;

                                msgSuccess("Review Sent.");
                            }
                            else
                                msgError("Error sending the Review.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            submitComentariosRevision: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/ComentariosRevision/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            vm.revisionSeleccionada.Comentarios = vm.revision.comentarios;
                            vm.revisionSeleccionada.ComentariosJefe = vm.revision.comentariosJefe;
                            vm.revisionSeleccionada.FechaComentarios = result.FechaComentarios;
                            vm.revisionSeleccionada.FechaComentariosJefe = result.FechaComentariosJefe;

                            msgSuccess("Review Comments Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Review Comments.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalReviewComments: function () {
                var vm = this;

                vm.revision.comentarios = vm.revisionSeleccionada.Comentarios;
                vm.revision.comentariosJefe = vm.revisionSeleccionada.ComentariosJefe;

                vm.modal("#frmComentariosRevision .modal", function () {
                    if (vm.tipo == 'manage')
                        $("#frmComentariosRevision").find("[name=Comentarios]").focus();
                    else
                        $("#frmComentariosRevision").find("[name=ComentariosJefe]").focus();
                });
                $.MMS.validateForm("#frmComentariosRevision");
            },
            submitHeredar: function () {
                var vm = this;

                var data = {};
                var countSelected = 0;
                for (var i in vm.heredar.objetivos)
                    if (vm.heredar.objetivos[i].Heredar) {
                        data[`[${countSelected}].Value`] = vm.heredar.objetivos[i].Id;
                        countSelected++;
                    }

                if (countSelected == 0) {
                    msgAlert("At least one Objective must be selected.");
                    return;
                }

                if ((countSelected + vm.objetivos.length) > vm.numeroObjetivos) {
                    msgAlert(`Only ${vm.numeroObjetivos - vm.objetivos.length} Objetive(s) can be selected.`);
                    return;
                }

                sLoading();
                $.post("/api/Peak/HeredarObjetivos/" + vm.peak.Id, $.param(data))
                    .done(function (result) {
                        if (result) {
                            for (var i in result) {
                                result[i].FechaMeta = moment(result[i].FechaMeta).format("YYYY-MM-DD");
                                vm.objetivos.push(result[i]);
                            }

                            msgSuccess("Objectives Saved.");
                            $("#modalHeredar").modal("hide");
                        }
                        else
                            msgError("Error saving Objectives.");
                    })
                    .fail(handleError)
                    .always(hLoading);

            },
            modalHeredar: function () {
                var vm = this;
                vm.heredar.objetivos = [];

                $.get(`/api/Peak/HeredarObjetivos/${vm.peak.Id}`)
                    .done(function (result) {
                        for (var i in result) {
                            result[i].FechaMeta = moment(result[i].FechaMeta).format("YYYY-MM-DD");
                            result[i].Heredar = false;
                        }

                        vm.heredar.objetivos = result;
                    })
                    .fail(handleError);

                vm.modal("#modalHeredar");
            },
            sendModificationApprovalResults: function () {
                var vm = this;

                var countApproval = 0;
                for (var i in vm.objetivos)
                    if (vm.objetivos[i].Estado == 4 && vm.objetivos[i].AprobacionModificacion == null)
                        countApproval++;

                if (countApproval > 0) {
                    msgAlert("All modified Objectives must be Approved or Disapproved.");
                    return;
                }

                swal({
                    title: "Send Modification Approval Results",
                    text: "The Modification Approval Results will be send to the Associate",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Send",
                }, function () {
                    $.post("/api/Peak/EnvioResultadosModificacionAprobacion/" + vm.peak.Id)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                vm.asignarObjetivos(result.Objetivos);

                                msgSuccess("The Modification Approval Results were sent.");
                            }
                            else
                                msgError("Error sending The Modification Approval Results.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            submitAprobacionModificacionObjetivo: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/AprobacionModificacionObjetivo/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            var index = vm.objetivo.Numero - 1;

                            vm.objetivos[index].AprobacionModificacion = result.AprobacionModificacion;
                            vm.objetivos[index].ComentariosRechazo = (!result.AprobacionModificacion) ? vm.aprobacion.comentariosRechazo : null;

                            msgSuccess("Objective Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Objective.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalAprobacionModificationObjetivo: function (index) {
                var vm = this;

                vm.objetivo = jQuery.extend(true, {}, vm.objetivos[index]);
                vm.aprobacion.estado = vm.objetivo.AprobacionModificacion == null ? 1 : (vm.objetivo.AprobacionModificacion ? 2 : 3);
                vm.aprobacion.comentariosRechazo = vm.objetivo.ComentariosRechazo;

                vm.modal("#frmAprobacionModificacionObjetivo .modal", function () {
                    $("#frmAprobacionModificacionObjetivo").find("[name=ComentariosRechazo]").focus();
                });
                $.MMS.validateForm("#frmAprobacionModificacionObjetivo");
            },
            undoSendForModificationApproval: function () {
                var vm = this;

                swal({
                    title: "Undo Send for Modification Approval",
                    text: "The Objectives will be able to modify them.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                }, function () {
                    $.post("/api/Peak/CancelarAprobacionModificacionObjetivos/" + vm.peak.Id)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("Undo done.");
                            }
                            else
                                msgError("Error undoing the action.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            sendForModificationApproval: function () {
                var vm = this;

                var countModified = 0;
                for (var i in vm.objetivos)
                    if (vm.objetivos[i].Estado == 4) // Modificado
                        countModified++;

                if (!countModified) {
                    msgAlert("At least one objective must be modified.");
                    return;
                }

                swal({
                    title: "Send for Modification Approval",
                    text: "The Modified Objectives will be send for approval.<br>After this action you will not be able to modify them.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Send",
                }, function () {
                    $.post("/api/Peak/EnvioAprobacionModificacionObjetivos/" + vm.peak.Id)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("The Objectives were sent for modification approval.");
                            }
                            else
                                msgError("Error sending Objectives for modification approval.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            modalModificacionObjetivo: function (index) {
                var vm = this;

                vm.objetivo = jQuery.extend(true, {}, vm.objetivos[index]);
                vm.modificacion.motivoModificacion = vm.objetivo.MotivoModificacion;

                vm.modal("#frmModificacionObjetivo .modal", function () {
                    $("#frmModificacionObjetivo").find("[name=Objetivo]").focus();
                });
                $.MMS.validateForm("#frmModificacionObjetivo");
            },
            submitModificacionObjetivo: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/ModificacionObjetivo/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            var index = vm.objetivo.Numero - 1;
                            vm.objetivos[index].FechaMeta = vm.objetivo.FechaMeta;
                            vm.objetivos[index].Objetivo = vm.objetivo.Objetivo;
                            vm.objetivos[index].MedidoPor = vm.objetivo.MedidoPor;
                            vm.objetivos[index].Estado = result.Estado;
                            vm.objetivos[index].MotivoModificacion = vm.modificacion.motivoModificacion;
                            vm.objetivos[index].SolicitudEliminacionHeredado = vm.objetivo.SolicitudEliminacionHeredado;

                            msgSuccess("Objective Saved.");
                            $("#frmModificacionObjetivo .modal").modal("hide");
                        }
                        else
                            msgError("Error saving Objective.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            modalAvance: function (index) {
                var vm = this;

                vm.objetivo = jQuery.extend(true, {}, vm.objetivos[index]);
                vm.avance.resultadosActuales = vm.objetivo.ResultadosActuales;
                vm.avance.comentarios = vm.objetivo.Comentarios;

                vm.modal("#frmAvance .modal", function () {
                    vm.avance.completado = vm.objetivo.Completado;

                    if (!sliderCompletado) {
                        sliderCompletado = $("#nouislider_completado")[0];
                        noUiSlider.create(sliderCompletado, {
                            start: [vm.avance.completado],
                            connect: 'lower',
                            step: 1,
                            range: {
                                'min': [0],
                                'max': [100]
                            }
                        });
                        sliderCompletado.noUiSlider.on('update', function () {
                            var val = sliderCompletado.noUiSlider.get();
                            vm.avance.completado = parseInt(val);
                        });
                    }
                    else
                        sliderCompletado.noUiSlider.set(vm.avance.completado);

                    $("#frmAvance").find("[name=ResultadosActuales]").focus();
                });
                $.MMS.validateForm("#frmAvance");
            },
            submitAvance: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/Avance/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            var index = vm.objetivo.Numero - 1;
                            vm.objetivos[index].Completado = vm.avance.completado;
                            vm.objetivos[index].ResultadosActuales = vm.avance.resultadosActuales;
                            vm.objetivos[index].Comentarios = vm.avance.comentarios;
                            vm.recalcularCompletado();

                            msgSuccess("Objective Saved.");
                            $("#frmAvance .modal").modal("hide");
                        }
                        else
                            msgError("Error saving Objective.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            sendApprovalResults: function () {
                var vm = this;

                var countApproval = 0;
                for (var i in vm.objetivos)
                    if (vm.objetivos[i].Estado != 1)
                        countApproval++;

                if (countApproval != vm.numeroObjetivos) {
                    msgAlert("All Objectives must be Approved or Disapproved.");
                    return;
                }

                swal({
                    title: "Send Approval Results",
                    text: "The Approval Results will be send to the Associate",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Send",
                }, function () {
                    $.post("/api/Peak/EnvioResultadosAprobacion/" + vm.peak.Id)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("The Approval Results were sent.");
                            }
                            else
                                msgError("Error sending The Approval Results.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            modalAprobacionObjetivo: function (index) {
                var vm = this;

                vm.objetivo = jQuery.extend(true, {}, vm.objetivos[index]);
                vm.aprobacion.estado = vm.objetivo.Estado;
                vm.aprobacion.comentariosRechazo = vm.objetivo.ComentariosRechazo;

                vm.modal("#frmAprobacionObjetivo .modal", function () {
                    $("#frmAprobacionObjetivo").find("[name=ComentariosRechazo]").focus();
                });
                $.MMS.validateForm("#frmAprobacionObjetivo");
            },
            submitAprobacionObjetivo: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/AprobacionObjetivo/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            var index = vm.objetivo.Numero - 1;

                            vm.objetivos[index].Estado = result.Estado;
                            vm.objetivos[index].ComentariosRechazo = (result.Estado == 3) ? vm.aprobacion.comentariosRechazo : null;

                            msgSuccess("Objective Saved.");
                            $(event.target).find(".modal").modal("hide");
                        }
                        else
                            msgError("Error saving Objective.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            sendForApproval: function () {
                var vm = this;

                var sumPorcentaje = 0;
                for (var i in vm.objetivos)
                    sumPorcentaje += parseInt(vm.objetivos[i].Peso);

                if (sumPorcentaje != 100) {
                    msgAlert("The sum of the Weights must be 100%");
                    return;
                }

                swal({
                    title: "Send for Approval",
                    text: "The Objectives will be send for approval.<br>After this action you will not be able to modify them.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonText: "Send",
                }, function () {
                    $.post("/api/Peak/EnvioAprobacionObjetivos/" + vm.peak.Id)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("The Objectives were sent for approval.");
                            }
                            else
                                msgError("Error sending Objectives for approval.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            undoSendForApproval: function () {
                var vm = this;

                swal({
                    title: "Undo Send for Approval",
                    text: "The Objectives will be able to modify them.",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                }, function () {
                    $.post("/api/Peak/CancelarAprobacionObjetivos/" + vm.peak.Id)
                        .done(function (result) {
                            if (result) {
                                vm.peak.Estado = result.Estado;
                                msgSuccess("Undo done.");
                            }
                            else
                                msgError("Error undoing the action.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            eliminarObjetivo: function (index) {
                var vm = this;

                swal({
                    title: "Delete Objective",
                    text: "Are you sure you want to delete this Objective?",
                    type: "warning",
                    html: true,
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    confirmButtonColor: "#f44336",
                    confirmButtonText: "Delete",
                }, function () {
                    $.post("/api/Peak/EliminarObjetivo/" + vm.objetivos[index].Id)
                        .done(function (result) {
                            if (result) {
                                vm.objetivos.splice(index, 1);

                                for (var i = index; i < vm.objetivos.length; i++)
                                    vm.objetivos[i].Numero--;

                                msgSuccess("Objective deleted.");
                            }
                            else
                                msgError("Error deleting Objective.");
                        })
                        .fail(handleError)
                        .always(function () {
                            swal.close();
                        })
                });
            },
            modalObjetivo: function (index) {
                var vm = this;

                if (index == -1) {
                    vm.objetivo.Id = 0;
                    vm.objetivo.Numero = vm.objetivos.length + 1;
                    vm.objetivo.Peso = "";
                    vm.objetivo.Heredable = false;
                    vm.objetivo.Objetivo = "";
                    vm.objetivo.FechaMeta = "";
                    vm.objetivo.MedidoPor = "";
                    vm.objetivo.Estado = 0;
                    vm.objetivo.PeakObjetivoId = null;
                }
                else
                    vm.objetivo = jQuery.extend(true, {}, vm.objetivos[index]);

                vm.modal("#frmObjetivo .modal", function () {
                    $("#frmObjetivo").find("[name=Peso]").focus();
                });
                $.MMS.validateForm("#frmObjetivo");
            },
            submitObjetivo: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;
                var submit = function () {
                    sLoading();
                    $.post("/api/Peak/Objetivo/", $(event.target).serialize())
                        .done(function (result) {
                            if (result) {
                                if (vm.objetivo.Id == 0) {
                                    var objetivo = jQuery.extend(true, {}, vm.objetivo);

                                    objetivo.Id = result.Id;
                                    objetivo.Estado = result.Estado;
                                    vm.objetivos.push(objetivo);
                                }
                                else {
                                    vm.objetivos.splice(vm.objetivo.Numero - 1, 1, jQuery.extend(true, {}, vm.objetivo));
                                    vm.objetivos[vm.objetivo.Numero - 1].Estado = result.Estado;
                                }

                                msgSuccess("Objective Saved.");
                                $("#frmObjetivo .modal").modal("hide");
                            }
                            else
                                msgError("Error saving Objective.");
                        })
                        .fail(handleError)
                        .always(hLoading);
                };

                if (vm.objetivo.Estado == 2) // aprobado
                    swal({
                        title: "Objective Approved",
                        text: "This Objective is approved. If you modify it, the approval will be lost.",
                        type: "warning",
                        html: true,
                        showCancelButton: true,
                    }, submit);
                else
                    submit();
            },
            modalConfirmacion: function () {
                var vm = this;
                vm.confirmacion.areaId = vm.usuario.Area ? vm.usuario.Area.Id : null;
                vm.confirmacion.cargo = vm.usuario.Cargo;
                vm.confirmacion.usuarioPadre = vm.usuario.UsuarioPadre ? { id: vm.usuario.UsuarioPadre.Id, nombre: vm.usuario.UsuarioPadre.Nombre } : null;
                vm.modal("#frmConfirmacion .modal");
                $.MMS.validateForm("#frmConfirmacion");
            },
            submitConfirmacion: function (event) {
                if (!$(event.target).valid())
                    return;

                var vm = this;

                sLoading();
                $.post("/api/Peak/Confirmacion/", $(event.target).serialize())
                    .done(function (result) {
                        if (result) {
                            vm.peak.Id = result.Id;
                            vm.peak.Estado = result.Estado;
                            vm.usuario.Area = result.Area;
                            vm.usuario.Cargo = vm.confirmacion.cargo;
                            msgSuccess("Associate's Information Saved.");
                            $("#frmConfirmacion .modal").modal("hide");
                        }
                        else
                            msgError("Error saving Associate's Information.");
                    })
                    .fail(handleError)
                    .always(hLoading);
            },
            asignarObjetivos: function (objetivos) {
                for (var i in objetivos) {
                    objetivos[i].FechaMeta = moment(objetivos[i].FechaMeta).format("YYYY-MM-DD");

                    if (objetivos[i].Completado == null)
                        objetivos[i].Completado = 0;
                }

                this.objetivos = objetivos;
            },
            getSkillName: function (value) {
                if (!value)
                    return "";

                var vm = this;

                for (var i in vm.skills)
                    if (vm.skills[i].Value == value)
                        return vm.skills[i].Name;

                return "";
            },
            recalcularCompletado: function () {
                var vm = this;
                var sum = 0;

                for (var i in vm.objetivos)
                    sum += vm.objetivos[i].Completado;

                vm.completado = Math.round(sum / vm.numeroObjetivos);
            },
            recalcularCalificacion: function () {
                var vm = this;
                var sum = 0;

                for (var i in vm.objetivos)
                    sum += vm.objetivos[i].Factor == null ? 0 : vm.objetivos[i].Factor;

                vm.calificacion = sum + vm.peak.FactorAjuste;
            },
            modal: function (modalId, fnShown) {
                $(modalId)
                    .one('shown.bs.modal', function () {
                        $.AdminBSB.input.activate(modalId);
                        $.AdminBSB.select.activate(modalId);
                        updateMaterialTextFields(modalId);

                        if (fnShown)
                            fnShown();
                    })
                    .modal();
            },
            replaceIntro: function (text) {
                if (!text)
                    return "";

                return text.replaceAll('\n', '<br>');
            },
            tabSelected: function (tabId, index) {
                this.tabId = tabId;

                if (tabId == 2)
                    this.revisionSeleccionada = this.revisiones[index];
            },
        }
    });

}