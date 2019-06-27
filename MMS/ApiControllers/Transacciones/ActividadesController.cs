using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;

namespace MMS.ApiControllers.Transacciones
{
    public class ActividadesController : ApiBaseController
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

                string ActividadId = form["_actividadid"];
                string ActividadTitulo = form["_actividadtitulo"];
                string ClienteID = form["_clienteid"];
                string UsuarioIdElabora = form["_usuarioidelabora"];
                string ActividadEstado = form["_actividadestado"];
                string ActividadFecha = form["_actividadfecha"];


                //string search = form["search[value]"];
                var clienteList = Seguridadcll.ClienteList.Select(c => c.ClienteID).ToArray();
                var countQuery = db.Actividad.Include(u => u.cliente)
                    .Select(a => new { a.PlantaID, a.ActividadId, a.ActividadTitulo, a.ClienteID, Cliente = a.ClienteID + " - " + a.cliente.ClienteRazonSocial, a.UsuarioIdElabora, ActividadEstado = a.ActividadEstado.ToString(), a.ActividadFecha })
                    .Where(a => clienteList.Contains(a.ClienteID));
                var dataQuery = db.Actividad.Include(u => u.cliente)
                    .Select(a => new { a.PlantaID, a.ActividadId, a.ActividadTitulo, a.ClienteID, Cliente = a.ClienteID + " - " + a.cliente.ClienteRazonSocial, a.UsuarioIdElabora, ActividadEstado = a.ActividadEstado.ToString(), a.ActividadFecha })
                   .Where(a => clienteList.Contains(a.ClienteID));

