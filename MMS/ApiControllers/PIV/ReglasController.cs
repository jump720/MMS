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
    public class ReglasController : ApiBaseController
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

                int count = await db.Regla
                    .Include(r => r.Item)
                    .Include(r => r.Marca)
                    .Where(r => r.LiquidacionId == null && (
                        (r.ItemId != null && (r.Item.Codigo.Contains(search) || r.Item.Descripcion.Contains(search))) ||
                        (r.MarcaId != null && r.Marca.Nombre.Contains(search))
                    ))
                    .CountAsync();

                var data = await db.Regla
                    .Include(r => r.Item)
                    .Include(r => r.Item.Marca)
                    .Include(r => r.Marca)
                    .Where(r => r.LiquidacionId == null && (
                        (r.ItemId != null && (r.Item.Codigo.Contains(search) || r.Item.Descripcion.Contains(search))) ||
                        (r.MarcaId != null && r.Marca.Nombre.Contains(search))
                    ))
                    .Select(r => new
                    {
                        r.Id,
                        Tipo = r.ItemId != null ? ReglaViewModel.TipoRegla.Item.ToString() : ReglaViewModel.TipoRegla.Brand.ToString(),
                        Valor = r.ItemId != null ? "(" + r.Item.Marca.Nombre + ") " + r.Item.Codigo + " - " + r.Item.Descripcion : r.Marca.Nombre,
                        r.Meta,
                        r.Porcentaje,
                        r.Activa
                    })
                    .OrderBy(r => r.Id)
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
