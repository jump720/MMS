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

namespace MMS.ApiControllers.WCSS
{
    public class TipoDocSoporteController : ApiBaseController
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

                int count = await db.TipoDocSoporte
                    .Where(m => m.Nombre.Contains(search) || m.Id.ToString().Contains(search))
                    .CountAsync();

                var data = await db.TipoDocSoporte
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
        public async Task<IHttpActionResult> Create(TipoDocSoporte model)
        {
            try
            {
                db.TipoDocSoporte.Add(model);
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
        public async Task<IHttpActionResult> Edit(TipoDocSoporte model)
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
                var tipo = await db.TipoDocSoporte.FindAsync(id);
                if (tipo == null)
                    return NotFound();

                db.TipoDocSoporte.Remove(tipo);
                await db.SaveChangesAsync();
                AddLog("", tipo.Id, tipo);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        public async Task<IHttpActionResult> BuscarTipoDocSoporte(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<TipoDocSoporte>());

                return Ok(await db.TipoDocSoporte
                   .Where(i => (i.Nombre.Contains(q)))
                    .Select(i => new
                    {
                        i.Id,
                        i.Nombre
                    })
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
        public async Task<IHttpActionResult> GetTipoDocSoporte(int id)
        {
            try
            {

                return Ok(await db.TipoDocSoporte
                   .Where(i => i.Id == id)
                    .Select(i => new
                    {
                        i.Id,
                        i.Nombre
                    })
                    .FirstOrDefaultAsync()
                    );
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
