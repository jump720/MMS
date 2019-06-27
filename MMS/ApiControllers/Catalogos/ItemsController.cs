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

namespace MMS.ApiControllers.Catalogos
{
    public class ItemsController : ApiBaseController
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

                int count = await db.Item
                    .Include(i => i.Marca)
                    .Where(i => i.Descripcion.Contains(search) || i.Codigo.ToString().Contains(search) || i.Marca.Nombre.ToString().Contains(search))
                    .CountAsync();

                var data = await db.Item
                    .Include(i => i.Marca)
                    .Where(i => i.Descripcion.Contains(search) || i.Codigo.ToString().Contains(search) || i.Marca.Nombre.ToString().Contains(search))
                    .Select(i => new { i.Id, i.Codigo, i.Descripcion, Marca = i.Marca.Nombre, i.UnidadEmpaque, i.PrecioSugerido })
                    .OrderBy(i => i.Id)
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
