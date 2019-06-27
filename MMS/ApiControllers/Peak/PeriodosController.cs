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

namespace MMS.ApiControllers.Peak
{
    public class PeriodosController : ApiBaseController
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

                int count = await db.Periodo
                    .Where(p => p.Descripcion.Contains(search))
                    .CountAsync();

                var data = (await db.Periodo
                    .Where(p => p.Descripcion.Contains(search))
                    .Select(p => new
                    {
                        p.Id,
                        p.Descripcion,
                        p.FechaIni,
                        p.FechaFin,
                    })
                    .OrderByDescending(p => p.FechaIni)
                    .Skip(displayStart).Take(displayLength).ToListAsync())
                    .Select(p => new
                    {
                        p.Id,
                        p.Descripcion,
                        FechaIni = p.FechaIni.ToShortDateString(),
                        FechaFin = p.FechaFin.ToShortDateString()
                    }).ToList();

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
