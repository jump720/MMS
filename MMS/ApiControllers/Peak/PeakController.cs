using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Transactions;

namespace MMS.ApiControllers.Peak
{
    public class PeakController : ApiBaseController
    {
        private MMSContext db = new MMSContext();

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Index(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);

                int count = await db.Periodo
                    .CountAsync();

                var periodos = await db.Periodo
                    .Select(p => new
                    {
                        p.Id,
                        p.Descripcion,
                        p.FechaIni,
                        p.FechaFin,
                    })
                    .OrderByDescending(p => p.FechaIni)
                    .Skip(displayStart).Take(displayLength).ToListAsync();

                var periodosId = periodos.Select(p => p.Id).ToArray();

                var peaks = await db.Peak
                    .Where(p => p.UsuarioId == Seguridadcll.Usuario.UsuarioId && periodosId.Contains(p.PeriodoId))
                    .Select(p => new
                    {
                        p.Id,
                        p.PeriodoId,
                        p.Estado
                    })
                    .ToListAsync();

                var fechaActual = DateTime.Now;
                var periodoActual = await db.Periodo
                    .Include(p => p.PeriodoRevisiones)
                    .Where(p => p.FechaIni <= fechaActual && fechaActual <= p.FechaFin)
                    .FirstOrDefaultAsync();

                int EstadoPeriodoActual = 0;

                if (periodoActual != null)
                {
                    if (periodoActual.PeriodoRevisiones.Any(pr => (pr.FechaIni <= fechaActual && fechaActual <= pr.FechaFin) || pr.ActivoManual))
                        EstadoPeriodoActual = 2;
                    else if (periodoActual.RevisionFinalFechaIni <= fechaActual && fechaActual <= periodoActual.FechaFin)
                        EstadoPeriodoActual = 3;
                    else
                        EstadoPeriodoActual = 1;
                }

                var newData = periodos
                    .Select(p => new
                    {
                        p.Id,
                        p.Descripcion,
                        FechaIni = p.FechaIni.ToShortDateString(),
                        FechaFin = p.FechaFin.ToShortDateString(),
                        Peak = peaks.Where(pk => pk.PeriodoId == p.Id).Select(pk => new
                        {
                            pk.Id,
                            Estado = pk.Estado.ToString().Replace("_", " ")
                        }).FirstOrDefault(),
                        EstadoPeriodo = fechaActual > p.FechaFin ? -1 : (fechaActual < p.FechaIni ? -2 : EstadoPeriodoActual)
                    });

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = newData
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        class PeakData
        {
            public int NumeroObjetivos { get; set; }
            public PeakInfo Peak { get; set; }
            public PeriodoInfo Periodo { get; set; }
            public UsuarioInfo Usuario { get; set; }
            public List<AreaInfo> Areas { get; set; }
            public List<PeakObjetivo> Objetivos { get; set; }
            public List<RevisionInfo> Revisiones { get; set; }
            public dynamic CoreValues { get; set; }
            public dynamic Skills { get; set; }
            public dynamic PlanesDesarrollo { get; set; }

            public class RevisionInfo
            {
                public PeriodoRevisionInfo PeriodoRevision { get; set; }
                public string Comentarios { get; set; }
                public string FechaComentarios { get; set; }
                public string ComentariosJefe { get; set; }
                public string FechaComentariosJefe { get; set; }
                public bool Cerrada { get; set; }
                public dynamic Objetivos { get; set; }
            }

            public class PeriodoRevisionInfo
            {
                public int Id { get; set; }
                public string Nombre { get; set; }
                public string FechaIni { get; set; }
                public string FechaFin { get; set; }
                public bool Actual { get; set; }
            }

            public class PeriodoInfo
            {
                public int Id { get; set; }
                public string Descripcion { get; set; }
                public string FechaIni { get; set; }
                public string FechaFin { get; set; }
                public string RevisionFinalFechaIni { get; set; }
                public int Estado { get; set; }
                public bool Actual { get; set; }
                public bool RevisionFinal { get; set; }
            }

            public class UsuarioInfo
            {
                public string Id { get; set; }
                public string Nombre { get; set; }
                public string Cargo { get; set; }
                public AreaInfo Area { get; set; }
                public UsuarioInfo UsuarioPadre { get; set; }
            }

            public class AreaInfo
            {
                public int Id { get; set; }
                public string Nombre { get; set; }
            }

            public class PeakInfo
            {
                public int Id { get; set; }
                public EstadoPeak Estado { get; set; }
                public DateTime? FechaEnvio { get; set; }
                public string ComentariosCompetencias { get; set; }
                public string ResumenContribuciones { get; set; }
                public string ResumenContribucionesJefe { get; set; }
                public string Fortalezas { get; set; }
                public string FortalezasJefe { get; set; }
                public string ObjetivosFuturo { get; set; }
                public string ObjetivosFuturoJefe { get; set; }
                public float FactorAjuste { get; set; }
                public string JustificacionFactorAjuste { get; set; }
                public string RendimientoGeneral { get; set; }
            }
        }