                if (!string.IsNullOrWhiteSpace(ActividadId))
                {
                    string value = ActividadId.Trim();
                    countQuery = countQuery.Where(id => id.ActividadId.ToString().Contains(value));
                    dataQuery = dataQuery.Where(id => id.ActividadId.ToString().Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(ActividadTitulo))
                {
                    string value = ActividadTitulo.Trim();
                    countQuery = countQuery.Where(id => id.ActividadTitulo.Contains(value));
                    dataQuery = dataQuery.Where(id => id.ActividadTitulo.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(ClienteID))
                {
                    string value = ClienteID.Trim();
                    countQuery = countQuery.Where(id => id.Cliente.Contains(value));
                    dataQuery = dataQuery.Where(id => id.Cliente.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(UsuarioIdElabora))
                {
                    string value = UsuarioIdElabora.Trim();
                    countQuery = countQuery.Where(id => id.UsuarioIdElabora.Contains(value));
                    dataQuery = dataQuery.Where(id => id.UsuarioIdElabora.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(ActividadEstado))
                {
                    string value = ActividadEstado.Trim();
                    countQuery = countQuery.Where(id => id.ActividadEstado.Contains(value));
                    dataQuery = dataQuery.Where(id => id.ActividadEstado.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(ActividadFecha))
                {
                    string value = ActividadFecha.Trim();
                    countQuery = countQuery.Where(id => id.ActividadFecha.ToString().Contains(value));
                    dataQuery = dataQuery.Where(id => id.ActividadFecha.ToString().Contains(value));
                }

                int count = await countQuery.CountAsync();

                var data = await dataQuery
                    .OrderByDescending(a => a.ActividadId)
                    .Skip(displayStart).Take(displayLength).ToListAsync();

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = data.Select(a => new { a.ActividadId, a.ActividadTitulo, a.Cliente, a.UsuarioIdElabora, a.ActividadEstado, ActividadFecha = a.ActividadFecha.ToString("d") })
                });

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // return InternalServerError("");
        }

        [HttpGet]
        public async Task<IHttpActionResult> BuscarCliente(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Cliente>());
                var clienteList = Seguridadcll.ClienteList.Select(c => c.ClienteID).ToArray();
                return Ok(await db.Clientes
                    .Include(i => i.canal)
                    .Include(i => i.ciudad.departamentos.paises)
                    .Include(i => i.usuario)
                    .Where(i => (i.ClienteID.Contains(q) || i.ClienteRazonSocial.Contains(q)))
                    .Select(i => new
                    {
                        i.ClienteID,
                        i.ClienteRazonSocial,
                        Zona = i.ciudad.departamentos.paises.PaisDesc + "-" + i.ciudad.departamentos.DepartamentoDesc + "-" + i.ciudad.CiudadDesc,
                        Canal = i.canal.CanalDesc,
                        CanalId = i.canal.CanalID,
                        Vendedor = i.usuario.UsuarioNombre,
                        i.PlantaID
                    })
                    .Where(c => clienteList.Contains(c.ClienteID))
                    .Take(50)
                    .ToListAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetCliente(string id)
        {
            try
            {

                return Ok(await db.Clientes
                    .Include(i => i.canal)
                    .Include(i => i.ciudad.departamentos.paises)
                    .Include(i => i.usuario)
                    .Where(i => (i.ClienteID == id))
                    .Select(i => new
                    {
                        i.ClienteID,
                        i.ClienteRazonSocial,
                        Zona = i.ciudad.departamentos.paises.PaisDesc + "-" + i.ciudad.departamentos.DepartamentoDesc + "-" + i.ciudad.CiudadDesc,
                        Canal = i.canal.CanalDesc,
                        CanalId = i.canal.CanalID,
                        Vendedor = i.usuario.UsuarioNombre,
                        i.PlantaID
                    })
                    .FirstOrDefaultAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> BuscarProducto(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Producto>());

                return Ok(await db.Producto
                    .Include(p => p.tipoProducto)
                    .Where(p => (p.ProductoId.Contains(q) || p.ProductoDesc.Contains(q)))
                    .Select(p => new
                    {
                        p.ProductoId,
                        p.ProductoDesc,
                        p.ProductoPrecio,
                        TipoProducto = p.tipoProducto.TipoProductoDesc
                    })
                    .Take(50)
                    .ToListAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetProducto(string id)
        {
            try
            {

                return Ok(await db.Producto
                    .Include(p => p.tipoProducto)
                    .Where(p => (p.ProductoId == id))
                    .Select(p => new
                    {
                        p.ProductoId,
                        p.ProductoDesc,
                        p.ProductoPrecio,
                        TipoProducto = p.tipoProducto.TipoProductoDesc
                    })

                    .FirstOrDefaultAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        //public async Task<IHttpActionResult> GetPresupuesto(string ClienteId, DateTime Fecha, int ActividadId)
        [HttpGet]
        public async Task<IHttpActionResult> GetPresupuesto(string CanalId, string PlantaId, DateTime Fecha, int ActividadId)
        {
            try
            {
                int Year = Fecha.Year;
                int Month = Fecha.Month;
                int quartile = 0;

                if (Month >= 1 && Month <= 3)
                    quartile = 1;
                else if (Month >= 4 && Month <= 6)
                    quartile = 2;
                else if (Month >= 7 && Month <= 9)
                    quartile = 3;
                else if (Month >= 10 && Month <= 12)
                    quartile = 4;

                var presupuesto = new List<PresupuestoData>();

                var CentrosCostos = await db.CentroCostos.Select(cc => cc.CentroCostoID).ToListAsync();

                foreach (var item in CentrosCostos)
                {

                    decimal GastoTemp = 0;

                    var ActividadItems = await db.ActividadItem.Include(ai => ai.Actividad)
                        .Where(ai => ai.Actividad.PlantaID == PlantaId && ai.Actividad.CanalID == CanalId && ai.CentroCostoID == item && ai.Actividad.ActividadId != ActividadId && (ai.Actividad.ActividadEstado == EstadosActividades.Abierto ||
                                                        ai.Actividad.ActividadEstado == EstadosActividades.Rechazado || ai.Actividad.ActividadEstado == EstadosActividades.Pendiente ||
                                                       /* ai.Actividad.ActividadEstado == EstadosActividades.PendienteTrade ||*/ ai.Actividad.ActividadEstado == EstadosActividades.Autorizado)
                                                        && ai.Actividad.ActividadFecha.Year == Year)
                        //    && ((quartile == 1 && ai.Actividad.ActividadFecha.Month >= 1 && ai.Actividad.ActividadFecha.Month <= 3)
                        //|| (quartile == 2 && ai.Actividad.ActividadFecha.Month >= 4 && ai.Actividad.ActividadFecha.Month <= 6)
                        //|| (quartile == 3 && ai.Actividad.ActividadFecha.Month >= 7 && ai.Actividad.ActividadFecha.Month <= 9)
                        //|| (quartile == 4 && ai.Actividad.ActividadFecha.Month >= 10 && ai.Actividad.ActividadFecha.Month <= 12))
                        .ToListAsync();

                    ActividadItems.ForEach(i =>
                        GastoTemp += (i.ActividadItemPrecio * i.ActividadItemCantidad) ?? 0
                    );

                    var result = await db.PresupuestoVendedor.Include(pv => pv.centroCosto)
                                    .Where(p => p.PlantaID == PlantaId && p.CanalID == CanalId &&
                                                p.PresupuestoVendedorAno == Year &&
                                                //p.PresupuestoVendedorMes == quartile &&
                                                p.CentroCostoID == item).ToListAsync();

                    var presupuestoTemp = (from r in result
                                           select new PresupuestoData
                                           {
                                               presupuesto = r.PresupuestoValor ?? 0,
                                               gasto = (r.PresupuestoGasto ?? 0) + GastoTemp,
                                               centroCostoId = r.CentroCostoID,
                                               centroCostoDesc = r.centroCosto.CentroCostoDesc
                                           }).FirstOrDefault();

                    if (presupuestoTemp != null)
                        presupuesto.Add(presupuestoTemp);

                }

                return Ok(presupuesto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public class PresupuestoData
        {
            public decimal presupuesto { get; set; }
            public decimal gasto { get; set; }
            public string centroCostoId { get; set; }
            public string centroCostoDesc { get; set; }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Approve(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);

                var countQuery = db.ActividadAutorizacion
                                    .Include(a => a.Actividad.ActividadItemList)
                                    .Include(a => a.Actividad.cliente)
                                    .Where(a => a.UsuarioIdAutoriza == Seguridadcll.Usuario.UsuarioId && a.ActividadAutorizacionAutoriza == EstadoAutorizaActividad.Por_Autorizar);
                var dataQuery = db.ActividadAutorizacion
                                    .Include(a => a.Actividad.ActividadItemList)
                                    .Include(a => a.Actividad.cliente)
                                    .Where(a => a.UsuarioIdAutoriza == Seguridadcll.Usuario.UsuarioId && a.ActividadAutorizacionAutoriza == EstadoAutorizaActividad.Por_Autorizar);

                if ((Seguridadcll.Usuario.UsuarioAprobadorPrincipal ?? false))
                {
                    countQuery = countQuery.Where(a => /*a.Actividad.ActividadEstado == EstadosActividades.PendienteTrade ||*/ a.Actividad.ActividadEstado == EstadosActividades.Pendiente);
                    dataQuery = countQuery.Where(a => /*a.Actividad.ActividadEstado == EstadosActividades.PendienteTrade ||*/ a.Actividad.ActividadEstado == EstadosActividades.Pendiente);
                }
                else
                {
                    countQuery = countQuery.Where(a => a.Actividad.ActividadEstado == EstadosActividades.Pendiente);
                    dataQuery = countQuery.Where(a => a.Actividad.ActividadEstado == EstadosActividades.Pendiente);
                }


                int count = await countQuery.CountAsync();

                var data = await dataQuery
                    .OrderByDescending(a => a.ActividadId)
                    .Skip(displayStart).Take(displayLength).ToListAsync();

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = data.Select(a => new
                    {
                        ActividadAutorizacionAutoriza = a.ActividadAutorizacionAutoriza.ToString(),
                        a.ActividadId,
                        a.Actividad.ActividadTitulo,
                        ActividadFecha = a.Actividad.ActividadFecha.ToString("d"),
                        ClienteID = a.Actividad.ClienteID + " - " + a.Actividad.cliente.ClienteRazonSocial,
                        a.Actividad.UsuarioIdElabora,
                        Total = a.Actividad.ActividadItemList.Sum(ai => (ai.ActividadItemPrecio * ai.ActividadItemCantidad)) ?? 0,
                        ActividadAutorizacionFecha = a.ActividadAutorizacionFecha.ToString("d"),
                        a.ActividadAutorizacionMotivo
                    })
                });

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Duplicar(int id)
        {
            try
            {
                AjaxResult result = new AjaxResult();

                var actividad = await db.Actividad.FindAsync(id);

                if (actividad == null)
                    return Ok(result.False("Actividad no encontrada"));

                var actividadNew = new Actividad();
                actividadNew = actividad;
                
                actividadNew.ActividadCuenta = actividad.ActividadCuenta;                
                actividadNew.ActividadDesc = actividad.ActividadDesc;
                actividadNew.ActividadObjetivo = actividad.ActividadObjetivo;    
                actividadNew.ActividadFechaAprob = DateTime.Now;
                actividadNew.ActividadFechaDesde = DateTime.Now.AddDays(1);
                actividadNew.ActividadFechaHasta = DateTime.Now.AddDays(2);
                actividadNew.ActividadMetaV = actividad.ActividadMetaV;
                actividadNew.ActividadMetaE = actividad.ActividadMetaE;
                actividadNew.ActividadUltimoItem = actividad.ActividadUltimoItem;
                actividadNew.UsuarioIdElabora = actividad.UsuarioIdElabora;
                actividadNew.CanalID = actividad.CanalID;
                actividadNew.PlantaID = actividad.PlantaID;
                actividadNew.GastoId = actividad.GastoId;
                actividadNew.TipoActividadID = actividad.TipoActividadID;
                actividadNew.ClienteID = actividad.ClienteID;
                actividadNew.ActividadLugarEnvioPOP = actividad.ActividadLugarEnvioPOP;
                actividadNew.Marcas = actividad.Marcas;
                actividadNew.ActividadEstado = EstadosActividades.Abierto;
                actividadNew.ActividadFecha = DateTime.Now;
                actividadNew.ActividadFechaMod = DateTime.Now;
                actividadNew.ActividadTitulo ="Copia " + actividad.ActividadTitulo;
                actividadNew.CumplimientoTotal = 0;
                actividadNew.CumplimientoPorcentaje = 0;
                actividadNew.Resultado = "";
                actividadNew.EstadoCierre = EstadoCierreActividad.No_Cumplio;
                db.Actividad.Add(actividadNew);

                await db.SaveChangesAsync();

                var items = await db.ActividadItem.Where(ai => ai.ActividadId == id).ToListAsync();

                foreach(var item in items)
                {
                    var itemNew = new ActividadItem();
                    
                    itemNew.ActividadId = actividadNew.ActividadId;
                    itemNew.ActividadItemCantidad = item.ActividadItemCantidad;
                    itemNew.ActividadItemProducto = item.ActividadItemProducto;
                    itemNew.ActividadItemPrecio = item.ActividadItemPrecio;
                    itemNew.ActividadItemDescripcion = item.ActividadItemDescripcion;
                    itemNew.ProductoId = item.ProductoId;
                    itemNew.CentroCostoID = item.CentroCostoID;
                    

                    db.ActividadItem.Add(itemNew);                    
                }

                await db.SaveChangesAsync();

                AddLog("", actividadNew.ActividadId, actividadNew);

                return Ok(result.True("Actividad duplicada #: " + actividadNew.ActividadId));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
