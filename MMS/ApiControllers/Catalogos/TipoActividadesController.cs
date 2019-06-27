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
    public class TipoActividadesController : ApiBaseController
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

                int count = await db.TipoActividades
                    .Where(ta => ta.TipoActividadID.Contains(search) || ta.TipoActividadDesc.Contains(search))
                    .CountAsync();

                var data = await db.TipoActividades
                    .Where(ta => ta.TipoActividadID.Contains(search) || ta.TipoActividadDesc.Contains(search))
                    .Select(ta => new { ta.TipoActividadID, ta.TipoActividadDesc })
                    .OrderBy(ta => ta.TipoActividadID)
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
        public async Task<IHttpActionResult> Create(TipoActividad tipoActividad)
        {
            try
            {
                var result = new AjaxResult();

                var tipoActividadTemp = await db.TipoActividades.Where(u => u.TipoActividadID == tipoActividad.TipoActividadID).FirstOrDefaultAsync();
                if (tipoActividadTemp == null)
                {
                    db.TipoActividades.Add(tipoActividad);
                    await db.SaveChangesAsync();
                    AddLog("", tipoActividad.TipoActividadID, tipoActividad);
                }
                else
                {
                    return Ok(result.False("Type of activity already exists"));
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
        public async Task<IHttpActionResult> Edit(TipoActividad tipoActividad)
        {
            try
            {
                var result = new AjaxResult();

                db.Entry(tipoActividad).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", tipoActividad.TipoActividadID, tipoActividad);

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
                var tipoActividad = await db.TipoActividades.FindAsync(id);
                if (tipoActividad == null)
                    return NotFound();

                var actividad = await db.Actividad.Where(g => g.TipoActividadID == id).ToListAsync();

                if (actividad.Count == 0)
                {
                    db.TipoActividades.Remove(tipoActividad);
                    await db.SaveChangesAsync();
                    AddLog("", tipoActividad.TipoActividadID, tipoActividad);
                }
                else
                {
                    return Ok(result.False("Type of activity is related with some records"));
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