        [HttpGet]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> Get(int id, string tipo)
        {
            try
            {
                if (tipo != "manage" && tipo != "review")
                    return BadRequest();

                var fechaActual = DateTime.Now;
                Periodo periodo = null;

                var peakQuery = db.Peak
                    .Include(p => p.Area)
                    .Include(p => p.Usuario)
                    .Include(p => p.PeakRevisiones)
                    .Include(p => p.PeakPlanesDesarrollo)
                    .Include(p => p.PeakObjetivos);

                if (tipo == "manage")
                {
                    periodo = await db.Periodo
                        .Include(p => p.PeriodoRevisiones)
                        .Where(p => p.Id == id)
                        .FirstOrDefaultAsync();

                    if (periodo == null)
                        return NotFound();

                    peakQuery = peakQuery
                        .Where(p => p.UsuarioId == Seguridadcll.Usuario.UsuarioId && p.PeriodoId == id);
                }
                else
                    peakQuery = peakQuery
                        .Where(p => p.Id == id);

                var peak = await peakQuery
                    .Select(p => new
                    {
                        p.UsuarioIdPadre,
                        p.PeriodoId,
                        Peak = new PeakData.PeakInfo
                        {
                            Id = p.Id,
                            Estado = p.Estado,
                            FechaEnvio = p.FechaEnvio,
                            ComentariosCompetencias = p.ComentariosCompetencias,
                            Fortalezas = p.Fortalezas,
                            FortalezasJefe = p.FortalezasJefe,
                            ObjetivosFuturo = p.ObjetivosFuturo,
                            ObjetivosFuturoJefe = p.ObjetivosFuturoJefe,
                            ResumenContribuciones = p.ResumenContribuciones,
                            ResumenContribucionesJefe = p.ResumenContribucionesJefe,
                            FactorAjuste = p.FactorAjuste,
                            JustificacionFactorAjuste = p.JustificacionFactorAjuste,
                            RendimientoGeneral = p.RendimientoGeneral,
                        },
                        Usuario = new PeakData.UsuarioInfo
                        {
                            Id = p.UsuarioId,
                            Nombre = p.Usuario.UsuarioNombre,
                            Cargo = p.Cargo,
                            Area = new PeakData.AreaInfo
                            {
                                Id = p.Area.Id,
                                Nombre = p.Area.Nombre
                            }
                        },
                        Objetivos = p.PeakObjetivos.ToList(),
                        Revisiones = p.PeakRevisiones.ToList(),
                        PlanesDesarrollo = p.PeakPlanesDesarrollo.ToList(),
                    })
                    .FirstOrDefaultAsync();

                if (tipo == "review")
                {
                    if (peak == null)
                        return NotFound();

                    periodo = await db.Periodo
                        .Include(p => p.PeriodoRevisiones)
                        .Where(p => p.Id == peak.PeriodoId)
                        .FirstOrDefaultAsync();

                    if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                        return BadRequest("You are not the Manager of the Associate's Peak.");
                }

                var periodoRevisionActivo = periodo.PeriodoRevisiones.FirstOrDefault(pr => pr.ActivoManual);
                if (periodoRevisionActivo == null)
                    periodoRevisionActivo = periodo.PeriodoRevisiones.FirstOrDefault(pr => pr.FechaIni <= fechaActual && fechaActual <= pr.FechaFin);

                string usuarioIdPadre;
                var data = new PeakData
                {
                    Revisiones = periodo.PeriodoRevisiones
                        .Select(pr => new PeakData.RevisionInfo
                        {
                            PeriodoRevision = new PeakData.PeriodoRevisionInfo
                            {
                                Id = pr.Id,
                                Nombre = pr.Nombre,
                                FechaIni = pr.FechaIni.ToString("yyyy-MM-dd"),
                                FechaFin = pr.FechaFin.ToString("yyyy-MM-dd"),
                                Actual = periodoRevisionActivo == null ? false : (periodoRevisionActivo.Id == pr.Id)
                            },
                            Objetivos = new List<object>()
                        })
                        .ToList()
                };

                if (peak == null)
                {
                    var usuarioData = await db.Usuarios
                        .Include(u => u.UsuarioHV)
                        .Include(u => u.UsuarioHV.Area)
                        .Where(u => u.UsuarioId == Seguridadcll.Usuario.UsuarioId)
                        .Select(u => new
                        {
                            u.UsuarioPadreId,
                            u.UsuarioHV.Area,
                            Usuario = new PeakData.UsuarioInfo
                            {
                                Id = u.UsuarioId,
                                Nombre = u.UsuarioNombre,
                                Area = u.UsuarioHV != null && u.UsuarioHV.Area != null ? new PeakData.AreaInfo
                                {
                                    Id = u.UsuarioHV.Area.Id,
                                    Nombre = u.UsuarioHV.Area.Nombre
                                } : null,
                                Cargo = u.UsuarioHV != null ? u.UsuarioHV.Cargo : null
                            }
                        })
                        .FirstOrDefaultAsync();

                    data.Peak = new PeakData.PeakInfo();
                    data.Usuario = usuarioData.Usuario;
                    usuarioIdPadre = usuarioData.UsuarioPadreId;
                    data.Objetivos = new List<PeakObjetivo>();
                    data.PlanesDesarrollo = new List<object>();

                    data.CoreValues = (await db.CoreValue
                        .Where(cv => cv.Activo)
                        .OrderBy(cv => cv.Orden)
                        .Select(cv => new
                        {
                            CoreValue = new
                            {
                                cv.Id,
                                cv.Nombre,
                                cv.Descripcion,
                                cv.Competencia,
                                cv.CompetenciaDescripcion,
                                cv.HabilidadAlta,
                                cv.HabilidadMedia,
                                cv.HabilidadBaja,
                            }
                        })
                        .ToListAsync())
                        .Select(cv => new
                        {
                            cv.CoreValue,
                            Evaluacion = null as object,
                            Autoevaluacion = null as object,
                        })
                        .ToList();
                }
                else
                {
                    data.Peak = peak.Peak;
                    data.Usuario = peak.Usuario;
                    usuarioIdPadre = peak.UsuarioIdPadre;
                    data.Objetivos = peak.Objetivos;

                    data.PlanesDesarrollo = peak.PlanesDesarrollo
                        .Select(ppd => new
                        {
                            ppd.Id,
                            ppd.Area,
                            ppd.Plan,
                            FechaMeta = ppd.FechaMeta.ToString("yyyy-MM-dd"),
                            ppd.ResultadoDeseado,
                        })
                        .ToList();

                    if (data.Peak.Estado == EstadoPeak.Final_Review || data.Peak.Estado == EstadoPeak.Finished)
                        data.CoreValues = await db.PeakCoreValue
                            .Include(pcv => pcv.CoreValue)
                            .Where(pcv => pcv.PeakId == peak.Peak.Id)
                            .OrderBy(pcv => pcv.CoreValue.Orden)
                            .Select(pcv => new
                            {
                                CoreValue = new
                                {
                                    pcv.CoreValue.Id,
                                    pcv.CoreValue.Nombre,
                                    pcv.CoreValue.Descripcion,
                                    pcv.CoreValue.Competencia,
                                    pcv.CoreValue.CompetenciaDescripcion,
                                    pcv.CoreValue.HabilidadAlta,
                                    pcv.CoreValue.HabilidadMedia,
                                    pcv.CoreValue.HabilidadBaja,
                                },
                                pcv.Evaluacion,
                                pcv.Autoevaluacion
                            })
                            .ToListAsync();
                    else
                    {
                        var peakCoreValues = await db.PeakCoreValue
                            .Where(pcv => pcv.PeakId == peak.Peak.Id)
                            .ToListAsync();

                        var coreValues = await db.CoreValue
                            .Where(cv => cv.Activo)
                            .OrderBy(cv => cv.Orden)
                            .Select(cv => new
                            {
                                CoreValue = new
                                {
                                    cv.Id,
                                    cv.Nombre,
                                    cv.Descripcion,
                                    cv.Competencia,
                                    cv.CompetenciaDescripcion,
                                    cv.HabilidadAlta,
                                    cv.HabilidadMedia,
                                    cv.HabilidadBaja,
                                },
                            })
                            .ToListAsync();

                        data.CoreValues = coreValues
                            .GroupJoin(peakCoreValues,
                                cv => cv.CoreValue.Id,
                                pcv => pcv.CoreValueId,
                                (cv, g) => new { CoreValue = cv.CoreValue, PeakCoreValue = g.FirstOrDefault() })
                            .Select(x => new
                            {
                                x.CoreValue,
                                Evaluacion = x.PeakCoreValue?.Evaluacion,
                                Autoevaluacion = x.PeakCoreValue?.Autoevaluacion,
                            })
                            .ToList();
                    }

                    var periodoRevisionesId = peak.Revisiones.Select(pr => pr.PeriodoRevisionId).ToArray();

                    var objetivosRevisiones = await db.PeakObjetivoRevision
                        .Include(por => por.PeakObjetivo)
                        .Where(por => por.PeakObjetivo.PeakId == peak.Peak.Id && periodoRevisionesId.Contains(por.PeriodoRevisionId))
                        .Select(por => new
                        {
                            por.PeriodoRevisionId,
                            por.PeakObjetivoId,
                            por.Objetivo,
                            por.FechaMeta,
                            por.MedidoPor,
                            por.ResultadosActuales,
                            por.Completado,
                            por.Comentarios,
                            por.ComentariosJefe,
                            por.PeakObjetivoIdHeredado,
                            por.TieneCambios,
                        })
                        .ToListAsync();

                    foreach (var revision in peak.Revisiones)
                    {
                        var peakRevision = data.Revisiones.First(pr => pr.PeriodoRevision.Id == revision.PeriodoRevisionId);

                        peakRevision.Comentarios = revision.Comentarios;
                        peakRevision.FechaComentarios = revision.FechaComentarios.ToString("yyyy-MM-dd hh:mm");
                        peakRevision.ComentariosJefe = revision.ComentariosJefe;
                        peakRevision.FechaComentariosJefe = revision.FechaComentariosJefe?.ToString("yyyy-MM-dd hh:mm");
                        peakRevision.Cerrada = revision.Cerrada;

                        var objetivosRevision = objetivosRevisiones
                            .Where(or => or.PeriodoRevisionId == revision.PeriodoRevisionId)
                            .Select(or => new
                            {
                                ObjetivoRevision = or,
                                Objetivo = peak.Objetivos.First(o => o.Id == or.PeakObjetivoId)
                            })
                            .ToList();

                        peakRevision.Objetivos = objetivosRevision
                            .Select(or => new
                            {
                                or.Objetivo.Id,
                                or.Objetivo.Numero,
                                or.Objetivo.Peso,
                                or.Objetivo.Heredable,
                                PeakObjetivoId = or.ObjetivoRevision.PeakObjetivoIdHeredado,
                                or.ObjetivoRevision.Objetivo,
                                FechaMeta = or.ObjetivoRevision.FechaMeta.ToString("yyyy-MM-dd"),
                                or.ObjetivoRevision.MedidoPor,
                                or.ObjetivoRevision.ResultadosActuales,
                                or.ObjetivoRevision.Completado,
                                or.ObjetivoRevision.Comentarios,
                                or.ObjetivoRevision.ComentariosJefe,
                                or.ObjetivoRevision.TieneCambios
                            }).ToList();
                    }
                }

                if (usuarioIdPadre != null)
                {
                    bool looping = true;
                    while (looping)
                    {
                        var usuarioData = await db.Usuarios
                            .Where(u => u.UsuarioId == usuarioIdPadre)
                            .Select(u => new
                            {
                                u.UsuarioPadreId,
                                u.Usuarioactivo,
                                Usuario = new PeakData.UsuarioInfo
                                {
                                    Id = u.UsuarioId,
                                    Nombre = u.UsuarioNombre,
                                }
                            })
                            .FirstOrDefaultAsync();

                        if (usuarioData == null)
                            looping = false;
                        else if (usuarioData.Usuarioactivo)
                        {
                            data.Usuario.UsuarioPadre = usuarioData.Usuario;
                            looping = false;
                        }
                        else if (usuarioData.UsuarioPadreId == null)
                            looping = false;
                        else
                            usuarioIdPadre = usuarioData.UsuarioPadreId;
                    }
                }

                short EstadoPeriodoActual = 0;
                bool actual = false;

                if (periodo.FechaIni <= fechaActual && fechaActual <= periodo.FechaFin)
                {
                    actual = true;

                    if (periodo.PeriodoRevisiones.Any(pr => (pr.FechaIni <= fechaActual && fechaActual <= pr.FechaFin) || pr.ActivoManual))
                        EstadoPeriodoActual = 2;
                    else if (periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= periodo.FechaFin)
                        EstadoPeriodoActual = 3;
                    else
                        EstadoPeriodoActual = 1;
                }

                data.Areas = await db.Area
                    .Select(a => new PeakData.AreaInfo
                    {
                        Id = a.Id,
                        Nombre = a.Nombre
                    })
                    .ToListAsync();

                data.Periodo = new PeakData.PeriodoInfo
                {
                    Id = periodo.Id,
                    Descripcion = periodo.Descripcion,
                    FechaIni = periodo.FechaIni.ToString("yyyy-MM-dd"),
                    RevisionFinalFechaIni = periodo.RevisionFinalFechaIni.ToString("yyyy-MM-dd"),
                    FechaFin = periodo.FechaFin.ToString("yyyy-MM-dd"),
                    Estado = fechaActual > periodo.FechaFin ? -1 : (fechaActual < periodo.FechaIni ? -2 : EstadoPeriodoActual),
                    Actual = actual,
                    RevisionFinal = periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= periodo.FechaFin
                };

                data.NumeroObjetivos = Seguridadcll.Configuracion.ConfigPeakNumeroObjetivos;
                data.Skills = Fn.EnumToIEnumarable<Skill>()
                    .Select(s => new
                    {
                        s.Value,
                        Name = s.Name.ToString().Replace("_", " ")
                    })
                    .ToList();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> Confirmacion(PeakConfirmacionViewModel model)
        {
            try
            {
                var periodo = await db.Periodo
                    .Where(p => p.Id == model.PeriodoId)
                    .FirstOrDefaultAsync();

                if (periodo == null)
                    return BadRequest("Invalid Period.");

                var fechaActual = DateTime.Now;
                if (!(periodo.FechaIni <= fechaActual && fechaActual <= periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Period");

                var peak = new Models.Peak
                {
                    UsuarioId = Seguridadcll.Usuario.UsuarioId,
                    UsuarioIdPadre = model.UsuarioIdPadre,
                    PeriodoId = model.PeriodoId,
                    AreaId = model.AreaId,
                    Cargo = model.Cargo,
                    Estado = EstadoPeak.Objectives_Definition,
                    FactorAjuste = 0
                };
                db.Peak.Add(peak);

                await db.SaveChangesAsync();

                model.PeakId = peak.Id;
                AddLog("", peak.Id, model);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado,
                    Area = await db.Entry(peak).Reference(p => p.Area).Query().Select(a => new PeakData.AreaInfo
                    {
                        Id = a.Id,
                        Nombre = a.Nombre
                    }).FirstOrDefaultAsync()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> Objetivo(PeakObjetivo objetivo)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == objetivo.PeakId)
                    .Select(p => new
                    {
                        p.Estado,
                        p.Periodo,
                        ObjetivosCount = p.PeakObjetivos.Count()
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Definition)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (objetivo.Id == 0 && peak.ObjetivosCount >= Seguridadcll.Configuracion.ConfigPeakNumeroObjetivos)
                    return BadRequest($"This Peak has already ${Seguridadcll.Configuracion.ConfigPeakNumeroObjetivos} Objectives.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Period");

                PeakObjetivo peakObjetivo;

                if (objetivo.Id == 0)
                {
                    peakObjetivo = objetivo;
                    peakObjetivo.PeakObjetivoId = null;
                    peakObjetivo.Estado = EstadoPeakObjetivo.Defined;

                    db.PeakObjetivo.Add(peakObjetivo);
                }
                else
                {
                    peakObjetivo = await db.PeakObjetivo.FindAsync(objetivo.Id);
                    if (peakObjetivo == null)
                        return BadRequest("Given Objective does not exist.");

                    peakObjetivo.Peso = objetivo.Peso;
                    peakObjetivo.FechaMeta = objetivo.FechaMeta;
                    peakObjetivo.Objetivo = objetivo.Objetivo;
                    peakObjetivo.MedidoPor = objetivo.MedidoPor;
                    peakObjetivo.Heredable = objetivo.Heredable;
                    peakObjetivo.PeakObjetivoId = objetivo.PeakObjetivoId;
                    peakObjetivo.Estado = EstadoPeakObjetivo.Defined;

                    db.Entry(peakObjetivo).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                AddLog("", peakObjetivo.Id, peakObjetivo);

                return Ok(new
                {
                    peakObjetivo.Id,
                    peakObjetivo.Estado,
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EliminarObjetivo(int id)
        {
            try
            {
                var data = await db.PeakObjetivo
                    .Include(po => po.Peak)
                    .Include(po => po.Peak.Periodo)
                    .Where(po => po.Id == id)
                    .Select(po => new
                    {
                        PeakObjetivo = po,
                        po.Peak.Id,
                        po.Peak.Estado,
                        po.Peak.Periodo,
                        po.Peak.UsuarioIdPadre
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return BadRequest("Given Objective does not exist.");
                else if (data.Estado != EstadoPeak.Objectives_Definition)
                    return BadRequest($"This Peak is in {data.Estado.ToString().Replace("_", " ")}.");

                var fechaActual = DateTime.Now;
                if (!(data.Periodo.FechaIni <= fechaActual && fechaActual <= data.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Period");

                using (var db2 = new MMSContext())
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        db.PeakObjetivo.Remove(data.PeakObjetivo);
                        await db.SaveChangesAsync();

                        var objetivos = await db2.PeakObjetivo
                            .Where(po => po.PeakId == data.Id && po.Numero > data.PeakObjetivo.Numero)
                            .ToListAsync();

                        foreach (var po in objetivos)
                        {
                            po.Numero--;
                            db.Entry(po).State = EntityState.Modified;
                        }

                        await db2.SaveChangesAsync();
                        scope.Complete();
                    }
                }

                AddLog("", data.PeakObjetivo, data.PeakObjetivo);
                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        enum TipoNotificacion
        {
            Review, Result, Undo, Finished, Opened, DP
        }

        private void SendNotificationEmailTask(string to, string toName, EstadoPeak estado, int peakId, TipoNotificacion tipo)
        {
            if (string.IsNullOrWhiteSpace(to))
                return;

            string subject = "AIS - ";
            string msg = $"The user <b>{Seguridadcll.Usuario.UsuarioNombre}</b> ";
            string action = "Manage";

            if (tipo == TipoNotificacion.Review)
            {
                subject += "A Peak has been sent to you for review.";
                msg += "has sent a Peak to you for review.";
                action = "Review";
            }
            else if (tipo == TipoNotificacion.Result)
            {
                subject += "Your Peak has been sent to you back.";
                msg += "has sent you your Peak back.";
            }
            else if (tipo == TipoNotificacion.Undo)
            {
                subject += "Peak cancel from review.";
                msg += "has cancel the Peak from review.";
                action = "Review";
            }
            else if (tipo == TipoNotificacion.Finished)
            {
                subject += "Your Peak has been finished.";
                msg += "has finished your Peak.";
            }
            else if (tipo == TipoNotificacion.Opened)
            {
                subject += "Your Peak Review has been opened.";
                msg += "has opened your Peak Review.";
            }
            else if (tipo == TipoNotificacion.DP)
            {
                subject += "Your Peak Final Review has been finished. Meet your manager to define your Development Plan";
                msg += "has finished your Peak Final Review.";
            }

            msg += $"<br /><br /><b>Peak Status</b>: {estado.ToString().Replace("_", " ")}<br /><br />";
            msg += $"<a style='color:#22BCE5' href={{url}}/Peak/{action}/{peakId}>Click here to view the Peak.</a>";

            Task.Run(() => Fn.SendHtmlEmail(to, toName, subject, msg, Seguridadcll.Aplicacion.Link));
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EnvioAprobacionObjetivos(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.UsuarioPadre)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Definition)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.PeakObjetivos.Count != Seguridadcll.Configuracion.ConfigPeakNumeroObjetivos)
                    return BadRequest($"There must be {Seguridadcll.Configuracion.ConfigPeakNumeroObjetivos} Objectives for approval.");
                else if (peak.PeakObjetivos.Sum(po => po.Peso) != 100)
                    return BadRequest("The sum of the Weights must be 100%");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Period");

                peak.Estado = EstadoPeak.Objectives_Approval;
                peak.FechaEnvio = DateTime.Now;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, new { Objetivos = peak.PeakObjetivos });

                if (peak.UsuarioIdPadre != null)
                    SendNotificationEmailTask(peak.UsuarioPadre.UsuarioCorreo, peak.UsuarioPadre.UsuarioNombre, peak.Estado, peak.Id, TipoNotificacion.Review);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> CancelarAprobacionObjetivos(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.UsuarioPadre)
                    .Include(p => p.Periodo)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Approval)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.PeakObjetivos.Any(po => po.UltimaFechaAprobacion > peak.FechaEnvio))
                    return BadRequest("Your Manager has already started the Approval.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Period");

                peak.Estado = EstadoPeak.Objectives_Definition;
                peak.FechaEnvio = null;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, null);

                if (peak.UsuarioIdPadre != null)
                    SendNotificationEmailTask(peak.UsuarioPadre.UsuarioCorreo, peak.UsuarioPadre.UsuarioNombre, peak.Estado, peak.Id, TipoNotificacion.Undo);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> Reviews()
        {
            try
            {
                var peaksQuery = db.Peak
                   .Include(p => p.Area)
                   .Include(p => p.Periodo)
                   .Include(p => p.Usuario);

                if (Seguridadcll.RolObjetoList.Any(ro => ro.ObjetoId == "Peak/AdminReview"))
                    peaksQuery = peaksQuery.Where(p => p.UsuarioIdPadre == Seguridadcll.Usuario.UsuarioId || p.UsuarioIdPadre == null);
                else
                    peaksQuery = peaksQuery.Where(p => p.UsuarioIdPadre == Seguridadcll.Usuario.UsuarioId);

                var peaks = await peaksQuery
                   .Select(p => new
                   {
                       p.Id,
                       Usuario = p.Usuario.UsuarioNombre,
                       Area = p.Area.Nombre,
                       Periodo = p.Periodo.Descripcion,
                       p.Cargo,
                       p.FechaEnvio,
                       p.Estado
                   })
                   .ToListAsync();

                return Ok(peaks.Select(p => new
                {
                    p.Id,
                    p.Usuario,
                    p.Area,
                    p.Periodo,
                    p.Cargo,
                    FechaEnvio = p.FechaEnvio.HasValue ? (p.FechaEnvio.Value.ToShortDateString() + " " + p.FechaEnvio.Value.ToShortTimeString()) : "",
                    Estado = p.Estado.ToString().Replace("_", " ")
                })
                .ToList());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> AprobacionObjetivo(PeakObjetivo objetivo)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == objetivo.PeakId)
                    .Select(p => new
                    {
                        p.Estado,
                        p.Periodo,
                        p.UsuarioIdPadre
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Approval)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Period");

                var peakObjetivo = await db.PeakObjetivo.FindAsync(objetivo.Id);
                if (peakObjetivo == null)
                    return BadRequest("Given Objective does not exist.");

                peakObjetivo.ComentariosRechazo = (objetivo.Estado == EstadoPeakObjetivo.Approved) ? null : objetivo.ComentariosRechazo;
                peakObjetivo.Estado = objetivo.Estado;
                peakObjetivo.UltimaFechaAprobacion = DateTime.Now;
                db.Entry(peakObjetivo).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peakObjetivo.Id, peakObjetivo);

                return Ok(new
                {
                    peakObjetivo.Id,
                    peakObjetivo.Estado,
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EnvioResultadosAprobacion(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Usuario)
                    .Include(p => p.Periodo)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Approval)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.PeakObjetivos.Any(po => po.Estado != EstadoPeakObjetivo.Approved && po.Estado != EstadoPeakObjetivo.Disapproved))
                    return BadRequest("All Objectives must be Approved or Disapproved.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Period");

                if (peak.PeakObjetivos.All(po => po.Estado == EstadoPeakObjetivo.Approved))
                    peak.Estado = EstadoPeak.Standby;
                else
                    peak.Estado = EstadoPeak.Objectives_Definition;

                peak.FechaEnvio = null;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, new { Objetivos = peak.PeakObjetivos });

                SendNotificationEmailTask(peak.Usuario.UsuarioCorreo, peak.Usuario.UsuarioNombre, peak.Estado, peak.Periodo.Id, TipoNotificacion.Result);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> Avance(PeakObjetivo objetivo)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == objetivo.PeakId)
                    .Select(p => new
                    {
                        p.Estado,
                        p.Periodo
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Standby && peak.Estado != EstadoPeak.Objectives_Modification_Approval && peak.Estado != EstadoPeak.Review)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Period");

                var peakObjetivo = await db.PeakObjetivo.FindAsync(objetivo.Id);
                if (peakObjetivo == null)
                    return BadRequest("Given Objective does not exist.");

                peakObjetivo.Completado = objetivo.Completado;
                peakObjetivo.ResultadosActuales = objetivo.ResultadosActuales;
                peakObjetivo.Comentarios = objetivo.Comentarios;
                db.Entry(peakObjetivo).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peakObjetivo.Id, peakObjetivo);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> ModificacionObjetivo(PeakObjetivo objetivo)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == objetivo.PeakId)
                    .Select(p => new
                    {
                        p.Estado,
                        p.Periodo,
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Standby)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Period");

                var peakObjetivo = await db.PeakObjetivo
                    .Include(po => po.PeakObjetivosHeredados)
                    .Where(po => po.Id == objetivo.Id)
                    .FirstOrDefaultAsync();

                if (peakObjetivo == null)
                    return BadRequest("Given Objective does not exist.");
                else if (peakObjetivo.PeakObjetivosHeredados.Count > 0)
                    return BadRequest("Others users have inherited this Objective, It cannot be modified.");

                peakObjetivo.FechaMeta = objetivo.FechaMeta;
                peakObjetivo.Objetivo = objetivo.Objetivo;
                peakObjetivo.MedidoPor = objetivo.MedidoPor;
                peakObjetivo.MotivoModificacion = objetivo.MotivoModificacion;
                peakObjetivo.Estado = EstadoPeakObjetivo.Modified;

                if (peakObjetivo.PeakObjetivoId != null)
                    peakObjetivo.SolicitudEliminacionHeredado = objetivo.SolicitudEliminacionHeredado;

                db.Entry(peakObjetivo).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peakObjetivo.Id, peakObjetivo);

                return Ok(new
                {
                    peakObjetivo.Id,
                    peakObjetivo.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EnvioAprobacionModificacionObjetivos(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.UsuarioPadre)
                    .Include(p => p.Periodo)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Standby)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (!peak.PeakObjetivos.Any(po => po.Estado == EstadoPeakObjetivo.Modified))
                    return BadRequest("At least one objective must be modified.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Period");

                peak.Estado = EstadoPeak.Objectives_Modification_Approval;
                peak.FechaEnvio = DateTime.Now;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, new { Objetivos = peak.PeakObjetivos });

                if (peak.UsuarioIdPadre != null)
                    SendNotificationEmailTask(peak.UsuarioPadre.UsuarioCorreo, peak.UsuarioPadre.UsuarioNombre, peak.Estado, peak.Id, TipoNotificacion.Review);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> CancelarAprobacionModificacionObjetivos(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.UsuarioPadre)
                    .Include(p => p.Periodo)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Modification_Approval)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.PeakObjetivos.Any(po => po.UltimaFechaAprobacion > peak.FechaEnvio))
                    return BadRequest("Your Manager has already started the Modification Approval.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Period");

                peak.Estado = EstadoPeak.Standby;
                peak.FechaEnvio = null;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, null);

                if (peak.UsuarioIdPadre != null)
                    SendNotificationEmailTask(peak.UsuarioPadre.UsuarioCorreo, peak.UsuarioPadre.UsuarioNombre, peak.Estado, peak.Id, TipoNotificacion.Undo);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> AprobacionModificacionObjetivo(PeakObjetivo objetivo)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == objetivo.PeakId)
                    .Select(p => new
                    {
                        p.Estado,
                        p.Periodo,
                        p.UsuarioIdPadre
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Modification_Approval)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Period");

                var peakObjetivo = await db.PeakObjetivo.FindAsync(objetivo.Id);
                if (peakObjetivo == null)
                    return BadRequest("Given Objective does not exist.");

                peakObjetivo.ComentariosRechazo = (objetivo.AprobacionModificacion == true) ? null : objetivo.ComentariosRechazo;
                peakObjetivo.AprobacionModificacion = objetivo.AprobacionModificacion;
                peakObjetivo.UltimaFechaAprobacion = DateTime.Now;
                db.Entry(peakObjetivo).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peakObjetivo.Id, peakObjetivo);

                return Ok(new
                {
                    peakObjetivo.Id,
                    peakObjetivo.AprobacionModificacion,
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EnvioResultadosModificacionAprobacion(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Usuario)
                    .Include(p => p.Periodo)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Modification_Approval)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.PeakObjetivos.Any(po => po.Estado == EstadoPeakObjetivo.Modified && po.AprobacionModificacion == null))
                    return BadRequest("All modified Objectives must be Approved or Disapproved.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Period");

                foreach (var objetivo in peak.PeakObjetivos)
                {
                    if (objetivo.Estado != EstadoPeakObjetivo.Modified)
                        continue;

                    if (objetivo.SolicitudEliminacionHeredado == true && objetivo.AprobacionModificacion == true)
                        objetivo.PeakObjetivoId = null;

                    objetivo.Estado = objetivo.AprobacionModificacion == true ? EstadoPeakObjetivo.Approved : EstadoPeakObjetivo.Disapproved;
                    objetivo.AprobacionModificacion = null;
                    objetivo.MotivoModificacion = null;
                    objetivo.SolicitudEliminacionHeredado = null;

                    db.Entry(objetivo).State = EntityState.Modified;
                }

                peak.Estado = EstadoPeak.Standby;
                peak.FechaEnvio = null;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, new { Objetivos = peak.PeakObjetivos });

                SendNotificationEmailTask(peak.Usuario.UsuarioCorreo, peak.Usuario.UsuarioNombre, peak.Estado, peak.Periodo.Id, TipoNotificacion.Result);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado,
                    Objetivos = peak.PeakObjetivos.ToList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> HeredarObjetivos(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.PeriodoId,
                        p.UsuarioIdPadre,
                        p.PeakObjetivos
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.UsuarioIdPadre == null)
                    return BadRequest("Given Peak has no Manager.");

                var objetivosId = peak.PeakObjetivos
                    .Where(po => po.PeakObjetivoId != null)
                    .Select(po => (int)po.PeakObjetivoId)
                    .ToArray();

                var objetivos = await db.PeakObjetivo
                    .Include(p => p.Peak)
                    .Where(po => po.Peak.PeriodoId == peak.PeriodoId && po.Peak.UsuarioId == peak.UsuarioIdPadre && po.Estado == EstadoPeakObjetivo.Approved && po.Heredable && po.Peak.Estado != EstadoPeak.Objectives_Definition && po.Peak.Estado != EstadoPeak.Objectives_Approval && !objetivosId.Contains(po.Id))
                    .Select(po => new
                    {
                        po.Id,
                        po.Peso,
                        po.Objetivo,
                        po.FechaMeta,
                        po.MedidoPor
                    })
                    .ToListAsync();

                return Ok(objetivos);

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> HeredarObjetivos(int id, List<Fn.DummyInt> objetivosId)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Estado,
                        p.Periodo,
                        p.UsuarioIdPadre,
                        p.PeakObjetivos
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Objectives_Definition)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (objetivosId.Count == 0)
                    return BadRequest($"No Objective given.");
                else if ((objetivosId.Count + peak.PeakObjetivos.Count) > Seguridadcll.Configuracion.ConfigPeakNumeroObjetivos)
                    return BadRequest($"Only ${Seguridadcll.Configuracion.ConfigPeakNumeroObjetivos - peak.PeakObjetivos.Count} can be selected.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.FechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Period");

