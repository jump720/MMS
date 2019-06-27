using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using MMS.Filters;
using MMS.Models;

namespace MMS.ApiControllers.PerfectStore
{
    public class TipoVisitaController : ApiBaseController
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

                int count = await db.TipoVisitas
                    .Where(m => m.Nombre.Contains(search) || m.Id.ToString().Contains(search))
                    .CountAsync();

                var data = await db.TipoVisitas
                    .Where(m => m.Nombre.Contains(search) || m.Id.ToString().Contains(search))
                    .Select(m => new { m.Id, m.Nombre })
                    .OrderBy(m => m.Id)
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
        public async Task<IHttpActionResult> Create(TipoVisita model)
        {
            try
            {
                db.TipoVisitas.Add(model);
                await db.SaveChangesAsync();
                AddLog("", model.Id, model);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(TipoVisita model)
        {
            try
            {
                db.Entry(model).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", model.Id, model);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {
                var tipo = await db.TipoVisitas.FindAsync(id);
                if (tipo == null)
                    return NotFound();

                db.TipoVisitas.Remove(tipo);
                await db.SaveChangesAsync();
                AddLog("", tipo.Id, tipo);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }


}
