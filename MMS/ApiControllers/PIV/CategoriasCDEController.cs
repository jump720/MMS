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

namespace MMS.ApiControllers.PIV
{
    public class CategoriasCDEController : ApiBaseController
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

                int count = await db.CategoriaCDE
                    .Where(c => c.LiquidacionId == null && c.Nombre.Contains(search))
                    .CountAsync();

                var data = await db.CategoriaCDE
                    .Where(c => c.LiquidacionId == null && c.Nombre.Contains(search))
                    .Select(c => new { c.Id, c.Nombre, c.ValorMinimo, c.ValorMaximo, c.Porcentaje, c.Icon })
                    .OrderBy(c => c.Id)
                    .Skip(displayStart).Take(displayLength).ToListAsync();

                data.Add(new
                {
                    Id = 0,
                    Nombre = "PIP",
                    ValorMinimo = 0m,
                    ValorMaximo = 0m,
                    Porcentaje = await db.Configuracion.Select(c => c.ConfigPorcentajePIV).FirstOrDefaultAsync(),
                    Icon = null as string
                });

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

        [HttpGet]
        [ApiAuthorizeAction("CategoriasCDE/Index")]
        public async Task<IHttpActionResult> GetPIPPercent()
        {
            try
            {
                return Ok(await db.Configuracion.Select(c => c.ConfigPorcentajePIV).FirstOrDefaultAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [ApiAuthorizeAction("CategoriasCDE/Index")]
        public async Task<IHttpActionResult> PostPIPPercent(float value)
        {
            try
            {
                var conf = await db.Configuracion.FirstOrDefaultAsync();
                if (conf == null)
                    return NotFound();

                conf.ConfigPorcentajePIV = value;
                db.Entry(conf).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("CategoriasCDE/EditPIVBase", "", new { Porcentaje = value });

                return Ok();
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