                var objetivosIds = objetivosId.Select(o => o.Value).ToArray();
                var objetivos = await db.PeakObjetivo
                    .Where(po => objetivosIds.Contains(po.Id))
                    .ToListAsync();

                if (objetivosId.Count != objetivos.Count)
                    return BadRequest("Given Objetives were not found.");

                var objetivosAdded = new List<PeakObjetivo>();
                int numero = 1;

                if (peak.PeakObjetivos.Count > 0)
                    numero = peak.PeakObjetivos.Max(po => po.Numero) + 1;

                foreach (var objetivo in objetivos)
                {
                    if (objetivo.Estado != EstadoPeakObjetivo.Approved)
                        return BadRequest($"Objecive is {objetivo.Estado.ToString()}");

                    var peakObjectivo = new PeakObjetivo
                    {
                        PeakId = peak.Id,
                        Numero = (short)numero,
                        Peso = objetivo.Peso,
                        FechaMeta = objetivo.FechaMeta,
                        Objetivo = objetivo.Objetivo,
                        MedidoPor = objetivo.MedidoPor,
                        Heredable = false,
                        Estado = EstadoPeakObjetivo.Defined,
                        PeakObjetivoId = objetivo.Id
                    };
                    numero++;
                    db.PeakObjetivo.Add(peakObjectivo);
                    objetivosAdded.Add(peakObjectivo);
                }

