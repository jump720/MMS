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

namespace MMS.ApiControllers.Transacciones
{
    public class MovimientosController : ApiBaseController
    {
        private MMSContext db = new MMSContext();

        [HttpPost]
        [ApiAuthorizeAction]
        public IHttpActionResult Index(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string search = form["search[value]"];

                dynamic movimientoList;
                var count = 0;

                if (!string.IsNullOrEmpty(search))
                {

                    movimientoList = (from m in db.Movimiento
                                      join t in db.TipoMovimientos on m.TipoMovimientoID equals t.TipoMovimientoID
                                      join u in db.Usuarios on m.UsuarioIdModifica equals u.UsuarioId
                                      select new { m.MovimientoId, t.TipoMovimientoDesc, m.MovimientoFechaCrea, u.UsuarioNombre, m.OrdenId })
                                        .Distinct()
                                        .ToList().Where(a => a.MovimientoId.ToString().Contains(search) ||
                                                 (a.MovimientoFechaCrea != null && a.MovimientoFechaCrea.ToString("d").ToLower().Contains(search)) ||
                                                 (!string.IsNullOrEmpty(a.TipoMovimientoDesc) && a.TipoMovimientoDesc.ToLower().Contains(search)) ||
                                                 (!string.IsNullOrEmpty(a.UsuarioNombre) && a.UsuarioNombre.ToLower().Contains(search))
                                     )
                                     .OrderByDescending(m => m.MovimientoId)
                                     .Skip(displayStart).Take(displayLength).ToList()
                                     .ToList();
                    count = ((from m in db.Movimiento
                              join t in db.TipoMovimientos on m.TipoMovimientoID equals t.TipoMovimientoID
                              join u in db.Usuarios on m.UsuarioIdModifica equals u.UsuarioId
                              select new { m.MovimientoId, t.TipoMovimientoDesc, m.MovimientoFechaCrea, u.UsuarioNombre, m.OrdenId })
                                        .Distinct()
                                        .ToList().Where(a => a.MovimientoId.ToString().Contains(search) ||
                                                 (a.MovimientoFechaCrea != null && a.MovimientoFechaCrea.ToString("d").ToLower().Contains(search)) ||
                                                 (!string.IsNullOrEmpty(a.TipoMovimientoDesc) && a.TipoMovimientoDesc.ToLower().Contains(search)) ||
                                                 (!string.IsNullOrEmpty(a.UsuarioNombre) && a.UsuarioNombre.ToLower().Contains(search))
                                     )
                                     .OrderByDescending(m => m.MovimientoId)
                                     .ToList()).Count;
                }
                else
                {
                    movimientoList = (from m in db.Movimiento
                                      join t in db.TipoMovimientos on m.TipoMovimientoID equals t.TipoMovimientoID
                                      join u in db.Usuarios on m.UsuarioIdModifica equals u.UsuarioId
                                      select new { m.MovimientoId, t.TipoMovimientoDesc, m.MovimientoFechaCrea, u.UsuarioNombre, m.OrdenId })
                                    .Distinct()
                                    .OrderByDescending(m => m.MovimientoId)
                                    .Skip(displayStart).Take(displayLength).ToList()
                                    .ToList();
                    count = ((from m in db.Movimiento
                              join t in db.TipoMovimientos on m.TipoMovimientoID equals t.TipoMovimientoID
                              join u in db.Usuarios on m.UsuarioIdModifica equals u.UsuarioId
                              select new { m.MovimientoId, t.TipoMovimientoDesc, m.MovimientoFechaCrea, u.UsuarioNombre, m.OrdenId })
                                    .Distinct()
                                    .OrderByDescending(m => m.MovimientoId)
                                    //.Skip(iDisplayStart).Take(iDisplayLength).ToList()
                                    .ToList()).Count;

                }

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = movimientoList
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
