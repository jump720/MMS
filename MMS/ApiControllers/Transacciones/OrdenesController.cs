using MMS.ApiControllers;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.Entity;

namespace MMS.ApiControllers.Transacciones
{
    public class OrdenesController : ApiBaseController
    {
        private MMSContext db = new MMSContext();

        [HttpPost]
        [ApiAuthorizeAction]
        public IHttpActionResult Index(FormDataCollection form)
        {
            try
            {
                string sEcho = form["draw"].ToString();//pagina
                int iDisplayStart = Convert.ToInt32(form["Start"]);//numero de objeto a esconder
                int iDisplayLength = Convert.ToInt32(form["Length"]);//tamaño de la grilla
                string sSearch = form["search[value]"].ToString().ToLower();//filtro smart (global)

                List<Orden> OrdenesList = new List<Orden>();
                var Count = 0;
                if (HttpContext.Current.Session != null)
                {


                    try
                    {
                        if (!string.IsNullOrEmpty(sSearch))
                        {
                            OrdenesList = db.Orden.Include(o => o.actividad.cliente).Include(o => o.usuario).ToList()
                                            .Where(o => o.OrdenId.ToString().Contains(sSearch) ||
                                                        (o.OrdenFecha != null && o.OrdenFecha.ToString("d").ToLower().Contains(sSearch)) ||
                                                        (o.OrdenFechaDespacho != null && o.OrdenFechaDespacho.ToString("d").ToLower().Contains(sSearch)) ||
                                                        (!string.IsNullOrEmpty(o.OrdenEstado.ToString()) && o.OrdenEstado.ToString().ToLower().Contains(sSearch)) ||
                                                        (!string.IsNullOrEmpty(o.ActividadId.ToString()) && o.ActividadId.ToString().ToLower().Contains(sSearch)) ||
                                                        (!string.IsNullOrEmpty(o.actividad.cliente.ClienteRazonSocial.ToString()) && o.actividad.cliente.ClienteRazonSocial.ToString().ToLower().Contains(sSearch)) ||
                                                        (!string.IsNullOrEmpty(o.OrdenNroGuia.ToString()) && o.OrdenNroGuia.ToString().ToLower().Contains(sSearch)) ||
                                                        (!string.IsNullOrEmpty(o.usuario.UsuarioNombre.ToString()) && o.usuario.UsuarioNombre.ToString().ToLower().Contains(sSearch))
                                                        ).ToList();
                        }
                        else
                        {
                            OrdenesList = db.Orden.Include(o => o.actividad.cliente).Include(o => o.usuario).ToList();
                        }//if (!string.IsNullOrEmpty(sSearch))

                    }//try
                    catch
                    {
                        OrdenesList = new List<Orden>();
                    }//catch
                }//if (HttpContext.Current.Session != null)

                Count = OrdenesList.Count;
                OrdenesList = OrdenesList.OrderByDescending(o => o.OrdenFecha)
                                               .Skip(iDisplayStart).Take(iDisplayLength).ToList();

                var CustomerPaged = new SysDataTablePager();

                CustomerPaged.draw = sEcho;
                CustomerPaged.recordsTotal = Count;
                CustomerPaged.recordsFiltered = Count;
                CustomerPaged.data = OrdenesList.Select(o => new { o.OrdenId, o.OrdenFecha, o.OrdenFechaDespacho, o.OrdenEstado, o.ActividadId, o.actividad.cliente.ClienteRazonSocial, o.OrdenNroGuia, o.usuario.UsuarioNombre });

                return Ok(CustomerPaged);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