                await db.SaveChangesAsync();
                AddLog("", peak.Id, new { Objetivos = objetivosAdded });

                return Ok(objetivosAdded);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> ComentariosRevision(PeakRevision revision)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.Periodo.PeriodoRevisiones)
                    .Where(p => p.Id == revision.PeakId)
                    .Select(p => new
                    {
                        p.Id,
                        p.Estado,
                        p.Periodo,
                        p.UsuarioIdPadre,
                        p.Periodo.PeriodoRevisiones,
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");

                var periodoRevision = peak.Periodo.PeriodoRevisiones.First(pr => pr.Id == revision.PeriodoRevisionId);
                var fechaActual = DateTime.Now;

                if (!periodoRevision.ActivoManual && !(periodoRevision.FechaIni <= fechaActual && fechaActual <= periodoRevision.FechaFin))
                    return BadRequest("Peak Review Period is not the current running Review Period");

                var peakRevision = await db.PeakRevision
                    .Where(pr => pr.PeakId == revision.PeakId && pr.PeriodoRevisionId == revision.PeriodoRevisionId)
                    .FirstOrDefaultAsync();

                if (peakRevision == null)
                {
                    if (peak.Estado != EstadoPeak.Standby)
                        return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");

                    peakRevision = new PeakRevision
                    {
                        PeakId = revision.PeakId,
                        PeriodoRevisionId = revision.PeriodoRevisionId,
                        Comentarios = revision.Comentarios,
                        FechaComentarios = DateTime.Today
                    };
                    db.PeakRevision.Add(peakRevision);
                }
                else
                {
                    if (peakRevision.Cerrada)
                        return BadRequest($"This Review is closed.");

                    if (!string.IsNullOrWhiteSpace(revision.Comentarios))
                    {
                        if (peak.Estado != EstadoPeak.Standby)
                            return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");

                        peakRevision.Comentarios = revision.Comentarios;
                        peakRevision.FechaComentarios = DateTime.Now;
                    }
                    else if (!string.IsNullOrWhiteSpace(revision.ComentariosJefe))
                    {
                        if (peak.Estado != EstadoPeak.Review)
                            return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                        else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                            return BadRequest("You are not the Manager of the Associate's Peak.");

                        peakRevision.ComentariosJefe = revision.ComentariosJefe;
                        peakRevision.FechaComentariosJefe = DateTime.Now;
                        peakRevision.UsuarioId = Seguridadcll.Usuario.UsuarioId;
                    }
                    db.Entry(peakRevision).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                AddLog("", peak.Id, peakRevision);

                return Ok(new
                {
                    FechaComentarios = peakRevision.FechaComentarios.ToString("yyyy-MM-dd hh:mm"),
                    FechaComentariosJefe = peakRevision.FechaComentariosJefe?.ToString("yyyy-MM-dd hh:mm"),
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EnvioRevision(int id, int periodoRevisionId)
        {
            try
            {
                var data = await db.PeakRevision
                    .Include(pr => pr.Peak)
                    .Include(pr => pr.PeriodoRevision)
                    .Include(pr => pr.PeriodoRevision.PeakObjetivosRevisiones)
                    .Include(pr => pr.Peak.UsuarioPadre)
                    .Include(pr => pr.Peak.PeakObjetivos)
                    .Include(pr => pr.Peak.Periodo)
                    .Include(pr => pr.Peak.Periodo.PeriodoRevisiones)
                    .Where(pr => pr.PeakId == id && pr.PeriodoRevisionId == periodoRevisionId)
                    .Select(pr => new
                    {
                        PeakRevision = pr,
                        pr.Peak,
                        pr.Peak.UsuarioPadre,
                        pr.Peak.Periodo.PeriodoRevisiones,
                        pr.Peak.PeakObjetivos,
                        pr.PeriodoRevision.PeakObjetivosRevisiones
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return BadRequest("Given Review does not exist.");
                else if (data.Peak.Estado != EstadoPeak.Standby)
                    return BadRequest($"This Peak is in {data.Peak.Estado.ToString().Replace("_", " ")}.");

                var periodoRevision = data.PeriodoRevisiones.First(pr => pr.Id == periodoRevisionId);
                var fechaActual = DateTime.Now;

                if (!periodoRevision.ActivoManual && !(periodoRevision.FechaIni <= fechaActual && fechaActual <= periodoRevision.FechaFin))
                    return BadRequest("Peak Review Period is not the current running Review Period");

                var objetivosLog = new List<PeakObjetivoRevision>();
                var objetivos = new List<dynamic>();
                var setValues = new Action<PeakObjetivoRevision, PeakObjetivo, bool>((por, po, valCambio) =>
                {
                    if (valCambio && !por.TieneCambios && (por.FechaMeta != po.FechaMeta || por.Objetivo != po.Objetivo || por.MedidoPor != po.MedidoPor || por.Completado != po.Completado || por.ResultadosActuales != po.ResultadosActuales || por.Comentarios != po.Comentarios))
                        por.TieneCambios = true;

                    por.PeakObjetivoId = po.Id;
                    por.PeriodoRevisionId = periodoRevisionId;
                    por.Objetivo = po.Objetivo;
                    por.FechaMeta = po.FechaMeta;
                    por.MedidoPor = po.MedidoPor;
                    por.ResultadosActuales = po.ResultadosActuales;
                    por.Comentarios = po.Comentarios;
                    por.Completado = po.Completado == null ? (short)0 : (short)po.Completado;
                    por.PeakObjetivoIdHeredado = po.PeakObjetivoId;
                });

                foreach (var po in data.PeakObjetivos)
                {
                    if (po.Estado != EstadoPeakObjetivo.Approved)
                        return BadRequest("All modified Objectives must be Approved");
                    else if (po.ResultadosActuales == null)
                        return BadRequest("All Objectives must have Actual Results");

                    var peakObjetivoRevision = data.PeakObjetivosRevisiones.FirstOrDefault(por => por.PeakObjetivoId == po.Id);
                    if (peakObjetivoRevision == null)
                    {
                        peakObjetivoRevision = new PeakObjetivoRevision();
                        setValues(peakObjetivoRevision, po, false);
                        db.PeakObjetivoRevision.Add(peakObjetivoRevision);
                    }
                    else
                    {
                        setValues(peakObjetivoRevision, po, true);
                        db.Entry(peakObjetivoRevision).State = EntityState.Modified;
                    }

                    objetivosLog.Add(peakObjetivoRevision);
                    objetivos.Add(new
                    {
                        po.Id,
                        po.Numero,
                        po.Peso,
                        po.Heredable,
                        PeakObjetivoId = peakObjetivoRevision.PeakObjetivoIdHeredado,
                        peakObjetivoRevision.Objetivo,
                        FechaMeta = peakObjetivoRevision.FechaMeta.ToString("yyyy-MM-dd"),
                        peakObjetivoRevision.MedidoPor,
                        peakObjetivoRevision.ResultadosActuales,
                        peakObjetivoRevision.Completado,
                        peakObjetivoRevision.Comentarios,
                        peakObjetivoRevision.ComentariosJefe,
                        peakObjetivoRevision.TieneCambios,
                    });
                }

                data.Peak.Estado = EstadoPeak.Review;
                data.Peak.FechaEnvio = DateTime.Now;
                db.Entry(data.Peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", data.Peak.Id, new { data.PeakRevision, Objetivos = objetivosLog });

                if (data.Peak.UsuarioIdPadre != null)
                    SendNotificationEmailTask(data.UsuarioPadre.UsuarioCorreo, data.UsuarioPadre.UsuarioNombre, data.Peak.Estado, data.Peak.Id, TipoNotificacion.Review);

                return Ok(new
                {
                    data.Peak.Estado,
                    Objetivos = objetivos
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> CancelarRevision(int id, int periodoRevisionId)
        {
            try
            {
                var data = await db.PeakRevision
                    .Include(pr => pr.Peak)
                    .Include(pr => pr.Peak.UsuarioPadre)
                    .Include(pr => pr.Peak.Periodo)
                    .Include(pr => pr.Peak.Periodo.PeriodoRevisiones)
                    .Include(pr => pr.PeriodoRevision)
                    .Include(pr => pr.PeriodoRevision.PeakObjetivosRevisiones)
                    .Where(pr => pr.PeakId == id && pr.PeriodoRevisionId == periodoRevisionId)
                    .Select(pr => new
                    {
                        PeakRevision = pr,
                        pr.Peak,
                        pr.Peak.UsuarioPadre,
                        pr.Peak.Periodo.PeriodoRevisiones,
                        pr.PeriodoRevision.PeakObjetivosRevisiones
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return BadRequest("Given Peak does not exist.");
                else if (data.Peak.Estado != EstadoPeak.Review)
                    return BadRequest($"This Peak is in {data.Peak.Estado.ToString().Replace("_", " ")}.");
                else if (data.PeakRevision.FechaComentariosJefe > data.Peak.FechaEnvio || data.PeakObjetivosRevisiones.Any(por => por.FechaComentariosJefe > data.Peak.FechaEnvio))
                    return BadRequest("Your Manager has already started the Review.");

                var periodoRevision = data.PeriodoRevisiones.First(pr => pr.Id == periodoRevisionId);
                var fechaActual = DateTime.Now;

                if (!periodoRevision.ActivoManual && !(periodoRevision.FechaIni <= fechaActual && fechaActual <= periodoRevision.FechaFin))
                    return BadRequest("Peak Review Period is not the current running Review Period");

                data.Peak.Estado = EstadoPeak.Standby;
                data.Peak.FechaEnvio = null;
                db.Entry(data.Peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", data.Peak.Id, null);

                if (data.Peak.UsuarioIdPadre != null)
                    SendNotificationEmailTask(data.UsuarioPadre.UsuarioCorreo, data.UsuarioPadre.UsuarioNombre, data.Peak.Estado, data.Peak.Id, TipoNotificacion.Undo);

                return Ok(new
                {
                    data.Peak.Id,
                    data.Peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> ComentariosObjetivoRevision(PeakObjetivoRevision objetivoRevision)
        {
            try
            {
                var data = await db.PeakObjetivoRevision
                    .Include(por => por.PeakObjetivo)
                    .Include(por => por.PeakObjetivo.Peak)
                    .Include(por => por.PeakObjetivo.Peak.Periodo)
                    .Include(por => por.PeakObjetivo.Peak.Periodo.PeriodoRevisiones)
                    .Where(por => por.PeakObjetivoId == objetivoRevision.PeakObjetivoId && por.PeriodoRevisionId == objetivoRevision.PeriodoRevisionId)
                    .Select(por => new
                    {
                        PeakObjetivoRevision = por,
                        por.PeakObjetivo.Peak,
                        por.PeakObjetivo.Peak.Periodo.PeriodoRevisiones
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return BadRequest("Given Peak does not exist.");
                else if (data.Peak.Estado != EstadoPeak.Review)
                    return BadRequest($"This Peak is in {data.Peak.Estado.ToString().Replace("_", " ")}.");

                var periodoRevision = data.PeriodoRevisiones.First(pr => pr.Id == objetivoRevision.PeriodoRevisionId);
                var fechaActual = DateTime.Now;

                if (!periodoRevision.ActivoManual && !(periodoRevision.FechaIni <= fechaActual && fechaActual <= periodoRevision.FechaFin))
                    return BadRequest("Peak Review Period is not the current running Review Period");

                data.PeakObjetivoRevision.ComentariosJefe = objetivoRevision.ComentariosJefe;
                data.PeakObjetivoRevision.FechaComentariosJefe = DateTime.Now;
                data.PeakObjetivoRevision.UsuarioId = Seguridadcll.Usuario.UsuarioId;

                db.Entry(data.PeakObjetivoRevision).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", data.PeakObjetivoRevision.Id, data.PeakObjetivoRevision);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EnvioResultadoRevision(int id, int periodoRevisionId, bool cerrar)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.Usuario)
                    .Include(p => p.Periodo.PeriodoRevisiones)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Review)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var periodoRevision = peak.Periodo.PeriodoRevisiones.First(pr => pr.Id == periodoRevisionId);
                var fechaActual = DateTime.Now;

                if (!periodoRevision.ActivoManual && !(periodoRevision.FechaIni <= fechaActual && fechaActual <= periodoRevision.FechaFin))
                    return BadRequest("Peak Review Period is not the current running Review Period");

                var peakObjetivosRevision = await db.PeakObjetivoRevision
                    .Include(por => por.PeakObjetivo)
                    .Where(por => por.PeakObjetivo.PeakId == peak.Id && por.PeriodoRevisionId == periodoRevisionId)
                    .ToListAsync();

                if (peakObjetivosRevision.Count == 0)
                    return BadRequest("Given Review does not have any Objectives.");
                else if (peakObjetivosRevision.Any(por => por.ComentariosJefe == null))
                    return BadRequest("All Objectives must have Manager's Comments.");

                var peakRevision = await db.PeakRevision.FirstOrDefaultAsync(pr => pr.PeakId == peak.Id && pr.PeriodoRevisionId == periodoRevisionId);
                if (peakRevision == null)
                    return BadRequest("Given Review does not exist.");

                if (cerrar)
                {
                    peakRevision.Cerrada = true;
                    db.Entry(peakRevision).State = EntityState.Modified;
                }

                foreach (var por in peakObjetivosRevision)
                {
                    por.TieneCambios = false;
                    db.Entry(por).State = EntityState.Modified;
                }

                peak.Estado = EstadoPeak.Standby;
                peak.FechaEnvio = null;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, peakRevision);

                SendNotificationEmailTask(peak.Usuario.UsuarioCorreo, peak.Usuario.UsuarioNombre, peak.Estado, peak.Periodo.Id, TipoNotificacion.Result);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado,
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> AbrirRevision(int id, int periodoRevisionId)
        {
            try
            {
                var data = await db.PeakRevision
                    .Include(pr => pr.Peak)
                    .Include(pr => pr.Peak.Usuario)
                    .Include(pr => pr.PeriodoRevision)
                    .Where(pr => pr.PeakId == id && pr.PeriodoRevisionId == periodoRevisionId)
                    .Select(pr => new
                    {
                        PeakRevision = pr,
                        pr.Peak.Id,
                        pr.Peak.Estado,
                        pr.Peak.PeriodoId,
                        pr.Peak.Usuario,
                        pr.Peak.UsuarioIdPadre,
                        pr.PeriodoRevision
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return BadRequest("Given Period Review does not exist.");
                else if (data.Estado != EstadoPeak.Standby)
                    return BadRequest($"This Peak is in {data.Estado.ToString().Replace("_", " ")}.");
                else if (data.UsuarioIdPadre != null && data.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!data.PeriodoRevision.ActivoManual && !(data.PeriodoRevision.FechaIni <= fechaActual && fechaActual <= data.PeriodoRevision.FechaFin))
                    return BadRequest("Peak Review Period is not the current running Review Period");

                data.PeakRevision.Cerrada = false;
                db.Entry(data.PeakRevision).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", data.Id, data.PeakRevision);

                SendNotificationEmailTask(data.Usuario.UsuarioCorreo, data.Usuario.UsuarioNombre, data.Estado, data.PeriodoId, TipoNotificacion.Opened);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> CoreValueAutoevaluacion(PeakCoreValue coreValue)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == coreValue.PeakId)
                    .Select(p => new
                    {
                        p.Estado,
                        p.Periodo,
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Standby)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Final Review Period");

                var peakCoreValue = await db.PeakCoreValue
                    .FirstOrDefaultAsync(pcv => pcv.PeakId == coreValue.PeakId && pcv.CoreValueId == coreValue.CoreValueId);

                if (peakCoreValue == null)
                {
                    peakCoreValue = new PeakCoreValue
                    {
                        PeakId = coreValue.PeakId,
                        CoreValueId = coreValue.CoreValueId,
                        Autoevaluacion = coreValue.Autoevaluacion
                    };
                    db.PeakCoreValue.Add(peakCoreValue);
                }
                else
                {
                    peakCoreValue.Autoevaluacion = coreValue.Autoevaluacion;
                    db.Entry(peakCoreValue).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                AddLog("", peakCoreValue.Id, peakCoreValue);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> AssesmentFeedback(Models.Peak model)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == model.Id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Review Period is not the current running Final Review Period");

                if (!string.IsNullOrWhiteSpace(model.ResumenContribuciones))
                {
                    if (peak.Estado != EstadoPeak.Standby)
                        return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");

                    peak.ResumenContribuciones = model.ResumenContribuciones;
                    peak.Fortalezas = model.Fortalezas;
                    peak.ObjetivosFuturo = model.ObjetivosFuturo;
                }
                else if (!string.IsNullOrWhiteSpace(model.ResumenContribucionesJefe))
                {
                    if (peak.Estado != EstadoPeak.Final_Review)
                        return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                    else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                        return BadRequest("You are not the Manager of the Associate's Peak.");

                    peak.ResumenContribucionesJefe = model.ResumenContribucionesJefe;
                    peak.FortalezasJefe = model.FortalezasJefe;
                    peak.ObjetivosFuturoJefe = model.ObjetivosFuturoJefe;
                }

                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, peak);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EnvioRevisionFinal(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.UsuarioPadre)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Standby)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (!peak.PeakObjetivos.All(po => po.Estado == EstadoPeakObjetivo.Approved))
                    return BadRequest($"All objectives must be Approved.");
                else if (string.IsNullOrEmpty(peak.ResumenContribuciones) || string.IsNullOrEmpty(peak.Fortalezas) || string.IsNullOrEmpty(peak.ObjetivosFuturo))
                    return BadRequest($"This Peak must have the Associate Self Assessment.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Final Review Period");

                peak.Estado = EstadoPeak.Final_Review;
                peak.FechaEnvio = DateTime.Now;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, null);

                if (peak.UsuarioIdPadre != null)
                    SendNotificationEmailTask(peak.UsuarioPadre.UsuarioCorreo, peak.UsuarioPadre.UsuarioNombre, peak.Estado, peak.Id, TipoNotificacion.Review);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> CancelarRevisionFinal(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.UsuarioPadre)
                    .Include(p => p.PeakCoreValues)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Final_Review)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.PeakObjetivos.Any(po => po.FechaComentariosJefe > peak.FechaEnvio) || !string.IsNullOrWhiteSpace(peak.ComentariosCompetencias) || !string.IsNullOrEmpty(peak.ResumenContribucionesJefe) || !string.IsNullOrEmpty(peak.FortalezasJefe) || !string.IsNullOrEmpty(peak.ObjetivosFuturoJefe) || !string.IsNullOrEmpty(peak.RendimientoGeneral) || peak.PeakCoreValues.Any(pcv => pcv.Evaluacion != null) || peak.FactorAjuste != 0 || !string.IsNullOrWhiteSpace(peak.JustificacionFactorAjuste))
                    return BadRequest("Your Manager has already started the Final Review.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Final Review Period");

                peak.Estado = EstadoPeak.Standby;
                peak.FechaEnvio = null;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, null);

                if (peak.UsuarioIdPadre != null)
                    SendNotificationEmailTask(peak.UsuarioPadre.UsuarioCorreo, peak.UsuarioPadre.UsuarioNombre, peak.Estado, peak.Id, TipoNotificacion.Undo);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> CoreValueEvaluacion(PeakCoreValue coreValue)
        {
            try
            {
                var data = await db.PeakCoreValue
                    .Include(pcv => pcv.Peak)
                    .Include(pcv => pcv.Peak.Periodo)
                    .Where(pcv => pcv.PeakId == coreValue.PeakId && pcv.CoreValueId == coreValue.CoreValueId)
                    .Select(pcv => new
                    {
                        PeakCoreValue = pcv,
                        pcv.Peak.Estado,
                        pcv.Peak.Periodo,
                        pcv.Peak.UsuarioIdPadre
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return BadRequest("Given Peak Core Value does not exist.");
                else if (data.Estado != EstadoPeak.Final_Review)
                    return BadRequest($"This Peak is in {data.Estado.ToString().Replace("_", " ")}.");
                else if (data.UsuarioIdPadre != null && data.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(data.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= data.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Final Review Period");

                data.PeakCoreValue.Evaluacion = coreValue.Evaluacion;
                db.Entry(data.PeakCoreValue).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", data.PeakCoreValue.Id, data.PeakCoreValue);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> ComentariosCompetencias(Models.Peak model)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == model.Id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Final_Review)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Final Review Period");

                peak.ComentariosCompetencias = model.ComentariosCompetencias;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, new { peak.ComentariosCompetencias });

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> CalificacionObjetivo(PeakObjetivo objetivo)
        {
            try
            {
                var data = await db.PeakObjetivo
                    .Include(po => po.Peak)
                    .Include(po => po.Peak.Periodo)
                    .Where(po => po.Id == objetivo.Id)
                    .Select(po => new
                    {
                        PeakObjetivo = po,
                        po.Peak.Estado,
                        po.Peak.Periodo,
                        po.Peak.UsuarioIdPadre
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return BadRequest("Given Objective does not exist.");
                else if (data.Estado != EstadoPeak.Final_Review)
                    return BadRequest($"This Peak is in {data.Estado.ToString().Replace("_", " ")}.");
                else if (data.UsuarioIdPadre != null && data.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(data.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= data.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Final Review Period");

                data.PeakObjetivo.Calificacion = objetivo.Calificacion;
                data.PeakObjetivo.Factor = objetivo.Factor;
                data.PeakObjetivo.ComentariosJefe = objetivo.ComentariosJefe;
                data.PeakObjetivo.FechaComentariosJefe = DateTime.Now;

                db.Entry(data.PeakObjetivo).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", data.PeakObjetivo.Id, data.PeakObjetivo);

                return Ok(new
                {
                    objetivo.Calificacion,
                    objetivo.Factor
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> FactorAjuste(Models.Peak model)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == model.Id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Final_Review)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Final Review Period");

                peak.FactorAjuste = model.FactorAjuste;
                peak.JustificacionFactorAjuste = string.IsNullOrWhiteSpace(model.JustificacionFactorAjuste) ? null : model.JustificacionFactorAjuste;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, new { peak.FactorAjuste, peak.JustificacionFactorAjuste });

                return Ok(new
                {
                    peak.FactorAjuste
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> RendimientoGeneral(Models.Peak model)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == model.Id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Final_Review)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Final Review Period");

                peak.RendimientoGeneral = model.RendimientoGeneral;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, new { peak.RendimientoGeneral });

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> PlanDesarrollo(PeakPlanDesarrollo planDesarrollo)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Where(p => p.Id == planDesarrollo.PeakId)
                    .Select(p => new
                    {
                        p.Estado,
                        p.Periodo,
                        p.UsuarioIdPadre,
                    })
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Development_Plan)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Final Review Period");

                PeakPlanDesarrollo peakPlanDesarrollo;

                if (planDesarrollo.Id == 0)
                {
                    peakPlanDesarrollo = planDesarrollo;
                    db.PeakPlanDesarrollo.Add(peakPlanDesarrollo);
                }
                else
                {
                    peakPlanDesarrollo = await db.PeakPlanDesarrollo.FindAsync(planDesarrollo.Id);
                    if (peakPlanDesarrollo == null)
                        return BadRequest("Given Development Plan does not exist.");

                    peakPlanDesarrollo.Area = planDesarrollo.Area;
                    peakPlanDesarrollo.Plan = planDesarrollo.Plan;
                    peakPlanDesarrollo.FechaMeta = planDesarrollo.FechaMeta;
                    peakPlanDesarrollo.ResultadoDeseado = planDesarrollo.ResultadoDeseado;

                    db.Entry(peakPlanDesarrollo).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                AddLog("", peakPlanDesarrollo.Id, peakPlanDesarrollo);

                return Ok(new { peakPlanDesarrollo.Id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> EliminarPlanDesarrollo(int id)
        {
            try
            {
                var data = await db.PeakPlanDesarrollo
                    .Include(ppd => ppd.Peak.Periodo)
                    .Include(ppd => ppd.Peak.PeakObjetivos)
                    .Where(ppd => ppd.Id == id)
                    .Select(ppd => new
                    {
                        PeakPlanDesarrollo = ppd,
                        ppd.Peak.Estado,
                        ppd.Peak.Periodo,
                        ppd.Peak.UsuarioIdPadre,
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                    return BadRequest("Given Peak does not exist.");
                else if (data.Estado != EstadoPeak.Development_Plan)
                    return BadRequest($"This Peak is in {data.Estado.ToString().Replace("_", " ")}.");
                else if (data.UsuarioIdPadre != null && data.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(data.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= data.Periodo.FechaFin))
                    return BadRequest("Peak Period is not the current running Final Review Period");

                db.PeakPlanDesarrollo.Remove(data.PeakPlanDesarrollo);

                await db.SaveChangesAsync();
                AddLog("", data.PeakPlanDesarrollo.Id, data.PeakPlanDesarrollo);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> Finalizar(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.Usuario)
                    .Include(p => p.PeakPlanesDesarrollo)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Development_Plan)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.PeakPlanesDesarrollo.Count == 0)
                    return BadRequest($"There must be at least one Development Plan.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Final Review Period");

                peak.Estado = EstadoPeak.Finished;
                peak.FechaEnvio = null;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, peak);

                SendNotificationEmailTask(peak.Usuario.UsuarioCorreo, peak.Usuario.UsuarioNombre, peak.Estado, peak.Periodo.Id, TipoNotificacion.Finished);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Peak/Index")]
        public async Task<IHttpActionResult> FinalizarRevisionFinal(int id)
        {
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.Usuario)
                    .Include(p => p.PeakObjetivos)
                    .Include(p => p.PeakCoreValues)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (peak == null)
                    return BadRequest("Given Peak does not exist.");
                else if (peak.Estado != EstadoPeak.Final_Review)
                    return BadRequest($"This Peak is in {peak.Estado.ToString().Replace("_", " ")}.");
                else if (peak.PeakObjetivos.Any(po => po.Calificacion == null))
                    return BadRequest($"All objectives must be rated.");
                else if (peak.PeakCoreValues.Any(pcv => pcv.Evaluacion == null))
                    return BadRequest($"All Core Values must have Manager's Assessment.");
                else if (string.IsNullOrEmpty(peak.ComentariosCompetencias))
                    return BadRequest($"Manager's Comments - Competencies required.");
                else if (string.IsNullOrEmpty(peak.ResumenContribucionesJefe) || string.IsNullOrEmpty(peak.FortalezasJefe) || string.IsNullOrEmpty(peak.ObjetivosFuturoJefe))
                    return BadRequest($"Manager Feedback required.");
                else if (string.IsNullOrEmpty(peak.RendimientoGeneral))
                    return BadRequest($"Overall Performance Rating required.");
                else if (peak.UsuarioIdPadre != null && peak.UsuarioIdPadre != Seguridadcll.Usuario.UsuarioId)
                    return BadRequest("You are not the Manager of the Associate's Peak.");

                var fechaActual = DateTime.Now;
                if (!(peak.Periodo.RevisionFinalFechaIni <= fechaActual && fechaActual <= peak.Periodo.FechaFin))
                    return BadRequest("Given Period is not the current running Final Review Period");

                peak.Estado = EstadoPeak.Development_Plan;
                db.Entry(peak).State = EntityState.Modified;

                await db.SaveChangesAsync();
                AddLog("", peak.Id, peak);

                SendNotificationEmailTask(peak.Usuario.UsuarioCorreo, peak.Usuario.UsuarioNombre, peak.Estado, peak.Periodo.Id, TipoNotificacion.DP);

                return Ok(new
                {
                    peak.Id,
                    peak.Estado
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
