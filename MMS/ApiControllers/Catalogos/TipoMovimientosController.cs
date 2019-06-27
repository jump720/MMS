using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using MMS.Filters;
using MMS.Models;
using MMS.Classes;
namespace MMS.ApiControllers.Catalogos
{
    public class TipoMovimientosController : ApiBaseController
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

                int count = await db.TipoMovimientos
                    .Where(tg => tg.TipoMovimientoID.Contains(search) || tg.TipoMovimientoDesc.Contains(search))
                    .CountAsync();

                var data = await db.TipoMovimientos
                    .Where(tg => tg.TipoMovimientoID.Contains(search) || tg.TipoMovimientoDesc.Contains(search))
                    .Select(cc => new { cc.TipoMovimientoID, cc.TipoMovimientoDesc })
                    .OrderBy(cc => cc.TipoMovimientoID)
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


        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Create(TipoMovimiento tipoMovimiento)
        {
            try
            {
                var result = new AjaxResult();

                var tipoMovTemp = await db.TipoActividades.Where(u => u.TipoActividadID == tipoMovimiento.TipoMovimientoID).FirstOrDefaultAsync();
                if (tipoMovTemp == null)
                {
                    db.TipoMovimientos.Add(tipoMovimiento);
                    await db.SaveChangesAsync();
                    AddLog("", tipoMovimiento.TipoMovimientoID, tipoMovimiento);
                }
                else
                {
                    return Ok(result.False("Type of movements already exists"));
                }


                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(TipoMovimiento tipoMovimiento)
        {
            try
            {
                var result = new AjaxResult();

                db.Entry(tipoMovimiento).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", tipoMovimiento.TipoMovimientoID, tipoMovimiento);

                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Delete(string id)
        {
            try
            {
                var result = new AjaxResult();
                var tipoMov = await db.TipoMovimientos.FindAsync(id);
                if (tipoMov == null)
                    return NotFound();

                var movimiento = await db.Movimiento.Where(g => g.TipoMovimientoID == id).ToListAsync();

                if (movimiento.Count == 0)
                {
                    db.TipoMovimientos.Remove(tipoMov);
                    await db.SaveChangesAsync();
                    AddLog("", tipoMov.TipoMovimientoID, tipoMov);
                }
                else
                {
                    return Ok(result.False("Type of movement is related with some records"));
                }


                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
