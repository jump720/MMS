using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers.Transacciones
{
    public class NivelesAprobacionController : ApiBaseController
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
                string search = form["search[value]"];

                int count = await db.NivelesAprobacion
                    .Where(na => na.Descripcion.Contains(search) || na.canal.CanalDesc.Contains(search) || na.planta.PlantaDesc.Contains(search) || na.usuario.UsuarioNombre.Contains(search))
                    .CountAsync();

                var data = await db.NivelesAprobacion
                    .Where(na => na.Descripcion.Contains(search) || na.canal.CanalDesc.Contains(search) || na.planta.PlantaDesc.Contains(search) || na.usuario.UsuarioNombre.Contains(search))
                    .Select(na => new
                    {
                        na.Id,
                        na.Descripcion,
                        Canal = na.canal.CanalDesc,
                        Planta = na.planta.PlantaDesc,
                        na.Orden,
                        Usuario = na.usuario.UsuarioNombre
                    })
                    .OrderBy(na => na.Id)
                    .Skip(displayStart).Take(displayLength).ToListAsync();

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = data.OrderBy(c => c.Id)
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
