using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.Owin.Infrastructure;
using MMS.Classes;
using System.Web;
using System.Net.Http.Formatting;

namespace MMS.ApiControllers.PerfectStore
{
    [RoutePrefix("api/Visitas")]
    public class VisitasController : ApiAppBaseController
    {
        private MMSContext db = new MMSContext();



        [HttpPost]
        [Route("Create/{reuploadVisitaId:int}")]
        public async Task<IHttpActionResult> Create(Visita visita, int reuploadVisitaId)
        {
            try
            {
                if (reuploadVisitaId != 0)
                {
                    var oldVisit = await db.Visita.FindAsync(reuploadVisitaId);
                    db.Visita.Remove(oldVisit);
                }

                if (string.IsNullOrWhiteSpace(visita.Administrador))
                    visita.Administrador = null;

                if (string.IsNullOrWhiteSpace(visita.Telefono))
                    visita.Telefono = null;

                if (string.IsNullOrWhiteSpace(visita.Barrio))
                    visita.Barrio = null;

                visita.FechaConfirmacion = DateTime.Now;
                visita.Activa = true;
                visita.Completada = false;
                db.Visita.Add(visita);

                await db.SaveChangesAsync();
                AddLog("VisitasCreate", visita.Id, visita);

                return Ok(new AjaxResult() { Data = visita.Id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("Complete/{id:int}/{lastVisitaId:int}")]
        public async Task<IHttpActionResult> Complete(int id, int lastVisitaId)
        {
            try
            {
                if (lastVisitaId != 0)
                {
                    var lastVisita = await db.Visita.FirstOrDefaultAsync(v => v.Id == lastVisitaId);
                    if (lastVisita != null)
                    {
                        lastVisita.Activa = false;
                        db.Entry(lastVisita).State = EntityState.Modified;
                    }
                }

                var visita = await db.Visita.FindAsync(id);
                visita.Completada = true;

                await db.SaveChangesAsync();
                AddLog("VisitasComplete", visita.Id, new { VisitaIdAnterior = lastVisitaId });

                return Ok(new AjaxResult() { Data = visita.Id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("AddItems")]
        public async Task<IHttpActionResult> AddItems(List<VisitaItem> visitaItems)
        {
            try
            {
                if (visitaItems.Count > 0)
                {
                    db.VisitaItem.AddRange(visitaItems);
                    await db.SaveChangesAsync();
                    AddLog("VisitasAddItems", visitaItems[0].VisitaId, new { Items = visitaItems });
                }
                return Ok(new AjaxResult());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("AddClientes")]
        public async Task<IHttpActionResult> AddClientes(List<VisitaCliente> visitaClientes)
        {
            try
            {
                if (visitaClientes.Count > 0)
                {
                    db.VisitaCliente.AddRange(visitaClientes);
                    await db.SaveChangesAsync();
                    AddLog("VisitasAddClientes", visitaClientes[0].VisitaId, new { Clientes = visitaClientes });
                }
                return Ok(new AjaxResult());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("AddFoto")]
        public async Task<IHttpActionResult> AddFoto(VisitaFoto visitaFoto)
        {
            try
            {
                db.VisitaFoto.Add(visitaFoto);
                await db.SaveChangesAsync();
                visitaFoto.Foto = null;
                AddLog("VisitasAddFoto", visitaFoto.VisitaId, visitaFoto);

                return Ok(new AjaxResult());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("AddPublicidad")]
        public async Task<IHttpActionResult> AddPublicidad(VisitaPublicidad visitaPublicidad)
        {
            try
            {
                db.VisitaPublicidad.Add(visitaPublicidad);
                await db.SaveChangesAsync();
                visitaPublicidad.Foto = null;
                AddLog("VisitasAddPublicidad", visitaPublicidad.VisitaId, visitaPublicidad);

                return Ok(new AjaxResult());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("BuscarEstablecimiento/{paisId}/{departamentoId}/{ciudadId}/{nombre}")]
        public async Task<IHttpActionResult> BuscarEstablecimiento(string paisId, string departamentoId, string ciudadId, string nombre)
        {
            try
            {
                var data = await db.Visita
                    .Include(v => v.Ciudad)
                    .Include(v => v.Ciudad.departamentos)
                    .Include(v => v.Ciudad.departamentos.paises)
                    .Include(v => v.Usuario)
                    .Where(v =>
                        (paisId == "---" || v.PaisId == paisId) &&
                        (departamentoId == "---" || v.DepartamentoId == departamentoId) &&
                        (ciudadId == "---" || v.CiudadId == ciudadId) &&
                        v.NombreEstablecimiento.Contains(nombre) && v.Activa)
                    .Take(100)
                    .Select(v => new
                    {
                        v.Id,
                        v.NombreEstablecimiento,
                        v.Direccion,
                        Ciudad = v.Ciudad.CiudadDesc,
                        Departamento = v.Ciudad.departamentos.DepartamentoDesc,
                        Pais = v.Ciudad.departamentos.paises.PaisDesc,
                        v.Fecha,
                        Usuario = v.Usuario.UsuarioNombre,
                        v.Longitud,
                        v.Latitud
                    })
                    .ToListAsync();

                return Ok(new AjaxResult { Data = data });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Base/{id:int}")]
        public async Task<IHttpActionResult> Base(int id)
        {
            try
            {
                var visita = await db.Visita.FirstOrDefaultAsync(v => v.Id == id);
                return Ok(new AjaxResult { Data = visita });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("Get")]
        public async Task<IHttpActionResult> Get(List<int> lastId)
        {
            try
            {
                var fechas = new List<DateTime>();
                var fechaConf = await db.Configuracion.Select(c => c.ConfigFechaActualizacionDatosBase).FirstOrDefaultAsync();
                fechas.Add((DateTime)fechaConf);
                fechas.Add(DateTime.Now);

                int min = lastId.Count == 0 ? 0 : lastId.Min();
                var visitas = await db.Visita
                    .Where(v => v.Id > min && v.UsuarioId == Usuario.UsuarioId && v.Completada)
                    .OrderByDescending(v => v.Fecha)
                    .Take(20)
                    .ToListAsync();

                if (lastId.Count > 0)
                    visitas = visitas.Where(v => !lastId.Contains(v.Id)).ToList();

                return Ok(new AjaxResult { Data = new { Fechas = fechas, Visitas = visitas } });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Load/{id:int}")]
        public async Task<IHttpActionResult> Load(int id)
        {
            try
            {
                var data = await db.Visita
                    .Include(v => v.VisitaFotos)
                    .Include(v => v.VisitaItems)
                    .Include(v => v.VisitaClientes)
                    .Include(v => v.VisitaPublicidades)
                    .Where(v => v.Id == id)
                    .Select(v => new
                    {
                        Fotos = v.VisitaFotos.Select(f => new
                        {
                            f.Order
                        }).ToList(),
                        Items = v.VisitaItems.Select(i => new
                        {
                            i.Order,
                            i.ItemId,
                            i.PrecioVenta
                        }).ToList(),
                        Clientes = v.VisitaClientes.Select(c => new
                        {
                            c.Order,
                            c.ClienteId,
                            c.NroCompras,
                            c.ValorCompras,
                            c.ValorVentas
                        }).ToList(),
                        Publicidad = v.VisitaPublicidades.Select(p => new
                        {
                            p.Order,
                            TipoId = p.Tipo,
                            p.MarcaId,
                            p.Nivel,
                            FileName = p.MediaType
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                return Ok(new AjaxResult { Data = data });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Markers")]
        public async Task<IHttpActionResult> Markers(double neLat, double neLon, double swLat, double swLon)
        {
            try
            {
                var markers = await db.Visita
                    .Include(v => v.Usuario)
                    .Where(v => v.Activa && v.Completada &&
                        neLat > v.Latitud && swLat < v.Latitud &&
                        neLon > v.Longitud && swLon < v.Longitud)
                    .Select(v => new
                    {
                        v.Id,
                        Name = v.NombreEstablecimiento,
                        Addr = v.Direccion,
                        Date = v.Fecha,
                        User = v.Usuario.UsuarioNombre,
                        Lat = v.Latitud,
                        Lon = v.Longitud
                    }).ToListAsync();

                return Ok(new AjaxResult { Data = markers });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("GetById/{id:int}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            try
            {
                var visita = await db.Visita.FirstOrDefaultAsync(v => v.Id == id);
                return Ok(new AjaxResult { Data = visita });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("BuscarVisita")]
        public async Task<IHttpActionResult> BuscarVisita(BusquedaVisita busqueda)
        {
            try
            {
                busqueda.FechaIni = new DateTime(busqueda.FechaIni.Year, busqueda.FechaIni.Month, busqueda.FechaIni.Day, 0, 0, 0);
                busqueda.FechaFin = new DateTime(busqueda.FechaFin.Year, busqueda.FechaFin.Month, busqueda.FechaFin.Day, 23, 59, 59);

                var data = await db.Visita
                    .Include(v => v.Ciudad)
                    .Include(v => v.Ciudad.departamentos)
                    .Include(v => v.Ciudad.departamentos.paises)
                    .Include(v => v.Usuario)
                    .Where(v =>
                        v.Completada &&
                        (busqueda.PaisId == null || v.PaisId == busqueda.PaisId) &&
                        (busqueda.DepartamentoId == null || v.DepartamentoId == busqueda.DepartamentoId) &&
                        (busqueda.CiudadId == null || v.CiudadId == busqueda.CiudadId) &&
                        (busqueda.UsuarioId == null || v.UsuarioId == busqueda.UsuarioId) &&
                        v.Fecha >= busqueda.FechaIni && v.Fecha <= busqueda.FechaFin &&
                        v.NombreEstablecimiento.Contains(busqueda.NombreEstablecimiento))
                    .Select(v => new
                    {
                        v.Id,
                        v.NombreEstablecimiento,
                        v.Direccion,
                        Ciudad = v.Ciudad.CiudadDesc,
                        Departamento = v.Ciudad.departamentos.DepartamentoDesc,
                        Pais = v.Ciudad.departamentos.paises.PaisDesc,
                        v.Fecha,
                        Usuario = v.Usuario.UsuarioNombre,
                        v.Longitud,
                        v.Latitud
                    })
                    .ToListAsync();

                return Ok(new AjaxResult { Data = data });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public class BusquedaVisita
        {
            public string NombreEstablecimiento { get; set; }
            public string PaisId { get; set; }
            public string DepartamentoId { get; set; }
            public string CiudadId { get; set; }
            public string UsuarioId { get; set; }
            public DateTime FechaIni { get; set; }
            public DateTime FechaFin { get; set; }
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
