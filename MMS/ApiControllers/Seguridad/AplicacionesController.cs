using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using MMS.Filters;
using MMS.Models;

namespace MMS.ApiControllers.Seguridad
{
    public class AplicacionesController : ApiBaseController
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

                int count = await db.Aplicaciones
                    .Where(a => a.Nombre.Contains(search) || a.Id.ToString().Contains(search))
                    .CountAsync();

                var data = await db.Aplicaciones
                    .Where(a => a.Nombre.Contains(search) || a.Id.ToString().Contains(search))
                    .Select(a => new { a.Id, a.Nombre, a.Link, a.Activo })
                    .OrderBy(a => a.Id)
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

    }
}
