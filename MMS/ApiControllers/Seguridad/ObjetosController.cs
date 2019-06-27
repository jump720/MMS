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
namespace MMS.ApiControllers.Seguridad
{
    public class ObjetosController : ApiBaseController
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

                int count = await db.Objeto
                    .Where(a => a.ObjetoDesc.Contains(search) || a.ObjetoId.ToString().Contains(search))
                    .CountAsync();

                var data = await db.Objeto
                    .Where(a => a.ObjetoDesc.Contains(search) || a.ObjetoId.ToString().Contains(search))
                    .Select(o => new { o.ObjetoId, o.ObjetoDesc})
                    .OrderBy(a => a.ObjetoId)
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
