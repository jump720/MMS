using MMS.Classes;
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

namespace MMS.ApiControllers.PerfectStore
{
    public class PlantillasController : ApiBaseController
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

                int count = await db.Plantilla
                    .Where(t => t.Nombre.Contains(search))
                    .CountAsync();

                var data = await db.Plantilla
                    .Select(t => new { t.Id, t.Nombre, t.Activa })
                    .Where(t => t.Nombre.Contains(search))
                    .OrderBy(t => t.Id)
                    .Skip(displayStart).Take(displayLength).ToListAsync();

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> BuscarItem(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Item>());

                return Ok(await db.Item
                    .Include(i => i.Marca)
                    .Where(i => (i.Codigo.Contains(q) || i.Descripcion.Contains(q)))
                    .Select(i => new { i.Id, Marca = i.Marca.Nombre, i.Codigo, i.Categoria, i.Grupo, i.Descripcion, i.PrecioSugerido, UnidadEmpaque = (i.UnidadEmpaque != null) ? i.UnidadEmpaque : 0 })
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
        public async Task<IHttpActionResult> GetItem(int id)
        {
            try
            {
                return Ok(await db.Item
                    .Include(i => i.Marca)
                    .Where(i => i.Id == id)
                    .Select(i => new { i.Id, Marca = i.Marca.Nombre, i.Codigo, i.Categoria, i.Grupo, i.Descripcion, i.PrecioSugerido, UnidadEmpaque = (i.UnidadEmpaque != null) ? i.UnidadEmpaque : 0 })
                    .FirstOrDefaultAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> ActualizarFechaActualizacion()
        {
            try
            {
                var conf = await db.Configuracion.FirstOrDefaultAsync();
                conf.ConfigFechaActualizacionDatosBase = DateTime.Now;
                db.Entry(conf).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return Ok(new AjaxResult());
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
