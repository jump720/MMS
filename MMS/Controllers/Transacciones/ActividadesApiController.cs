using MMS.Classes;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MMS.Controllers.Transacciones
{
    public class ActividadesApiController : ApiController
    {
        private MMSContext db = new MMSContext();
        [HttpPost]
        public SysDataTablePager Get(FormDataCollection form)
        {
            //NameValueCollection nvc = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            string sEcho = form["draw"].ToString();//pagina
            int iDisplayStart = Convert.ToInt32(form["Start"]);//numero de objeto a esconder
            int iDisplayLength = Convert.ToInt32(form["Length"]);//tamaño de la grilla
            string sSearch = form["search[value]"].ToString().ToLower();//filtro smart (global)

            dynamic ActividadesList;
            var Count = 0;
            if (HttpContext.Current.Session != null)
            {
                Seguridadcll seguridadcll = (Seguridadcll)HttpContext.Current.Session["seguridad"];
                var clienteList = seguridadcll.ClienteList.Select(c => c.ClienteID).ToArray();
                try
                {
                    if (!string.IsNullOrEmpty(sSearch))
                    {
                        ActividadesList = (from a in db.Actividad
                                           join u in db.Usuarios on a.UsuarioIdElabora equals u.UsuarioId
                                           join c in db.Clientes on a.ClienteID equals c.ClienteID
                                           select new { a.ActividadId, a.ActividadFecha, a.ActividadEstado, a.ActividadTitulo, c.ClienteID, a.cliente.ClienteRazonSocial, a.UsuarioIdElabora, u.UsuarioNombre })
                                           .ToList()
                                           .Where(a => clienteList.Contains(a.ClienteID) == true && a.ActividadId.ToString().Contains(sSearch) ||
                                           (a.ActividadFecha != null && a.ActividadFecha.ToString("d").ToLower().Contains(sSearch)) ||
                                           (!string.IsNullOrEmpty(a.ActividadEstado.ToString()) && a.ActividadEstado.ToString().ToLower().Contains(sSearch)) ||
                                           (!string.IsNullOrEmpty(a.ActividadTitulo) && a.ActividadTitulo.ToLower().Contains(sSearch)) ||
                                           (!string.IsNullOrEmpty(a.ClienteRazonSocial) && a.ClienteRazonSocial.ToLower().Contains(sSearch)) ||
                                           (!string.IsNullOrEmpty(a.UsuarioIdElabora) && a.UsuarioIdElabora.ToLower().Contains(sSearch)))
                                            .OrderByDescending(a => a.ActividadId)
                                            .Skip(iDisplayStart).Take(iDisplayLength).ToList();
                        Count = ((from a in db.Actividad
                                  join u in db.Usuarios on a.UsuarioIdElabora equals u.UsuarioId
                                  join c in db.Clientes on a.ClienteID equals c.ClienteID
                                  select new { a.ActividadId, a.ActividadFecha, a.ActividadEstado, a.ActividadTitulo, c.ClienteID, a.cliente.ClienteRazonSocial, a.UsuarioIdElabora })
                                           .ToList()
                                           .Where(a => clienteList.Contains(a.ClienteID) == true && a.ActividadId.ToString().Contains(sSearch) ||
                                           (a.ActividadFecha != null && a.ActividadFecha.ToString("d").ToLower().Contains(sSearch)) ||
                                           (!string.IsNullOrEmpty(a.ActividadEstado.ToString()) && a.ActividadEstado.ToString().ToLower().Contains(sSearch)) ||
                                           (!string.IsNullOrEmpty(a.ActividadTitulo) && a.ActividadTitulo.ToLower().Contains(sSearch)) ||
                                           (!string.IsNullOrEmpty(a.ClienteRazonSocial) && a.ClienteRazonSocial.ToLower().Contains(sSearch)) ||
                                           (!string.IsNullOrEmpty(a.UsuarioIdElabora) && a.UsuarioIdElabora.ToLower().Contains(sSearch)))
                                            .OrderByDescending(a => a.ActividadId)
                                            .ToList()).Count;
                    }
                    else
                    {
                        ActividadesList = (from a in db.Actividad
                                           join u in db.Usuarios on a.UsuarioIdElabora equals u.UsuarioId
                                           join c in db.Clientes on a.ClienteID equals c.ClienteID
                                           select new { a.ActividadId, a.ActividadFecha, a.ActividadEstado, a.ActividadTitulo, c.ClienteID, a.cliente.ClienteRazonSocial, a.UsuarioIdElabora, u.UsuarioNombre })
                                           .Where(a => clienteList.Contains(a.ClienteID) == true)
                                           .OrderByDescending(a => a.ActividadId)
                                           .Skip(iDisplayStart).Take(iDisplayLength).ToList();

                        Count = ((from a in db.Actividad
                                  join u in db.Usuarios on a.UsuarioIdElabora equals u.UsuarioId
                                  join c in db.Clientes on a.ClienteID equals c.ClienteID
                                  select new { a.ActividadId, a.ActividadFecha, a.ActividadEstado, a.ActividadTitulo, c.ClienteID, a.cliente.ClienteRazonSocial, a.UsuarioIdElabora })
                                           .Where(a => clienteList.Contains(a.ClienteID) == true)
                                           .OrderByDescending(a => a.ActividadId)
                                           .ToList()).Count;
                    }//if (!string.IsNullOrEmpty(sSearch))
                }
                catch// (Exception e)
                {
                    ActividadesList = new List<Actividad>();
                }
            }
            else
            {
                ActividadesList = new List<Actividad>();
            }//if (HttpContext.Current.Session != null)







            var CustomerPaged = new SysDataTablePager();

            CustomerPaged.draw = sEcho;
            CustomerPaged.recordsTotal = Count;
            CustomerPaged.recordsFiltered = Count;
            CustomerPaged.data = ActividadesList;

            return CustomerPaged;
        }

        [HttpGet]
        public async Task<IHttpActionResult> MarcaDescripcion(string id)
        {
            try
            {
                return Ok(await db.Marca.Where(a => a.Id.ToString() == id).Select(a => a.Nombre).FirstOrDefaultAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetActivities()
        {
            try
            {
                return Ok(db.Database.SqlQuery<dynamic>("SELECT * FROM ActividadesView").ToListAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
